using UnityEngine;

public sealed class ChaseState : EntityState<EnemyController>
{
    public ChaseState(EnemyController controller) : base(controller)
    {
    }

    public override void OnEnter()
    {
        controller.AnimationModule.PlayMove();
    }

    public override void OnLogic()
    {
        controller.MoveModule.MoveInfo = controller.ChaseDirection;
        controller.MoveModule.Move();
    }

    public override void OnExit()
    {
        controller.MoveModule.MoveInfo = Vector3.zero;
    }
}
