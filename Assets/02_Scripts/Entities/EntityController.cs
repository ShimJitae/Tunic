using UnityEngine;
using UnityHFSM;

public abstract class EntityController : MonoBehaviour
{
    protected StateMachine<EntityStateId, EntityEvent> Fsm { get; private set; }

    public IMoveStrategy MoveModule { get; protected set; }
    public virtual bool HasMoveInput => false;
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

        // 1. State 생성
        CreateSpecificStates();

        // 2. State 등록
        RegisterCommonStates();
        RegisterSpecificStates();

        // 3. 시작 State
        Fsm.SetStartState(EntityStateId.Idle);

        // 4. Transition 등록
        RegisterCommonTransitions();
        RegisterSpecificTransitions();

        // 5. 초기화
        Fsm.Init();
    }
    protected virtual void RegisterCommonStates()
    {
        Fsm.AddState(EntityStateId.Idle, new IdleState(this));
        Fsm.AddState(EntityStateId.Move, new MoveState(this));
        Fsm.AddState(EntityStateId.Attack, new AttackState(this));
        Fsm.AddState(EntityStateId.Hit, new HitState(this));
        Fsm.AddState(EntityStateId.Dead, new DeadState(this));
    }

    private void RegisterCommonTransitions()
    {
        Fsm.AddTwoWayTransition(new IdleMoveTransition(this));
        Fsm.AddTriggerTransitionFromAny(EntityEvent.Damaged, new DamagedToHitTransition(this));
        Fsm.AddTriggerTransitionFromAny(EntityEvent.Died, new DiedToDeadTransition(this));
    }

    // =========================================================
    // Specific State / Transition
    // =========================================================

    protected abstract void CreateSpecificStates();

    protected abstract void RegisterSpecificStates();

    protected abstract void RegisterSpecificTransitions();

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
