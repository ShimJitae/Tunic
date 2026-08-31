using UnityHFSM;

public abstract class EnemyMoveState : StateBase<EntityStateId>
{
    protected readonly EnemyController controller;
    protected readonly EnemyBrain brain;

    protected EnemyMoveState(EnemyController controller, EnemyBrain brain) : base(needsExitTime: false)
    {
        this.controller = controller;
        this.brain = brain;
    }

    public override void OnEnter()
    {
        controller.AnimationModule.PlayMove();
        controller.ResetMoveStartState();
    }

    public override void OnExit()
    {
        controller.MoveModule.MoveInfo = UnityEngine.Vector3.zero;
    }
}

public sealed class PatrolState : EnemyMoveState
{
    public PatrolState(EnemyController controller, EnemyBrain brain) : base(controller, brain)
    {
    }

    public override void OnLogic()
    {
        controller.MoveModule.Move();
        brain.UpdatePatrol();
    }
}

public sealed class ChaseState : EnemyMoveState
{
    public ChaseState(EnemyController controller, EnemyBrain brain) : base(controller, brain)
    {
    }

    public override void OnEnter()
    {
        base.OnEnter();
        brain.RefreshChaseDestination();
        controller.MoveModule.Move();
    }

    public override void OnLogic()
    {
        if (brain.UpdateChase())
            controller.MoveModule.Move();
    }
}
