using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : EntityData
{
    [Header("판단 거리")]
    [SerializeField, Min(0f)] private float detectionRange = 6f;
    public float DetectionRange => detectionRange;
    [SerializeField, Min(0f)] private float giveUpRange = 7.5f;
    public float GiveUpRange => giveUpRange;

    [Header("공격")]
    [SerializeField, Min(0f)] private float attackRange = 1.5f;
    public float AttackRange => attackRange;
    [SerializeField, Min(0f)] private float attackCooltime = 1.5f;
    public float AttackCooltime => attackCooltime;

    [Header("시야 설정")]
    [SerializeField, Range(0f, 360f)] private float viewAngle = 120f;
    public float ViewAngle => viewAngle;
    [SerializeField] private LayerMask obstacleMask;
    public LayerMask ObstacleMask => obstacleMask;
    [SerializeField, Min(0f)] private float eyeHeight = 1f;
    public float EyeHeight => eyeHeight;

    [Header("순찰")]
    [SerializeField, Min(0f)] private float patrolWaitDuration = 2f;
    public float PatrolWaitDuration => patrolWaitDuration;
}
