using System;
using UnityEngine;

public class AttackAnimationEventRelay : MonoBehaviour
{
    [Serializable]
    private sealed class AttackParticleBinding
    {
        public AnimationClip clip;
        public ParticleSystem particle;
    }

    [Header("Attack Particles (Optional)")]
    [Tooltip("공격 클립과 재사용할 파티클 루트를 연결합니다. 하위 파티클도 함께 재생합니다.")]
    [SerializeField] private AttackParticleBinding[] attackParticles = Array.Empty<AttackParticleBinding>();

    private IAttackZoneController attackZoneController;

    protected IAttackZoneController AttackZoneController => attackZoneController;

    protected virtual void Awake()
    {
        attackZoneController = GetComponentInParent<IAttackZoneController>();
        if (attackZoneController == null)
        {
            Debug.LogError(
                $"{nameof(AttackAnimationEventRelay)} : " +
                $"{gameObject.name}의 부모 오브젝트에서 공격 판정 모듈을 찾지 못했습니다.",
                this);
        }

        PrepareParticles();
    }

    public void OpenAttackZone(AnimationEvent animationEvent)
    {
        attackZoneController?.SetAttackZoneActive(true);

        if (animationEvent == null || !animationEvent.isFiredByAnimator || attackParticles == null)
            return;

        AnimationClip clip = animationEvent.animatorClipInfo.clip;
        if (clip == null)
            return;

        foreach (AttackParticleBinding binding in attackParticles)
        {
            if (binding == null || binding.clip != clip || binding.particle == null)
                continue;

            ParticleSystem particle = binding.particle;
            particle.gameObject.SetActive(true);
            particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particle.Play(true);
            return;
        }
    }

    public void CloseAttackZone()
    {
        attackZoneController?.SetAttackZoneActive(false);
    }

    protected virtual void OnDisable()
    {
        if (attackParticles == null)
            return;

        foreach (AttackParticleBinding binding in attackParticles)
        {
            if (binding?.particle != null)
                binding.particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    private void PrepareParticles()
    {
        if (attackParticles == null)
            return;

        foreach (AttackParticleBinding binding in attackParticles)
        {
            if (binding?.particle == null)
                continue;

            foreach (ParticleSystem particle in binding.particle.GetComponentsInChildren<ParticleSystem>(true))
            {
                ParticleSystem.MainModule main = particle.main;
                main.loop = false;
                main.playOnAwake = false;
                main.stopAction = ParticleSystemStopAction.None;
            }

            binding.particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}
