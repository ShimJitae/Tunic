using UnityEngine;
using UnityHFSM;

public abstract class EntityController : MonoBehaviour
{
    protected StateMachine<EntityStateId, EntityEvent> Fsm { get; private set; }

    public IMoveStrategy MoveModule { get; protected set; }
    // 제곱근 연산을 피하고 벡터의 크기만 비교하기 위해 sqrMagnitude를 사용
    // MoveInfo가 Vector3.zero가 아니라 이동 방향 / 이동할 위치 등에 대한 정보가 있는 경우에만 Move, 그 외에 Idle
    public virtual bool HasMoveInput => MoveModule.MoveInfo.sqrMagnitude > 0.01f;
    public IAttackStrategy AttackModule { get; protected set; }

    public EntityStateId CurrentState => Fsm.ActiveStateName;

    protected virtual void Awake()
    {
        CreateStateMachine();
    }

    protected virtual void Update()
    {
        Fsm.OnLogic();
    }

    private void CreateStateMachine()
    {
        Fsm = new StateMachine<EntityStateId, EntityEvent>();

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
        Fsm.AddState(EntityStateId.Move, new MoveState(this));
        Fsm.AddState(EntityStateId.Attack, new AttackState(this));
        Fsm.AddState(EntityStateId.Hit, new HitState(this));
        Fsm.AddState(EntityStateId.Dead, new DeadState(this));
    }

    protected virtual void RegisterTransitions()
    {
        Fsm.AddTwoWayTransition(new IdleMoveTransition(this)); // 움직임 트랜지션

        RegisterAttackTransition();

        Fsm.AddTriggerTransitionFromAny(EntityEvent.Damaged, new DamagedToHitTransition(this)); // 피격 트랜지션

        Fsm.AddTriggerTransitionFromAny(EntityEvent.Died, new DiedToDeadTransition(this)); // 사망 트랜지션
    }

    private void RegisterAttackTransition()
    {
        Fsm.AddTriggerTransition(EntityEvent.Attack, new IdleAttackTransition(this)); // idle -> attack
        Fsm.AddTriggerTransition(EntityEvent.Attack, new MoveAttackTransition(this)); // move -> attack
        Fsm.AddTriggerTransition(EntityEvent.AttackFinished, new AttackIdleTransition(this)); // attack -> idle
        Fsm.AddTriggerTransition(EntityEvent.AttackFinished, new AttackMoveTransition(this)); // attack -> move
    }

    // =========================================================
    // Event API
    // =========================================================

    /// <summary>
    /// Health 등의 외부 모듈에서 호출
    /// </summary>
    public void NotifyDamaged()
    {
        Fsm.Trigger(EntityEvent.Damaged);
    }

    public void NotifyDied()
    {
        Fsm.Trigger(EntityEvent.Died);
    }

    /// <summary>
    /// AttackModule / AnimationEvent 등에서 호출
    /// </summary>
    public void NotifyAttackFinished()
    {
        Fsm.Trigger(EntityEvent.AttackFinished);
    }

    public void NotifyHitFinished()
    {
        Fsm.Trigger(EntityEvent.HitFinished);
    }

    // =========================================================
    // Debug
    // =========================================================

}
