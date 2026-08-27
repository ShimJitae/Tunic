using UnityEngine;

// DodgeTransition
public sealed class IdleDodgeTransition : EntityTransition<EntityController>
{
    public IdleDodgeTransition(EntityController controller) : base(controller, EntityStateId.Idle, EntityStateId.Dodge)
    {
    }
}

public sealed class MoveDodgeTransition : EntityTransition<EntityController>
{
    public MoveDodgeTransition(EntityController controller) : base(controller, EntityStateId.Move, EntityStateId.Dodge)
    {
    }
}

public sealed class DodgeIdleTransition : EntityTransition<EntityController>
{
    public DodgeIdleTransition(EntityController controller) : base(controller, EntityStateId.Dodge, EntityStateId.Idle)
    {
    }
}