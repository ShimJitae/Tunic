using UnityEngine;

namespace Tunic.BossCombat
{
    public sealed class BossAnimationModule : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        private int playingHash;
        private float deadline;
        public bool Playing { get; private set; }
        public bool Failed { get; private set; }
        public float Progress { get; private set; }
        public Animator Animator => animator;

        private void Awake()
        {
            if (animator == null) animator = GetComponentInChildren<Animator>();
            if (animator != null) animator.applyRootMotion = false;
        }

        public bool CanPlay(BossMotion motion)
        {
            return animator != null && animator.runtimeAnimatorController != null && motion != null &&
                motion.clip != null && motion.clip.length > 0f && motion.speed > 0f &&
                !string.IsNullOrEmpty(motion.animatorState) &&
                animator.HasState(0, Animator.StringToHash(motion.animatorState));
        }

        public bool Begin(BossMotion motion)
        {
            StopMotion();
            Failed = !CanPlay(motion);
            if (Failed) return false;
            playingHash = Animator.StringToHash(motion.animatorState);
            animator.speed = motion.speed;
            // Restart at zero even when consecutive combo entries use the same clip/state.
            animator.Play(playingHash, 0, 0f);
            Progress = 0f;
            deadline = Time.time + motion.clip.length / motion.speed + 1f;
            Playing = true;
            return true;
        }

        public bool Tick()
        {
            if (!Playing) return true;
            var state = animator.GetCurrentAnimatorStateInfo(0);
            if (state.fullPathHash == playingHash)
            {
                Progress = Mathf.Max(Progress, state.normalizedTime);
                if (Progress >= 1f) return true;
            }
            if (Time.time < deadline) return false;
            Failed = true;
            Debug.LogWarning("Boss motion timed out; closing the attack safely.", this);
            return true;
        }

        public void StopMotion()
        {
            Playing = false;
            if (animator != null) animator.speed = 1f;
        }

        public void Locomotion(bool moving) => PlayState(moving ? "Base Layer.Move" : "Base Layer.Idle");
        public void Die() { StopMotion(); PlayState("Base Layer.Die"); }

        private void PlayState(string stateName)
        {
            if (animator == null || Playing) return;
            int hash = Animator.StringToHash(stateName);
            if (!animator.HasState(0, hash)) return;
            if (animator.GetCurrentAnimatorStateInfo(0).fullPathHash != hash &&
                (!animator.IsInTransition(0) || animator.GetNextAnimatorStateInfo(0).fullPathHash != hash))
                animator.CrossFadeInFixedTime(hash, 0.1f);
        }
    }
}
