using UnityEngine;

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

public sealed class MoveIdleTransition : EntityTransition<EntityController>
{
    public MoveIdleTransition(EntityController controller) : base(controller, EntityStateId.Attack, EntityStateId.Idle)
    {
    }
}

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