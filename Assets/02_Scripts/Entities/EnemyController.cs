using UnityEngine;
using UnityHFSM;

[RequireComponent(typeof(EnemyMoveModule))]
[RequireComponent(typeof(EnemyAnimationModule))]
public class EnemyController : EntityController
{
    [SerializeField] private EnemyData enemyData;
    private EnemyMoveModule enemyMoveModule;
    private StateMachine<EntityStateId, EntityEvent> moveFsm;

    protected override void Awake()
    {
        if (!TryGetComponent(out enemyMoveModule))
        {
#if UNITY_EDITOR
            Debug.LogError(
                $"{nameof(EnemyController)} requires a " +
                $"{nameof(EnemyMoveModule)} component.",
                this);
#endif
            enabled = false;
            return;
        }

        MoveModule = enemyMoveModule;

        AnimationModule =
            GetComponentInChildren<EnemyAnimationModule>();

        if (AnimationModule == null)
        {
#if UNITY_EDITOR
            Debug.LogError(
                $"{nameof(EnemyController)} requires a " +
                $"{nameof(EnemyAnimationModule)} component.",
                this);
#endif
            enabled = false;
            return;
        }

        base.Awake();
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void SetUpEntityData()
    {
    }

    protected override StateBase<EntityStateId> CreateMoveState()
    {
        if (!TryGetComponent(out EnemyBrain enemyBrain))
        {
            Debug.LogError("Enemybrain 컴포넌트를 가지고 있지 않음");
            return null;
        }

        moveFsm = new StateMachine<EntityStateId, EntityEvent>();

        moveFsm.AddState(EntityStateId.Patrol, new PatrolState(this, enemyBrain));
        moveFsm.AddState(EntityStateId.Chase, new ChaseState(this, enemyBrain));

        moveFsm.SetStartState(EntityStateId.Patrol);

        moveFsm.AddTransition(new PatrolToChaseTransition(this, enemyBrain));

        return moveFsm;
    }

    public void RequestMoveState(EntityStateId moveState)
    {
        if (moveFsm.IsInitialized)
        {
            moveFsm.RequestStateChange(moveState);
            return;
        }

        moveFsm.SetStartState(moveState);
    }

    internal void ResetMoveStartState()
    {
        moveFsm.SetStartState(EntityStateId.Patrol);
    }

    protected override void RegisterTransitions()
    {
        if (!TryGetComponent(out EnemyBrain enemyBrain))
        {
            Debug.LogError("Enemybrain 컴포넌트를 가지고 있지 않음");
            return;
        }

        Fsm.AddTriggerTransition(EntityEvent.HitFinished, new HitToChaseTransition(this, enemyBrain));

        base.RegisterTransitions();
    }

    public void SetMoveDestination(Vector3 destination)
    {
        MoveModule.MoveInfo = destination;
    }

    public void StopMove()
    {
        // 기존 Transition이 Move → Idle로 전환하는 조건
        MoveModule.MoveInfo = Vector3.zero;

        // 이미 이동 중인 NavMeshAgent 정지
        enemyMoveModule.Stop();
    }

    public bool HasReachedDestination()
    {
        return enemyMoveModule.HasReachedDestination();
    }

    private void OnDisable()
    {
        if (enemyMoveModule != null)
            StopMove();
    }
}
