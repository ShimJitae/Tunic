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
}
