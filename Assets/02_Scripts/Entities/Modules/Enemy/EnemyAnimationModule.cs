using UnityEngine;

public class EnemyAnimationModule : EntityAnimationModule
{
    protected override void Awake()
    {
        base.Awake();
        Attack = Animator.StringToHash("Base Layer.Attack");
    }
}
