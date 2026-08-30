public sealed class PatrolToChaseTransition : EntityTransition<EnemyController>
{
    private readonly EnemyBrain brain;

    public PatrolToChaseTransition(EnemyController controller, EnemyBrain brain)
        : base(controller, EntityStateId.Patrol, EntityStateId.Chase)
    {
        this.brain = brain;
    }

    public override bool ShouldTransition()
    {
        return brain.CanDetectTarget();
    }
}
