using UnityEngine;
using UnityHFSM;

public class EnemyController : EntityController
{
    private State<EntityStateId, EntityEvent> chaseState;

    [Header("Target")]
    [SerializeField]
    private Transform target;

    [Header("Range")]
    [SerializeField]
    private float detectionRange = 10f;

    [SerializeField]
    private float attackRange = 2f;

    // =========================================================
    // Conditions
    // =========================================================

    private bool HasTarget
    {
        get
        {
            if (target == null)
                return false;

            return DistanceToTarget <= detectionRange;
        }
    }


    private bool IsTargetInAttackRange
    {
        get
        {
            if (target == null)
                return false;

            return DistanceToTarget <= attackRange;
        }
    }


    private float DistanceToTarget
    {
        get
        {
            if (target == null)
                return float.MaxValue;

            return Vector3.Distance(
                transform.position,
                target.position
            );
        }
    }


    // =========================================================
    // Enemy State Creation / Registration
    // =========================================================

    // protected override void CreateSpecificStates()
    // {
    //     chaseState = new State<EntityStateId, EntityEvent>(
    //         onEnter: _ => EnterChase(),
    //         onLogic: _ => UpdateChase(),
    //         onExit: _ => ExitChase()
    //     );
    // }

    // protected override void RegisterSpecificStates()
    // {
    //     Fsm.AddState(EntityStateId.Chase, chaseState);
    // }


    // // =========================================================
    // // Enemy Transitions
    // // =========================================================

    // protected override void RegisterSpecificTransitions()
    // {
    //     RegisterChaseTransitions();

    //     RegisterAttackTransitions();

    //     RegisterHitTransitions();
    // }


    private void RegisterChaseTransitions()
    {
        /*
         * HasTarget == true
         * Idle -> Chase
         *
         * HasTarget == false
         * Chase -> Idle
         */

        Fsm.AddTwoWayTransition(
            EntityStateId.Idle,
            EntityStateId.Chase,
            _ => HasTarget
        );


        /*
         * Chase -> Attack
         *
         * 타겟과의 거리는 계속 변화하므로
         * Polling Transition.
         */
        Fsm.AddTransition(
            EntityStateId.Chase,
            EntityStateId.Attack,
            _ => IsTargetInAttackRange
        );
    }


    private void RegisterAttackTransitions()
    {
        /*
         * Attack이 완료되었을 때,
         * 여전히 타겟이 있으면 다시 Chase.
         *
         * Chase로 돌아간 다음 다음 OnLogic에서
         * 여전히 AttackRange 안이라면 다시 Attack.
         */

        Fsm.AddTriggerTransition(
            EntityEvent.AttackFinished,
            EntityStateId.Attack,
            EntityStateId.Chase,
            _ => HasTarget
        );


        /*
         * 공격 도중 타겟이 사라진 경우.
         */
        Fsm.AddTriggerTransition(
            EntityEvent.AttackFinished,
            EntityStateId.Attack,
            EntityStateId.Idle,
            _ => !HasTarget
        );
    }


    private void RegisterHitTransitions()
    {
        /*
         * Hit 종료 후 Target이 존재하면 Chase.
         */
        Fsm.AddTriggerTransition(
            EntityEvent.HitFinished,
            EntityStateId.Hit,
            EntityStateId.Chase,
            _ => HasTarget
        );


        /*
         * Target이 없다면 Idle.
         */
        Fsm.AddTriggerTransition(
            EntityEvent.HitFinished,
            EntityStateId.Hit,
            EntityStateId.Idle,
            _ => !HasTarget
        );
    }


    // =========================================================
    // Chase
    // =========================================================

    private void EnterChase()
    {
        // animator.Play("Run");
    }


    private void UpdateChase()
    {
        if (target == null)
            return;

        Vector3 direction =
            target.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        direction.Normalize();

        /*
         * movementModule.Move(direction);
         */
    }


    private void ExitChase()
    {
        /*
         * movementModule.Stop();
         */
    }


    // =========================================================
    // Target API
    // =========================================================

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void ClearTarget()
    {
        target = null;
    }
}
