public sealed class DodgeState : EntityState<PlayerController>
{
    PlayerAnimationModule animModule;
    public DodgeState(PlayerController controller) : base(controller)
    {
        animModule = (PlayerAnimationModule)controller.AnimationModule;
    }

    public override void OnEnter()
    {
        animModule.PlayDodge();
    }

    public override void OnLogic()
    {
        if (!animModule.IsCurrentAnimationFinished(0.75f))
            return;

        controller.NotifyDodgeFinished();
    }
}

