using UnityEngine;
using UnityHFSM;



// =========================================================
// Common State Creation / Registration
// =========================================================


public abstract class EntityState<TController> : StateBase<EntityStateId> where TController : EntityController
{
    protected readonly TController controller;

    protected EntityState(TController controller) : base(needsExitTime: false)
    {
        this.controller = controller;
    }
}

public sealed class IdleState : EntityState<EntityController>
{
    public IdleState(EntityController controller) : base(controller) { }

    public override void OnEnter()
    {
        controller.AnimationModule.PlayIdle();
    }
    public override void OnLogic() { }
    public override void OnExit() { }
}

public sealed class MoveState : EntityState<EntityController>
{
    public MoveState(EntityController controller) : base(controller) { }

    public override void OnEnter()
    {
        controller.AnimationModule.PlayMove();
    }
    public override void OnLogic()
    {
        controller.MoveModule.Move();
    }
    public override void OnExit() { }
}

public sealed class AttackState : EntityState<EntityController>
{
    public AttackState(EntityController controller) : base(controller) { }

    public override void OnEnter()
    {
        controller.AnimationModule.PlayAttack();
    }
    public override void OnLogic()
    {
        if (controller.AnimationModule.IsCurrentAnimationFinished())
        {
            controller.NotifyAttackFinished();
        }
    }
    public override void OnExit() { }
}
public sealed class HitState : EntityState<EntityController>
{
    public HitState(EntityController controller) : base(controller) { }

    public override void OnEnter()
    {
        controller.AnimationModule.PlayHit();
    }
    public override void OnLogic()
    {
        if (controller.AnimationModule.IsCurrentAnimationFinished())
        {
            controller.NotifyHitFinished();
        }
    }
    public override void OnExit() { }
}
public sealed class DieState : EntityState<EntityController>
{
    public DieState(EntityController controller) : base(controller) { }

    public override void OnEnter()
    {
        controller.AnimationModule.PlayDie();
    }
    public override void OnLogic() { }
    public override void OnExit() { }
}
