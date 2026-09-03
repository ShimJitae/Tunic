using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : EntityData
{
    [Header("판단 거리")]
    [SerializeField, Min(0f)] private float detectionRange = 6f;
    [SerializeField, Min(0f)] private float giveUpRange = 7.5f;
    [Header("공격")]
    [SerializeField, Min(0f)] private float attackRange = 1.5f;
    [SerializeField, Min(0f)] private float attackCooltime = 1.5f;

    [Header("시야 설정")]
    [SerializeField, Range(0f, 360f)] private float viewAngle = 120f;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField, Min(0f)] private float eyeHeight = 1f;

    [Header("추적 대상")]
    [SerializeField] private Transform target;

    [Header("순찰")]
    [SerializeField, Min(0f)] private float patrolWaitDuration = 2f;
}
