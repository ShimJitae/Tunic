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
        base.Awake();

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
        base.Start();
        SubscribeInput();
    }

    protected override void OnEnable()
    {
        if (locomotionFsm != null)
            locomotionFsm.SetStartState(PlayerLocomotionStateId.Idle);

        base.OnEnable();

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

        base.OnDisable();
    }

    protected override StateBase<EntityLifeStateId> CreateAliveState()
    {
        CreateLocomotionStateMachine();
        CreateCombatStateMachine();

        aliveFsm = new StateMachine<EntityLifeStateId, PlayerAliveStateId, PlayerStateEvent>();

        aliveFsm.StateChanged += _ =>
            OnAliveStateEntered?.Invoke(aliveFsm.ActiveStateName);

        aliveFsm.AddState(PlayerAliveStateId.Locomotion, locomotionFsm);
        aliveFsm.AddState(PlayerAliveStateId.Combat, combatFsm);
        aliveFsm.AddState(
            PlayerAliveStateId.Hit,
            new PlayerHitState(animationModule, attackModule));

        aliveFsm.SetStartState(PlayerAliveStateId.Locomotion);

        aliveFsm.AddTriggerTransition(
            PlayerStateEvent.AttackRequested,
            PlayerAliveStateId.Locomotion,
            PlayerAliveStateId.Combat);

        aliveFsm.AddTriggerTransitionFromAny(
            PlayerStateEvent.Damaged,
            PlayerAliveStateId.Hit,
            forceInstantly: true);

        aliveFsm.AddTransition(
            PlayerAliveStateId.Combat,
            PlayerAliveStateId.Locomotion);

        aliveFsm.AddTransition(
            PlayerAliveStateId.Hit,
            PlayerAliveStateId.Locomotion,
            _ => animationModule.IsAnimationComplete(animationModule.Hit));

        return aliveFsm;
    }

    protected override StateBase<EntityLifeStateId> CreateDeadState()
    {
        return new PlayerDeadState(
            moveModule,
            animationModule,
            attackModule,
            Health);
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
        locomotionFsm = new StateMachine<
            PlayerAliveStateId,
            PlayerLocomotionStateId,
            PlayerStateEvent>(rememberLastState: true);

        locomotionFsm.StateChanged += _ =>
            OnLocomotionStateEntered?.Invoke(locomotionFsm.ActiveStateName);

        locomotionFsm.AddState(
            PlayerLocomotionStateId.Idle,
            new PlayerIdleState(moveModule, animationModule));
        locomotionFsm.AddState(
            PlayerLocomotionStateId.Move,
            new PlayerMoveState(moveModule, animationModule, GetMoveInput));
        locomotionFsm.AddState(
            PlayerLocomotionStateId.Dodge,
            new PlayerDodgeState(
                moveModule,
                animationModule,
                Health,
                GetMoveInput));

        locomotionFsm.SetStartState(PlayerLocomotionStateId.Idle);

        locomotionFsm.AddTransition(
            PlayerLocomotionStateId.Idle,
            PlayerLocomotionStateId.Move,
            _ => HasMoveInput);
        locomotionFsm.AddTransition(
            PlayerLocomotionStateId.Move,
            PlayerLocomotionStateId.Idle,
            _ => !HasMoveInput);

        locomotionFsm.AddTriggerTransition(
            PlayerStateEvent.DodgeRequested,
            PlayerLocomotionStateId.Idle,
            PlayerLocomotionStateId.Dodge);
        locomotionFsm.AddTriggerTransition(
            PlayerStateEvent.DodgeRequested,
            PlayerLocomotionStateId.Move,
            PlayerLocomotionStateId.Dodge);

        locomotionFsm.AddTransition(
            PlayerLocomotionStateId.Dodge,
            PlayerLocomotionStateId.Move,
            _ => animationModule.IsAnimationComplete(animationModule.Dodge)
                && HasMoveInput);
        locomotionFsm.AddTransition(
            PlayerLocomotionStateId.Dodge,
            PlayerLocomotionStateId.Idle,
            _ => animationModule.IsAnimationComplete(animationModule.Dodge)
                && !HasMoveInput);
    }

    private void CreateCombatStateMachine()
    {
        combatFsm = new StateMachine<
            PlayerAliveStateId,
            PlayerCombatStateId,
            PlayerStateEvent>(needsExitTime: true);

        combatFsm.StateChanged += _ =>
            OnCombatStateEntered?.Invoke(combatFsm.ActiveStateName);

        combatFsm.AddState(
            PlayerCombatStateId.Attack1,
            new PlayerAttackState(
                PlayerCombatStateId.Attack1,
                moveModule,
                animationModule,
                attackModule,
                GetMoveInput));
        combatFsm.AddState(
            PlayerCombatStateId.Attack2,
            new PlayerAttackState(
                PlayerCombatStateId.Attack2,
                moveModule,
                animationModule,
                attackModule,
                GetMoveInput));
        combatFsm.AddState(
            PlayerCombatStateId.Attack3,
            new PlayerAttackState(
                PlayerCombatStateId.Attack3,
                moveModule,
                animationModule,
                attackModule,
                GetMoveInput));

        combatFsm.SetStartState(PlayerCombatStateId.Attack1);

        combatFsm.AddTransition(
            PlayerCombatStateId.Attack1,
            PlayerCombatStateId.Attack2,
            _ => IsAttackComplete(PlayerCombatStateId.Attack1)
                && IsComboQueuedFrom(PlayerCombatStateId.Attack1));
        combatFsm.AddTransition(
            PlayerCombatStateId.Attack2,
            PlayerCombatStateId.Attack3,
            _ => IsAttackComplete(PlayerCombatStateId.Attack2)
                && IsComboQueuedFrom(PlayerCombatStateId.Attack2));

        combatFsm.AddExitTransition(
            PlayerCombatStateId.Attack1,
            _ => IsAttackComplete(PlayerCombatStateId.Attack1)
                && !IsComboQueuedFrom(PlayerCombatStateId.Attack1));
        combatFsm.AddExitTransition(
            PlayerCombatStateId.Attack2,
            _ => IsAttackComplete(PlayerCombatStateId.Attack2)
                && !IsComboQueuedFrom(PlayerCombatStateId.Attack2));
        combatFsm.AddExitTransition(
            PlayerCombatStateId.Attack3,
            _ => IsAttackComplete(PlayerCombatStateId.Attack3));
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
            || !TrySpendAttackStamina())
        {
            return;
        }

        queuedComboFrom = currentAttack;
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
        return animationModule.IsAnimationComplete(
            animationModule.GetAttackStateHash(attackState));
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
