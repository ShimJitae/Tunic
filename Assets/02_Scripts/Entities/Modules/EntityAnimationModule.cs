using UnityEngine;

public class EntityAnimationModule : MonoBehaviour, IAnimationModule
{
    [SerializeField] protected Animator animator;

    void Awake()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
            if (animator == null)
            {
                Debug.LogError("해당 Entity의 자식 오브젝트에 Animator가 없습니다.");
            }
        }

        Idle = Animator.StringToHash("Base Layer.Idle");
        Move = Animator.StringToHash("Base Layer.Move");
        Attack = Animator.StringToHash("Base Layer.Attack.Attack_1");
        Hit = Animator.StringToHash("Base Layer.Hit");
        Die = Animator.StringToHash("Base Layer.Die");
    }

    public int Idle { get; set; }
    public int Move { get; set; }
    public int Attack { get; set; }
    public int Hit { get; set; }
    public int Die { get; set; }

    public void PlayIdle()
    {
        animator.CrossFadeInFixedTime("Idle", 0.15f);
    }

    public void PlayMove()
    {
        animator.CrossFadeInFixedTime("Move", 0.15f);
    }

    public void PlayAttack()
    {
        animator.CrossFadeInFixedTime("Attack", 0.05f);
    }

    public void PlayHit()
    {
        animator.CrossFadeInFixedTime("Hit", 0.05f);
    }

    public void PlayDie()
    {
        animator.CrossFadeInFixedTime("Die", 0.1f);
    }
}
