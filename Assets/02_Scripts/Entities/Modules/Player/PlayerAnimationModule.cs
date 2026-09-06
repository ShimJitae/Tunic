using UnityEngine;

public class PlayerAnimationModule : EntityAnimationModule
{
    public int Dodge { get; private set; }
    public int Attack1 { get; private set; }
    public int Attack2 { get; private set; }
    public int Attack3 { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        Dodge = Animator.StringToHash("Base Layer.Dodge");
        Attack1 = Animator.StringToHash("Base Layer.Attack.Attack_1");
        Attack2 = Animator.StringToHash("Base Layer.Attack.Attack_2");
        Attack3 = Animator.StringToHash("Base Layer.Attack.Attack_3");
        Attack = Attack1;
    }

    public void PlayDodge()
    {
        animator.CrossFadeInFixedTime(Dodge, 0.15f);
    }

    public void PlayAttack(PlayerCombatStateId attackState)
    {
        animator.CrossFadeInFixedTime(GetAttackStateHash(attackState), 0.05f);
    }

    public int GetAttackStateHash(PlayerCombatStateId attackState)
    {
        return attackState switch
        {
            PlayerCombatStateId.Attack1 => Attack1,
            PlayerCombatStateId.Attack2 => Attack2,
            PlayerCombatStateId.Attack3 => Attack3,
            _ => Attack1
        };
    }
}
