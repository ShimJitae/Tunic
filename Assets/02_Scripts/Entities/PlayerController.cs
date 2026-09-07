using System;
using UnityEngine;
using UnityHFSM;

[RequireComponent(typeof(PlayerMoveModule))]
[RequireComponent(typeof(PlayerAttackModule))]
[RequireComponent(typeof(Status))]
public class PlayerController : EntityController
{
    [SerializeField] private DataSetUp_Player dataSetUp;

    private PlayerMoveModule moveModule;
    private PlayerAnimationModule animationModule;
    private PlayerAttackModule attackModule;
    private InputManager inputManager;

    private StateMachine<EntityLifeStateId, PlayerAliveStateId, PlayerStateEvent> aliveFsm;
    private StateMachine<PlayerAliveStateId, PlayerLocomotionStateId, PlayerStateEvent> locomotionFsm;
    private StateMachine<PlayerAliveStateId, PlayerCombatStateId, PlayerStateEvent> combatFsm;
    private PlayerCombatStateId? queuedComboFrom;

    public event Action<PlayerAliveStateId> OnAliveStateEntered;
    public event Action<PlayerLocomotionStateId> OnLocomotionStateEntered;
    public event Action<PlayerCombatStateId> OnCombatStateEntered;

    private Status PlayerStatus => Health as Status;

    public PlayerAliveStateId CurrentAliveState => aliveFsm.ActiveStateName;
    public PlayerLocomotionStateId CurrentLocomotionState => locomotionFsm.ActiveStateName;
    public PlayerCombatStateId CurrentCombatState => combatFsm.ActiveStateName;

    public bool IsDodging => IsAlive
        && aliveFsm.ActiveStateName == PlayerAliveStateId.Locomotion
        && locomotionFsm.IsInitialized
        && locomotionFsm.ActiveStateName == PlayerLocomotionStateId.Dodge;

    private bool HasMoveInput => GetMoveInput().sqrMagnitude > 0.01f;

