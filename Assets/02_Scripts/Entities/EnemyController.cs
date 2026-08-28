using UnityEngine;
using UnityEngine.AI;
using UnityHFSM;

[RequireComponent(typeof(Rigidbody))]
public class EnemyController : EntityController
{
    private EnemyMoveModule enemyMoveModule;
    NavMeshAgent agent;

    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Range")]
    [SerializeField, Min(0f)] private float detectionRange = 10f;
    [SerializeField, Min(0f)] private float attackRange = 2f;

    public bool HasTarget => target != null && DistanceToTarget <= detectionRange;
    public bool IsTargetInAttackRange => HasTarget && DistanceToTarget <= attackRange;

    public Vector3 ChaseDirection
    {
        get
        {
            if (target == null)
                return Vector3.zero;

            Vector3 direction = target.position - transform.position;
            direction.y = 0f;

            return direction.sqrMagnitude > 0.001f
                ? direction.normalized
                : Vector3.zero;
        }
    }

    private float DistanceToTarget => target == null
        ? float.MaxValue
        : Vector3.Distance(transform.position, target.position);

    protected override void Awake()
    {
        if (!TryGetComponent(out enemyMoveModule))
        {
            Debug.LogError($"{nameof(EnemyController)} requires a {nameof(EnemyMoveModule)} component.", this);
            enabled = false;
            return;
        }

        MoveModule = enemyMoveModule;

        AnimationModule = GetComponentInChildren<EnemyAnimationModule>();
        if (AnimationModule == null)
        {
            Debug.LogError($"{nameof(EnemyController)} requires a {nameof(EnemyAnimationModule)} component.", this);
            enabled = false;
            return;
        }

        base.Awake();
    }

    protected override void RegisterStates()
    {
        base.RegisterStates();

        Fsm.AddState(EntityStateId.Chase, new ChaseState(this));
    }

    protected override void RegisterTransitions()
    {
        base.RegisterTransitions();

        RegisterChaseTransitions();
    }

    private void RegisterChaseTransitions()
    {
        Fsm.AddTwoWayTransition(new IdleToChaseTransition(this));
        Fsm.AddTransition(new ChaseToAttackTransition(this));
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void ClearTarget()
    {
        target = null;
    }
}
