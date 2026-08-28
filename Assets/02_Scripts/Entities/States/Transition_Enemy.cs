public sealed class IdleToChaseTransition : EntityTransition<EnemyController>
{
    public IdleToChaseTransition(EnemyController controller)
        : base(controller, EntityStateId.Idle, EntityStateId.Chase)
    {
    }

    public override bool ShouldTransition()
    {
        return controller.HasTarget;
    }
}

public sealed class ChaseToAttackTransition : EntityTransition<EnemyController>
{
    public ChaseToAttackTransition(EnemyController controller)
        : base(controller, EntityStateId.Chase, EntityStateId.Attack)
    {
    }

    public override bool ShouldTransition()
    {
        return controller.IsTargetInAttackRange;
    }
}
