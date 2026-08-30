using UnityEngine;

[RequireComponent(typeof(EnemyMoveModule))]
[RequireComponent(typeof(EnemyAnimationModule))]
public class EnemyController : EntityController
{
    private EnemyMoveModule enemyMoveModule;

    public EnemyMoveType MoveType =>
        enemyMoveModule.MoveType;

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

    public void SetMoveType(EnemyMoveType moveType)
    {
        enemyMoveModule.MoveType = moveType;
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