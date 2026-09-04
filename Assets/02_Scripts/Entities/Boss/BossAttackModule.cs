using System.Collections.Generic;
using UnityEngine;

namespace Tunic.BossCombat
{
    public sealed class BossAttackModule : MonoBehaviour
    {
        private BossController boss;
        private BossMotion motion;
        private BossAttackData attack;
        private float previousProgress;
        private Vector3 previousPosition;
        private bool fired;
        private readonly HashSet<IDamageable> hitTargets = new HashSet<IDamageable>();
        private readonly List<BossProjectile> projectiles = new List<BossProjectile>();
        public bool IsMotionPlaying { get; private set; }
        public bool HitWindowOpen { get; private set; }
        public int MotionStartCount { get; private set; }
        public int ProjectilesFired { get; private set; }

        private void Awake() => boss = GetComponent<BossController>();

        public bool IsValid(BossAttackData data)
        {
            if (data.motions == null || data.motions.Length == 0 ||
                data.kind == BossAttackKind.Ranged && data.projectilePrefab == null) return false;
            foreach (var step in data.motions)
                if (!boss.Animation.CanPlay(step) || step.hitStart > step.hitEnd) return false;
            return true;
        }

        public bool BeginMotion()
        {
            StopMotion();
            attack = boss.SelectedAttack;
            motion = boss.CurrentMotion;
            if (attack == null || !boss.HasTarget || !boss.Animation.Begin(motion)) return false;
            boss.Movement.Stop();
            if (attack.kind == BossAttackKind.Charge) boss.Movement.BeginCharge(attack.chargeDistance);
            fired = false;
            hitTargets.Clear();
            previousProgress = -Mathf.Epsilon;
            previousPosition = transform.position;
            IsMotionPlaying = true;
            MotionStartCount++;
            boss.MarkMotionStarted();
            return true;
        }

        public bool TickMotion()
        {
            if (!IsMotionPlaying) return true;
            if (boss.IsDead || !boss.HasTarget) { StopMotion(); return true; }
            boss.ObserveComboDistance();
            bool finished = boss.Animation.Tick();
            float progress = boss.Animation.Progress;
            // A frame that crosses the entire window still samples the hit volume once.
            HitWindowOpen = !boss.Animation.Failed && previousProgress <= motion.hitEnd && progress >= motion.hitStart;
            if (attack.kind == BossAttackKind.Charge && HitWindowOpen) boss.Movement.TickCharge(attack.chargeSpeed);
            if (attack.kind != BossAttackKind.Ranged && HitWindowOpen) SampleMelee();
            if (attack.kind == BossAttackKind.Ranged && !fired && !boss.Animation.Failed && progress >= motion.fireTime)
            {
                fired = true;
                FireProjectile();
            }
            previousProgress = progress;
            previousPosition = transform.position;
            if (boss.Animation.Failed) boss.CancelCombo();
            return finished;
        }

        private void SampleMelee()
        {
            Vector3 displacement = transform.position - previousPosition;
            int samples = Mathf.Clamp(Mathf.CeilToInt(displacement.magnitude / 0.3f), 1, 64);
            for (int i = 1; i <= samples; i++)
            {
                Vector3 origin = Vector3.Lerp(previousPosition, transform.position, (float)i / samples);
                Vector3 center = origin + transform.rotation * motion.hitCenter;
                foreach (Collider collider in Physics.OverlapBox(center, motion.hitHalfExtents, transform.rotation,
                    boss.Data.targetLayers, QueryTriggerInteraction.Ignore))
                {
                    var receiver = collider.GetComponentInParent<IDamageable>();
                    if (receiver == null || collider.transform.IsChildOf(transform) || !hitTargets.Add(receiver)) continue;
                    receiver.TakeDamage(motion.damage);
                }
            }
        }

        private void FireProjectile()
        {
            if (attack.projectilePrefab == null || !boss.HasTarget) return;
            Vector3 origin = transform.TransformPoint(attack.muzzleOffset);
            // Aim once at release. The projectile keeps this direction for its whole lifetime.
            Vector3 destination = boss.Target.position + Vector3.up;
            Vector3 direction = (destination - origin).normalized;
            if (direction.sqrMagnitude < 0.001f) direction = transform.forward;
            var projectile = Instantiate(attack.projectilePrefab, origin, Quaternion.LookRotation(direction));
            projectile.Initialize(boss, direction, motion.damage, attack.projectileSpeed, attack.projectileLifetime);
            projectiles.RemoveAll(item => item == null);
            projectiles.Add(projectile);
            ProjectilesFired++;
        }

        public void StopMotion()
        {
            IsMotionPlaying = false;
            HitWindowOpen = false;
            if (boss != null) { boss.Animation?.StopMotion(); boss.Movement?.Stop(); }
            hitTargets.Clear();
        }

        public void ClearProjectiles()
        {
            foreach (var projectile in projectiles) if (projectile != null) projectile.Despawn();
            projectiles.Clear();
        }

        private void OnDisable() { StopMotion(); ClearProjectiles(); }

        private void OnDrawGizmosSelected()
        {
            if (!HitWindowOpen || motion == null) return;
            Gizmos.color = Color.red;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(motion.hitCenter, motion.hitHalfExtents * 2f);
        }
    }
}
