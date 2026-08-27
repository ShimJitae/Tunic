
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

public sealed class IdleMoveTransition : EntityTransition<EntityController>
{
    public IdleMoveTransition(EntityController controller) : base(controller, EntityStateId.Idle, EntityStateId.Move)
    {
    }

    public override bool ShouldTransition()
    {
        return controller.HasMoveInput;
    }
}

public sealed class DamagedToHitTransition : EntityTransition<EntityController>
{
    public DamagedToHitTransition(EntityController controller) : base(controller, default, EntityStateId.Hit, forceInstantly: true)
    {
    }

    public override bool ShouldTransition()
    {
        return controller.CurrentState != EntityStateId.Dead;
    }
}

public sealed class DiedToDeadTransition : EntityTransition<EntityController>
{
    public DiedToDeadTransition(EntityController controller) : base(controller, default, EntityStateId.Dead, forceInstantly: true)
    {
    }
}

// AttackTransition
public sealed class IdleAttackTransition : EntityTransition<EntityController>
{
    public IdleAttackTransition(EntityController controller) : base(controller, EntityStateId.Idle, EntityStateId.Attack)
    {
    }
}

public sealed class MoveAttackTransition : EntityTransition<EntityController>
{
    public MoveAttackTransition(EntityController controller) : base(controller, EntityStateId.Move, EntityStateId.Attack)
    {
    }
}

public sealed class AttackIdleTransition : EntityTransition<EntityController>
{
    public AttackIdleTransition(EntityController controller) : base(controller, EntityStateId.Attack, EntityStateId.Idle)
    {
    }

    public override bool ShouldTransition()
    {
        return !controller.HasMoveInput;
    }
}

public sealed class AttackMoveTransition : EntityTransition<EntityController>
{
    public AttackMoveTransition(EntityController controller) : base(controller, EntityStateId.Attack, EntityStateId.Move)
    {
    }

    public override bool ShouldTransition()
    {
        return controller.HasMoveInput;
    }
}
