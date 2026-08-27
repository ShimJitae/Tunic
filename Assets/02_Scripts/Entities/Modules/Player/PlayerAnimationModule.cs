using UnityEngine;

public class PlayerAnimationModule : EntityAnimationModule
{
    private static readonly int Dodge = Animator.StringToHash("Base Layer.Dodge");

    public void PlayDodge()
    {
        animator.CrossFadeInFixedTime(Dodge, 0.15f);
    }
}
