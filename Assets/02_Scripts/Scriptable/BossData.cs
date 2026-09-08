using UnityEngine;

[CreateAssetMenu(fileName = "BossData", menuName = "Scriptable Objects/BossData")]
public class BossData : EntityData
{
    [Header("공격")]
    [SerializeField, Min(0f)] private float meleeAttackRange = 3.5f;
    public float MeleeAttackRange => meleeAttackRange;
    [SerializeField, Min(0f)] private float attackCooldown = 4f;
    public float AttackCooldown => attackCooldown;
    [SerializeField, Min(0f)] private float chargeDistance = 10f; // 돌진 공격 거리
    public float ChargeDistance => chargeDistance;
    [SerializeField, Min(0f)] private float chargeDuration = 1f; // 돌진 진행할 시간
    public float ChargeDuration => chargeDuration;
    [Header("움직임")]
    [SerializeField, Min(0f)] private float chaseDistance = 2.5f; // 플레이어와 유지할 일정 거리
    public float ChaseDistance => chaseDistance;
}
