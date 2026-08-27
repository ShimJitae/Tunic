public sealed class DodgeState : EntityState<PlayerController>
{
    private const float ExitNormalizedTime = 0.75f;

    public DodgeState(PlayerController controller) : base(controller) { }

    public override void OnEnter()
    {
        controller.PlayerAnimation.PlayDodge();
    }

    public override void OnLogic()
    {
        if (!controller.PlayerAnimation.IsCurrentAnimationFinished(ExitNormalizedTime))
            return;

        controller.NotifyDodgeFinished();
    }
}

