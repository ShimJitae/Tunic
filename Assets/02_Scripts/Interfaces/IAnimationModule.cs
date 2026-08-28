using UnityEngine;

public interface IAnimationModule
{
    public int Idle { get; set; }
    public int Move { get; set; }
    public int Attack { get; set; }
    public int Hit { get; set; }
    public int Die { get; set; }

    public void PlayIdle();
    public void PlayMove();
    public void PlayAttack();
    public void PlayHit();
    public void PlayDie();
    // 현재 애니메이션의 상태가 completionThreshold 이상으로 진행되면 종료되었다고 판단하는 메서드
    public bool IsCurrentAnimationFinished(float completionThreshold = 0.95f, int layerIndex = 0);
}
