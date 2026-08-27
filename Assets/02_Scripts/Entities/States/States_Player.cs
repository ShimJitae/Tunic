using UnityEngine;


public sealed class DodgeState : EntityState<EntityController>
{
    public DodgeState(EntityController controller) : base(controller) { }

    public override void OnEnter() { }
    public override void OnLogic() { }
    public override void OnExit() { }
}

