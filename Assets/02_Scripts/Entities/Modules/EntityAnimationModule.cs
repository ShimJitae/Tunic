using UnityEngine;

public class EntityAnimationModule : MonoBehaviour
{
    [SerializeField] private Animator animator;

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
    }

    private static readonly int Idle = Animator.StringToHash("Base Layer.Idle");
    private static readonly int Move = Animator.StringToHash("Base Layer.Move");
    private static readonly int Attack = Animator.StringToHash("Base Layer.Attack");
    private static readonly int Hit = Animator.StringToHash("Base Layer.Hit");
    private static readonly int Dead = Animator.StringToHash("Base Layer.Dead");

    public void PlayIdle()
    {
        animator.CrossFadeInFixedTime(Idle, 0.15f);
    }

    public void PlayMove()
    {
        animator.CrossFadeInFixedTime(Move, 0.15f);
    }

    public void PlayAttack()
    {
        animator.CrossFadeInFixedTime(Attack, 0.05f);
    }

    public void PlayHit()
    {
        animator.CrossFadeInFixedTime(Hit, 0.05f);
    }

    public void PlayDead()
    {
        animator.CrossFadeInFixedTime(Dead, 0.1f);
    }
}
