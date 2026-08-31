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
