using UnityEngine;

public class PlayerAttackAnimationEventRelay : AttackAnimationEventRelay
{
    [SerializeField] private ParticleSystem attackParticleSystem;

    private void OnEnable()
    {
        if (AttackZoneController != null)
            AttackZoneController.OnAttackZoneChanged += HandleAttackZoneChanged;
    }

    private void OnDisable()
    {
        if (AttackZoneController != null)
            AttackZoneController.OnAttackZoneChanged -= HandleAttackZoneChanged;

        if (attackParticleSystem != null)
            attackParticleSystem.Stop();
    }

    private void HandleAttackZoneChanged(bool isActive)
    {
        if (attackParticleSystem == null)
            return;

        if (isActive)
            attackParticleSystem.Play();
        else
            attackParticleSystem.Stop();
    }
}