    protected override void Awake()
    {
        PlayerController[] players = FindObjectsByType<PlayerController>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        if (players.Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        base.Awake();

        // 기존 컴포넌트 초기화 코드
        if (!TryGetComponent(out moveModule))
        {
            Debug.LogError(
                $"{nameof(PlayerController)} requires a {nameof(PlayerMoveModule)} component.",
                this);

            enabled = false;
        }

        if (!TryGetComponent(out attackModule))
        {
            Debug.LogError(
                $"{nameof(PlayerController)} requires a {nameof(PlayerAttackModule)} component.",
                this);

            enabled = false;
        }

        animationModule = GetComponentInChildren<PlayerAnimationModule>();

        if (animationModule == null)
        {
            Debug.LogError(
                $"{nameof(PlayerController)} requires a {nameof(PlayerAnimationModule)} component.",
                this);

            enabled = false;
        }

        if (dataSetUp == null && !TryGetComponent(out dataSetUp))
        {
            Debug.LogError(
                $"{nameof(PlayerController)} requires a {nameof(DataSetUp_Player)} component.",
                this);

            enabled = false;
        }
    }

    protected override void Start()
    {
        if (!enabled)
            return;

        dataSetUp.SetUpData();
        base.Start(); // CreateLifeStateMachine(); 실행
        SubscribeInput();
    }

    protected override void OnEnable()
    {
        if (locomotionFsm != null)
            locomotionFsm.SetStartState(PlayerLocomotionStateId.Idle);

        base.OnEnable(); // health.OnDied += HandleDied, EnterInitialLifeState()

        if (Health != null)
            Health.OnDamaged += HandleDamaged;

        SubscribeInput();
    }

    protected override void OnDisable()
    {
        if (Health != null)
            Health.OnDamaged -= HandleDamaged;

        UnsubscribeInput();
        queuedComboFrom = null;

        if (moveModule != null)
        {
            moveModule.Stop();
            moveModule.CancelDodge();
        }

        base.OnDisable();  // health.OnDied += HandleDied, lifeFsm.OnExit();
    }

    protected override StateBase<EntityLifeStateId> CreateAliveState()
    {
        CreateLocomotionStateMachine();
        CreateCombatStateMachine();

        aliveFsm = new StateMachine<EntityLifeStateId, PlayerAliveStateId, PlayerStateEvent>();

        aliveFsm.StateChanged += _ => OnAliveStateEntered?.Invoke(aliveFsm.ActiveStateName);

        aliveFsm.AddState(PlayerAliveStateId.Locomotion, locomotionFsm);
        aliveFsm.AddState(PlayerAliveStateId.Combat, combatFsm);
        aliveFsm.AddState(PlayerAliveStateId.Hit, new PlayerHitState(animationModule, attackModule));

        aliveFsm.SetStartState(PlayerAliveStateId.Locomotion);

        // Locomotion -> Combat, 공격 요청 시
        aliveFsm.AddTriggerTransition(PlayerStateEvent.AttackRequested, PlayerAliveStateId.Locomotion, PlayerAliveStateId.Combat);

        // Any -> Hit, 피격 시 즉시
        aliveFsm.AddTriggerTransitionFromAny(PlayerStateEvent.Damaged, PlayerAliveStateId.Hit, forceInstantly: true);

        // Combat -> Locomotion, Combat 하위 FSM 종료 시
        aliveFsm.AddTransition(PlayerAliveStateId.Combat, PlayerAliveStateId.Locomotion);

        // Hit -> Locomotion, 피격 애니메이션 종료 시
        aliveFsm.AddTransition(PlayerAliveStateId.Hit, PlayerAliveStateId.Locomotion, _ => animationModule.IsAnimationComplete(animationModule.Hit));

        return aliveFsm;
    }

    protected override StateBase<EntityLifeStateId> CreateDeadState()
    {
        return new PlayerDeadState(moveModule, animationModule, attackModule, Health);
    }

    public void RequestAttack()
    {
        if (!IsAlive || aliveFsm == null || !aliveFsm.IsInitialized)
            return;

        if (aliveFsm.ActiveStateName == PlayerAliveStateId.Combat)
        {
            TryQueueComboAttack();
            return;
        }

        if (!CanStartLocomotionAction())
            return;

        if (!TrySpendAttackStamina())
            return;

        queuedComboFrom = null;
        aliveFsm.Trigger(PlayerStateEvent.AttackRequested);
    }

    public void RequestDodge()
    {
        if (!CanStartLocomotionAction())
            return;

        if (PlayerStatus == null
            || dataSetUp.PlayerData == null
            || !PlayerStatus.TakeStamina(dataSetUp.PlayerData.DodgeStaminaCost))
            return;

        aliveFsm.Trigger(PlayerStateEvent.DodgeRequested);
    }

    private void CreateLocomotionStateMachine()
    {
        locomotionFsm = new StateMachine<PlayerAliveStateId, PlayerLocomotionStateId, PlayerStateEvent>(rememberLastState: true);

        locomotionFsm.StateChanged += _ => OnLocomotionStateEntered?.Invoke(locomotionFsm.ActiveStateName);

        locomotionFsm.AddState(PlayerLocomotionStateId.Idle, new PlayerIdleState(moveModule, animationModule));
        locomotionFsm.AddState(PlayerLocomotionStateId.Move, new PlayerMoveState(moveModule, animationModule, GetMoveInput));
        locomotionFsm.AddState(PlayerLocomotionStateId.Dodge, new PlayerDodgeState(moveModule, animationModule, Health, GetMoveInput));

        locomotionFsm.SetStartState(PlayerLocomotionStateId.Idle);

        // Idle -> Move, 이동 입력이 있을 시
        locomotionFsm.AddTransition(PlayerLocomotionStateId.Idle, PlayerLocomotionStateId.Move, _ => HasMoveInput);

        // Move -> Idle, 이동 입력이 없을 시
        locomotionFsm.AddTransition(PlayerLocomotionStateId.Move, PlayerLocomotionStateId.Idle, _ => !HasMoveInput);

        // Idle -> Dodge, 회피 요청 시
        locomotionFsm.AddTriggerTransition(PlayerStateEvent.DodgeRequested, PlayerLocomotionStateId.Idle, PlayerLocomotionStateId.Dodge);

        // Move -> Dodge, 회피 요청 시
        locomotionFsm.AddTriggerTransition(PlayerStateEvent.DodgeRequested, PlayerLocomotionStateId.Move, PlayerLocomotionStateId.Dodge);

        // Dodge -> Move, 회피 애니메이션이 종료되고 이동 입력이 있을 시
        locomotionFsm.AddTransition(PlayerLocomotionStateId.Dodge, PlayerLocomotionStateId.Move, _ => animationModule.IsAnimationComplete(animationModule.Dodge) && HasMoveInput);

        // Dodge -> Idle, 회피 애니메이션이 종료되고 이동 입력이 없을 시
        locomotionFsm.AddTransition(PlayerLocomotionStateId.Dodge, PlayerLocomotionStateId.Idle, _ => animationModule.IsAnimationComplete(animationModule.Dodge) && !HasMoveInput);
    }

    private void CreateCombatStateMachine()
    {
        combatFsm = new StateMachine<PlayerAliveStateId, PlayerCombatStateId, PlayerStateEvent>(needsExitTime: true);

        combatFsm.StateChanged += _ => OnCombatStateEntered?.Invoke(combatFsm.ActiveStateName);

        combatFsm.AddState(PlayerCombatStateId.Attack1, new PlayerAttackState(PlayerCombatStateId.Attack1, moveModule, animationModule, attackModule, GetMoveInput));
        combatFsm.AddState(PlayerCombatStateId.Attack2, new PlayerAttackState(PlayerCombatStateId.Attack2, moveModule, animationModule, attackModule, GetMoveInput));
        combatFsm.AddState(PlayerCombatStateId.Attack3, new PlayerAttackState(PlayerCombatStateId.Attack3, moveModule, animationModule, attackModule, GetMoveInput));

        combatFsm.SetStartState(PlayerCombatStateId.Attack1);

        // Attack1 -> Attack2, Attack1이 종료되고 다음 콤보가 예약됐을 시
        combatFsm.AddTransition(PlayerCombatStateId.Attack1, PlayerCombatStateId.Attack2, _ => IsAttackComplete(PlayerCombatStateId.Attack1) && IsComboQueuedFrom(PlayerCombatStateId.Attack1));

        // Attack2 -> Attack3, Attack2가 종료되고 다음 콤보가 예약됐을 시
        combatFsm.AddTransition(PlayerCombatStateId.Attack2, PlayerCombatStateId.Attack3, _ => IsAttackComplete(PlayerCombatStateId.Attack2) && IsComboQueuedFrom(PlayerCombatStateId.Attack2));

        // Attack1 -> Combat 종료, Attack1이 종료되고 다음 콤보가 없을 시
        combatFsm.AddExitTransition(PlayerCombatStateId.Attack1, _ => IsAttackComplete(PlayerCombatStateId.Attack1) && !IsComboQueuedFrom(PlayerCombatStateId.Attack1));

        // Attack2 -> Combat 종료, Attack2가 종료되고 다음 콤보가 없을 시
        combatFsm.AddExitTransition(PlayerCombatStateId.Attack2, _ => IsAttackComplete(PlayerCombatStateId.Attack2) && !IsComboQueuedFrom(PlayerCombatStateId.Attack2));

        // Attack3 -> Combat 종료, Attack3가 종료됐을 시
        combatFsm.AddExitTransition(PlayerCombatStateId.Attack3, _ => IsAttackComplete(PlayerCombatStateId.Attack3));
    }

    private bool CanStartLocomotionAction()
    {
        if (!IsAlive
            || aliveFsm.ActiveStateName != PlayerAliveStateId.Locomotion
            || !locomotionFsm.IsInitialized)
        {
            return false;
        }

        PlayerLocomotionStateId locomotionState = locomotionFsm.ActiveStateName;
        return locomotionState == PlayerLocomotionStateId.Idle
            || locomotionState == PlayerLocomotionStateId.Move;
    }

    private void HandleDamaged(float _)
    {
        if (!IsAlive || Health.IsDied)
            return;

        queuedComboFrom = null;

        if (aliveFsm.ActiveStateName == PlayerAliveStateId.Hit)
        {
            aliveFsm.RequestStateChange(PlayerAliveStateId.Hit, forceInstantly: true);
            return;
        }

        aliveFsm.Trigger(PlayerStateEvent.Damaged);
    }

    private void TryQueueComboAttack()
    {
        if (combatFsm == null || !combatFsm.IsInitialized)
            return;

        PlayerCombatStateId currentAttack = combatFsm.ActiveStateName;
        if (currentAttack == PlayerCombatStateId.Attack3
            || IsComboQueuedFrom(currentAttack)
            || !IsComboInputWindowOpen(currentAttack)
            || !TrySpendAttackStamina())
        {
            return;
        }

        queuedComboFrom = currentAttack;
    }

    private bool IsComboInputWindowOpen(PlayerCombatStateId attackState)
    {
        if (animationModule == null
            || dataSetUp == null
            || dataSetUp.PlayerData == null)
        {
            return false;
        }

        int stateHash = animationModule.GetAttackStateHash(attackState);
        return animationModule.TryGetNormalizedTime(stateHash, out float normalizedTime)
            && normalizedTime >= dataSetUp.PlayerData.ComboInputStartTime
            && normalizedTime <= dataSetUp.PlayerData.ComboInputEndTime;
    }

    private bool TrySpendAttackStamina()
    {
        return PlayerStatus != null
            && dataSetUp.PlayerData != null
            && PlayerStatus.TakeStamina(dataSetUp.PlayerData.AttackStaminaCost);
    }

    private bool IsComboQueuedFrom(PlayerCombatStateId attackState)
    {
        return queuedComboFrom == attackState;
    }

    private bool IsAttackComplete(PlayerCombatStateId attackState)
    {
        return animationModule.IsAnimationComplete(animationModule.GetAttackStateHash(attackState));
    }

    private Vector3 GetMoveInput()
    {
        return InputManager.Instance != null
            ? InputManager.Instance.MoveInput
            : Vector3.zero;
    }

    private void SubscribeInput()
    {
        InputManager currentInputManager = InputManager.Instance;
        if (currentInputManager == null || currentInputManager == inputManager)
            return;

        UnsubscribeInput();

        inputManager = currentInputManager;
        inputManager.AttackPressed += RequestAttack;
        inputManager.DodgePressed += RequestDodge;
    }

    private void UnsubscribeInput()
    {
        if (inputManager == null)
            return;

        inputManager.AttackPressed -= RequestAttack;
        inputManager.DodgePressed -= RequestDodge;
        inputManager = null;
    }
}
