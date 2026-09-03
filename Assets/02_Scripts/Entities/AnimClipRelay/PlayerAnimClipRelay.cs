using UnityEngine;

public class PlayerAnimClipRelay : EntityAnimClipRelay
{
    public void DodgeFinished()
    {
        (entityController as PlayerController).NotifyDodgeFinished();
    }

    [SerializeField] private ParticleSystem attackParticleSystem;
    public override void OpenAttackZone()
    {
        base.OpenAttackZone();
        attackParticleSystem.Play();
    }

    public override void CloseAttackZone()
    {
        base.CloseAttackZone();
        attackParticleSystem.Stop();
    }
}
