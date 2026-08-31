// DodgeTransition
public sealed class IdleDodgeTransition : EntityTransition<PlayerController>
{
    public IdleDodgeTransition(PlayerController controller) : base(controller, EntityStateId.Idle, EntityStateId.Move)
    {
    }
}

public sealed class MoveDodgeTransition : EntityTransition<PlayerController>
{
    public MoveDodgeTransition(PlayerController controller) : base(controller, EntityStateId.Move, EntityStateId.Dodge)
    {
    }
}

public sealed class DodgeIdleTransition : EntityTransition<PlayerController>
{
    public DodgeIdleTransition(PlayerController controller) : base(controller, EntityStateId.Move, EntityStateId.Idle)
    {
    }

    public override bool ShouldTransition()
    {
        return controller.IsDodging && !controller.HasMoveInput;
    }
}

public sealed class DodgeMoveTransition : EntityTransition<PlayerController>
{
    public DodgeMoveTransition(PlayerController controller) : base(controller, EntityStateId.Dodge, EntityStateId.Move)
    {
    }

    public override bool ShouldTransition()
    {
        return controller.HasMoveInput;
    }
}
