using System;
using UnityEngine;
using UnityHFSM;

public abstract class EntityController : MonoBehaviour
{
    protected StateMachine<EntityStateId, EntityEvent> Fsm { get; private set; }

    // 제곱근 연산을 피하고 벡터의 크기만 비교하기 위해 sqrMagnitude를 사용
    // MoveInfo가 Vector3.zero가 아니라 이동 방향 / 이동할 위치 등에 대한 정보가 있는 경우에만 Move, 그 외에 Idle
    public virtual bool HasMoveInput => MoveModule.MoveInfo.sqrMagnitude > 0.01f;
    public virtual bool ShouldExitMoveState => !HasMoveInput;
    public virtual bool CanAttackFromMoveState => true;
    public IMoveStrategy MoveModule { get; protected set; }
    public IAnimationModule AnimationModule { get; protected set; }
    public IAttackStrategy AttackModule { get; protected set; }

    public EntityStateId CurrentState => Fsm.ActiveStateName;

    [SerializeField] private Health health;
    public Health Health => health;

    protected virtual void Awake()
    {
        AnimationModule = gameObject.GetComponentInChildren<IAnimationModule>();
        if (AnimationModule == null)
        {
            Debug.LogError("해당 EntityController의 하위 오브젝트에 EntityAnimationModule가 없습니다.");
        }

        AttackModule = gameObject.GetComponentInChildren<IAttackStrategy>();
        if (AttackModule == null)
        {
            Debug.LogError("해당 EntityController의 하위 오브젝트에 AttackModule가 없습니다.");
        }

        if (!gameObject.TryGetComponent(out health))
        {
            Debug.LogError("해당 EntityController에 Health가 없습니다.");
        }
    }

    protected virtual void Start()
    {
        CreateStateMachine();
    }

    protected virtual void OnEnable()
    {
        health.OnDamaged += NotifyDamaged;
        health.OnDied += NotifyDied;

        AttackModule.OnAttack += RequestAttack;
    }

    protected virtual void OnDisable()
    {
        health.OnDamaged -= NotifyDamaged;
        health.OnDied -= NotifyDied;

        AttackModule.OnAttack -= RequestAttack;
    }

    protected virtual void Update()
    {
        Fsm.OnLogic();
    }

    protected abstract void SetUpEntityData();

    private void CreateStateMachine()
    {
        Fsm = new StateMachine<EntityStateId, EntityEvent>();
        Fsm.StateChanged += state =>
{
    Debug.Log($"State Changed -> {state.name}");
};

        // 2. State 등록
        RegisterStates();

        // 3. 시작 State
        Fsm.SetStartState(EntityStateId.Idle);

        // 4. Transition 등록
        RegisterTransitions();

        // 5. 초기화
        Fsm.Init();
    }

    protected virtual void RegisterStates()
    {
        Fsm.AddState(EntityStateId.Idle, new IdleState(this));
        Fsm.AddState(EntityStateId.Move, CreateMoveState());
        Fsm.AddState(EntityStateId.Attack, new AttackState(this));
        Fsm.AddState(EntityStateId.Hit, new HitState(this));
        Fsm.AddState(EntityStateId.Die, new DieState(this));
    }

    // 이동 상태의 경우, Player와 Enemy는 CreateMoveState()를 오버라이드하여 자식 FSM을 반환한다.
    protected abstract StateBase<EntityStateId> CreateMoveState();

    protected virtual void RegisterTransitions()
    {
        Fsm.AddTransition(new IdleToMoveTransition(this));
        Fsm.AddTransition(new MoveToIdleTransition(this));

        RegisterAttackTransition();

        RegisterHitTransition();

        Fsm.AddTriggerTransitionFromAny(EntityEvent.Died, new DiedToDeadTransition(this)); // 사망 트랜지션
    }

    private void RegisterAttackTransition()
    {
        Fsm.AddTriggerTransition(EntityEvent.Attack, new IdleToAttackTransition(this)); // idle -> attack
        Fsm.AddTriggerTransition(EntityEvent.Attack, new MoveToAttackTransition(this)); // move -> attack
        Fsm.AddTriggerTransition(EntityEvent.AttackFinished, new AttackToMoveTransition(this)); // attack -> move
        Fsm.AddTriggerTransition(EntityEvent.AttackFinished, new AttackToIdleTransition(this)); // attack -> idle
    }

    private void RegisterHitTransition()
    {
        Fsm.AddTriggerTransitionFromAny(EntityEvent.Damaged, new DamagedToHitTransition(this)); // 피격 트랜지션
        Fsm.AddTriggerTransition(EntityEvent.HitFinished, new HitToIdleTransition(this)); // 피격 후 복귀 트랜지션
    }

    // =========================================================
    // Event API
    // =========================================================

    // 공격 요청

    /// <summary>
    /// AttackModule / AnimationEvent 등에서 호출
    /// </summary>
    public virtual void RequestAttack()
    {
        Fsm.Trigger(EntityEvent.Attack);
    }

    public void NotifyAttackFinished()
    {
        Fsm.Trigger(EntityEvent.AttackFinished);
    }

    /// <summary>
    /// Health 등의 외부 모듈에서 호출
    /// </summary>
    public void NotifyDamaged(float _)
    {
        Fsm.Trigger(EntityEvent.Damaged);
    }

    public void NotifyHitFinished()
    {
        Fsm.Trigger(EntityEvent.HitFinished);
    }

    public void NotifyDied()
    {
        Fsm.Trigger(EntityEvent.Died);
    }
}
