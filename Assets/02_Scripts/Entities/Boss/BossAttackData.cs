using System;
using UnityEngine;

namespace Tunic.BossCombat
{
    public enum BossAttackKind { Melee, Charge, Ranged }

    [Serializable]
    public sealed class BossMotion
    {
        public string animatorState;
        public AnimationClip clip;
        [Min(0.05f)] public float speed = 1f;
        [Range(0f, 1f)] public float hitStart = 0.2f;
        [Range(0f, 1f)] public float hitEnd = 0.6f;
        [Range(0f, 1f)] public float fireTime = 0.5f;
        [Min(0f)] public float damage = 10f;
        public Vector3 hitCenter = new Vector3(0f, 1.2f, 1.8f);
        public Vector3 hitHalfExtents = new Vector3(1f, 1.2f, 1.2f);
    }

    [CreateAssetMenu(menuName = "Boss/Attack Data")]
    public sealed class BossAttackData : ScriptableObject
    {
        public BossAttackKind kind;
        public bool available = true;
        [Min(0f)] public float minimumRange;
        [Min(0f)] public float maximumRange = 3.5f;
        [Min(0f)] public float comboBreakRange = 3.5f;
        [Min(0f)] public float weight = 1f;
        [Min(0f)] public float recovery = 0.7f;
        [Min(0f)] public float comboGap = 0.15f;
        [Min(0f)] public float preparation = 0.5f;
        public BossMotion[] motions = new BossMotion[0];
        [Header("Charge")]
        [Min(0.1f)] public float chargeSpeed = 12f;
        [Min(0.1f)] public float chargeDistance = 9f;
        [Header("Projectile")]
        public BossProjectile projectilePrefab;
        public Vector3 muzzleOffset = new Vector3(0f, 1.5f, 1.3f);
        [Min(0.1f)] public float projectileSpeed = 12f;
        [Min(0.1f)] public float projectileLifetime = 3f;

        public bool ContainsDistance(float distance) => distance >= minimumRange && distance <= maximumRange;
    }
}
