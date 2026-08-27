using UnityEngine;
using UnityHFSM;



// =========================================================
// Common State Creation / Registration
// =========================================================


public class EntityState<TController> : StateBase<EntityStateId> where TController : EntityController
{
    protected readonly EntityController controller;

    protected EntityState(EntityController _controller) : base(needsExitTime: false)
    {
        controller = _controller;
    }
}

public sealed class IdleState : EntityState<EntityController>
{
    public IdleState(EntityController controller) : base(controller) { }

    public override void OnEnter() { }
    public override void OnLogic() { }
    public override void OnExit() { }
}

public sealed class MoveState : EntityState<EntityController>
{
    public MoveState(EntityController controller) : base(controller) { }

    public override void OnEnter() { }
    public override void OnLogic() { controller.MoveModule.Move(); }
    public override void OnExit() { }
}

public sealed class AttackState : EntityState<EntityController>
{
    public AttackState(EntityController controller) : base(controller) { }

    public override void OnEnter() { controller.AttackModule.Attack(); }
    public override void OnLogic() { }
    public override void OnExit() { }
}
public sealed class HitState : EntityState<EntityController>
{
    public HitState(EntityController controller) : base(controller) { }

    public override void OnEnter() { }
    public override void OnLogic() { }
    public override void OnExit() { }
}
public sealed class DeadState : EntityState<EntityController>
{
    public DeadState(EntityController controller) : base(controller) { }

    public override void OnEnter() { }
    public override void OnLogic() { }
    public override void OnExit() { }
}