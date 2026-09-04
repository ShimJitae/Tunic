using UnityEngine;

namespace Tunic.BossCombat
{
    [CreateAssetMenu(menuName = "Boss/Combat Data")]
    public sealed class BossCombatData : EntityData
    {
        [Header("Distance (XZ center to center)")]
        [Min(0f)] public float minimumDistance = 3f;
        [Min(0f)] public float maximumDistance = 4f;
        [Min(0f)] public float retreatSpeed = 2f;
        [Min(0f)] public float approachSpeed = 4f;
        [Min(0f)] public float turnSpeed = 360f;
        [Min(0.02f)] public float decisionInterval = 0.1f;
        [Min(0f)] public float attackInterval = 2.5f;
        [Range(0f, 1f)] public float repeatedAttackWeight = 0.25f;
        public LayerMask targetLayers;
        public LayerMask obstacleLayers;
        public BossAttackData[] attacks = new BossAttackData[0];
    }
}
