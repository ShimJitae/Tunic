public sealed class DodgeState : EntityState<PlayerController>
{
    PlayerController controller;
    PlayerAnimationModule animModule;
    public DodgeState(PlayerController _controller) : base(_controller)
    {
        controller = _controller;
        animModule = (PlayerAnimationModule)controller.AnimationModule;
    }

    public override void OnEnter()
    {
        animModule.PlayDodge();
        controller.Health.IsInvincible = true;
    }

    public override void OnExit()
    {
        controller.Health.IsInvincible = false;
    }
}

