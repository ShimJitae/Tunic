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
    private static readonly int Attack = Animator.StringToHash("Base Layer.Attack.Attack_1");
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

    // 현재 애니메이션의 상태가 completionThreshold 이상으로 진행되면 종료되었다고 판단하는 메서드
    public bool IsCurrentAnimationFinished(float completionThreshold = 0.95f, int layerIndex = 0)
    {
        // 참조가 없는 경우 / 컴포넌트나 GameObject가 비활성화된 경우 / Animator 초기화가 끝나지 않은 경우
        if (animator == null || !animator.isActiveAndEnabled || !animator.isInitialized)
        {
            return false;
        }

        if (layerIndex < 0 || layerIndex >= animator.layerCount)
            return false;

        // CrossFade 중에는 현재 상태가 명확하지 않으므로 종료 처리하지 않음
        if (animator.IsInTransition(layerIndex))
            return false;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);

        // Idle, Move 같은 반복 애니메이션은 종료로 판단하지 않음
        if (stateInfo.loop)
            return false;

        return stateInfo.normalizedTime >= Mathf.Clamp01(completionThreshold);
    }
}
