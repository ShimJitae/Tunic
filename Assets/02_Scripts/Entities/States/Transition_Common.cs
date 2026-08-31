
using UnityHFSM;

// =========================================================
// Common Transitions
// =========================================================

public abstract class EntityTransition<TController> : TransitionBase<EntityStateId> where TController : EntityController
{
    protected readonly TController controller;

    protected EntityTransition(TController controller, EntityStateId from, EntityStateId to, bool forceInstantly = false) : base(from, to, forceInstantly)
    {
        this.controller = controller;
    }
}

public sealed class IdleToMoveTransition : EntityTransition<EntityController>
{
    public IdleToMoveTransition(EntityController controller) : base(controller, EntityStateId.Idle, EntityStateId.Move)
    {
    }

    public override bool ShouldTransition()
    {
        return controller.HasMoveInput;
    }
}

public sealed class MoveToIdleTransition : EntityTransition<EntityController>
{
    public MoveToIdleTransition(EntityController controller) : base(controller, EntityStateId.Move, EntityStateId.Idle)
    {
    }

    public override bool ShouldTransition()
    {
        return controller.ShouldExitMoveState;
    }
}

public sealed class DamagedToHitTransition : EntityTransition<EntityController>
{
    public DamagedToHitTransition(EntityController controller) : base(controller, default, EntityStateId.Hit, forceInstantly: true)
    {
    }

    public override bool ShouldTransition()
    {
        return controller.CurrentState != EntityStateId.Die;
    }
}

public sealed class HitToIdleTransition : EntityTransition<EntityController>
{
    public HitToIdleTransition(EntityController controller) : base(controller, EntityStateId.Hit, EntityStateId.Idle, forceInstantly: true)
    {
    }
}

public sealed class DiedToDeadTransition : EntityTransition<EntityController>
{
    public DiedToDeadTransition(EntityController controller) : base(controller, default, EntityStateId.Die, forceInstantly: true)
    {
    }
}

// AttackTransition
public sealed class IdleToAttackTransition : EntityTransition<EntityController>
{
    public IdleToAttackTransition(EntityController controller) : base(controller, EntityStateId.Idle, EntityStateId.Attack)
    {
    }
}

public sealed class MoveToAttackTransition : EntityTransition<EntityController>
{
    public MoveToAttackTransition(EntityController controller) : base(controller, EntityStateId.Move, EntityStateId.Attack)
    {
    }

    public override bool ShouldTransition()
    {
        return controller.CanAttackFromMoveState;
    }
}

public sealed class AttackToIdleTransition : EntityTransition<EntityController>
{
    public AttackToIdleTransition(EntityController controller) : base(controller, EntityStateId.Attack, EntityStateId.Idle)
    {
    }

    public override bool ShouldTransition()
    {
        return !controller.HasMoveInput;
    }
}

public sealed class AttackToMoveTransition : EntityTransition<EntityController>
{
    public AttackToMoveTransition(EntityController controller) : base(controller, EntityStateId.Attack, EntityStateId.Move)
    {
    }

    public override bool ShouldTransition()
    {
        return controller.HasMoveInput;
    }
}
