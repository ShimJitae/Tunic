public sealed class PatrolToChaseTransition : EntityTransition<EnemyController>
{
    private readonly EnemyBrain brain;

    public PatrolToChaseTransition(EnemyController controller, EnemyBrain brain) : base(controller, EntityStateId.Patrol, EntityStateId.Chase)
    {
        this.brain = brain;
    }

    public override bool ShouldTransition()
    {
        return brain.CanDetectTarget();
    }
}

public sealed class HitToChaseTransition : EntityTransition<EnemyController>
{
    private readonly EnemyBrain brain;

    public HitToChaseTransition(EnemyController controller, EnemyBrain brain) : base(controller, EntityStateId.Hit, EntityStateId.Move, forceInstantly: true)
    {
        this.brain = brain;
    }

    public override void BeforeTransition()
    {
        brain.EnterChase();
    }
}