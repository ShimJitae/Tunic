using UnityEngine;
using UnityHFSM;

public abstract class EntityController : MonoBehaviour
{
    protected StateMachine<EntityStateId, EntityEvent> Fsm { get; private set; }

    // =========================================================
    // Common Condition
    // =========================================================

    /// <summary>
    /// 이동 입력이 존재하는가?
    /// Player에서는 Input,
    /// 필요하다면 다른 Entity에서는 이동 의도로 사용할 수 있습니다.
    /// </summary>
    protected abstract bool HasMoveInput { get; }


    // =========================================================
    // Unity Lifecycle
    // =========================================================

    protected virtual void Awake()
    {
        CreateStateMachine();
    }

    protected virtual void Update()
    {
        BeforeFsmLogic();

        Fsm.OnLogic();

        AfterFsmLogic();
    }


    protected virtual void BeforeFsmLogic()
    {
    }

    protected virtual void AfterFsmLogic()
    {
    }


    // =========================================================
    // FSM Creation
    // =========================================================

    private void CreateStateMachine()
    {
        Fsm = new StateMachine<EntityStateId, EntityEvent>();

        // 1. State 등록
        RegisterCommonStates();
        RegisterSpecificStates();

        // 2. 시작 State
        Fsm.SetStartState(EntityStateId.Idle);

        // 3. Transition 등록
        RegisterCommonTransitions();
        RegisterSpecificTransitions();

        // 4. 초기화
        Fsm.Init();
    }


    // =========================================================
    // Common State Registration
    // =========================================================

    private void RegisterCommonStates()
    {
        // -----------------------------------------------------
        // Idle
        // -----------------------------------------------------

        Fsm.AddState(
            EntityStateId.Idle,
            new State<EntityStateId, EntityEvent>(
                onEnter: _ => EnterIdle(),
                onLogic: _ => UpdateIdle(),
                onExit: _ => ExitIdle()
            )
        );


        // -----------------------------------------------------
        // Move
        // -----------------------------------------------------

        Fsm.AddState(
            EntityStateId.Move,
            new State<EntityStateId, EntityEvent>(
                onEnter: _ => EnterMove(),
                onLogic: _ => UpdateMove(),
                onExit: _ => ExitMove()
            )
        );


        // -----------------------------------------------------
        // Attack
        // -----------------------------------------------------

        Fsm.AddState(
            EntityStateId.Attack,
            new State<EntityStateId, EntityEvent>(
                onEnter: _ => EnterAttack(),
                onLogic: _ => UpdateAttack(),
                onExit: _ => ExitAttack()
            )
        );


        // -----------------------------------------------------
        // Hit
        // -----------------------------------------------------

        Fsm.AddState(
            EntityStateId.Hit,
            new State<EntityStateId, EntityEvent>(
                onEnter: _ => EnterHit(),
                onLogic: _ => UpdateHit(),
                onExit: _ => ExitHit()
            )
        );


        // -----------------------------------------------------
        // Dead
        // -----------------------------------------------------

        Fsm.AddState(
            EntityStateId.Dead,
            new State<EntityStateId, EntityEvent>(
                onEnter: _ => EnterDead(),
                onLogic: _ => UpdateDead(),
                onExit: _ => ExitDead()
            )
        );
    }


    // =========================================================
    // Common Transitions
    // =========================================================

    private void RegisterCommonTransitions()
    {
        /*
         * Idle <-> Move
         *
         * HasMoveInput == true
         * Idle -> Move
         *
         * HasMoveInput == false
         * Move -> Idle
         */
        Fsm.AddTwoWayTransition(
            EntityStateId.Idle,
            EntityStateId.Move,
            _ => HasMoveInput
        );


        /*
         * Any -> Hit
         *
         * 매 프레임 "피격됐는가?"를 검사하지 않습니다.
         * 실제 Damage 이벤트가 발생했을 때 Trigger 합니다.
         *
         * forceInstantly:
         * Attack 같은 상태가 ExitTime을 사용하게 되더라도
         * Hit으로 강제 interrupt 가능.
         */
        Fsm.AddTriggerTransitionFromAny(
            EntityEvent.Damaged,
            EntityStateId.Hit,
            forceInstantly: true
        );


        /*
         * Any -> Dead
         *
         * 사망은 다른 모든 상태를 강제로 끊습니다.
         */
        Fsm.AddTriggerTransitionFromAny(
            EntityEvent.Died,
            EntityStateId.Dead,
            forceInstantly: true
        );
    }


    // =========================================================
    // Specific State / Transition
    // =========================================================

    protected abstract void RegisterSpecificStates();

    protected abstract void RegisterSpecificTransitions();


    // =========================================================
    // Common State Behaviour
    // =========================================================

    protected virtual void EnterIdle()
    {
    }

    protected virtual void UpdateIdle()
    {
    }

    protected virtual void ExitIdle()
    {
    }


    protected virtual void EnterMove()
    {
    }

    protected virtual void UpdateMove()
    {
    }

    protected virtual void ExitMove()
    {
    }


    protected virtual void EnterAttack()
    {
    }

    protected virtual void UpdateAttack()
    {
    }

    protected virtual void ExitAttack()
    {
    }


    protected virtual void EnterHit()
    {
    }

    protected virtual void UpdateHit()
    {
    }

    protected virtual void ExitHit()
    {
    }


    protected virtual void EnterDead()
    {
    }

    protected virtual void UpdateDead()
    {
    }

    protected virtual void ExitDead()
    {
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

    public EntityStateId CurrentState =>
        Fsm.ActiveStateName;
}