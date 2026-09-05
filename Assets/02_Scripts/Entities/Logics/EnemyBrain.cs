using System.Collections.Generic;
using UnityEngine;

public class EnemyBrain : MonoBehaviour
{
    private const float ChaseUpdateInterval = 0.1f;

    [Header("순찰 지점")]
    [SerializeField] private List<Transform> patrolPoints = new();

    private Transform target;

    private float detectionRange;
    private float giveUpRange;
    private float attackRange;
    private float attackCooltime;
    private float viewAngle;
    private LayerMask obstacleMask;
    private float eyeHeight;
    private float patrolWaitDuration;

    private int currentPatrolIndex = -1;
    private float idleEndTime;
    private float nextAttackTime;

    private bool hasChaseSnapshot;
    private float nextChaseUpdateTime;
    private bool shouldGiveUpTarget;
    private bool canAttack;
    private bool hasChaseDestination;
    private Vector3 chaseDestination;

    internal bool HasPatrolPoint
    {
        get
        {
            foreach (Transform patrolPoint in patrolPoints)
            {
                if (patrolPoint != null)
                    return true;
            }

            return false;
        }
    }

    private void Awake()
    {
        target = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    internal void SetUpData(EnemyData enemyData)
    {
        detectionRange = enemyData.DetectionRange;
        giveUpRange = enemyData.GiveUpRange;
        attackRange = enemyData.AttackRange;
        attackCooltime = enemyData.AttackCooltime;
        viewAngle = enemyData.ViewAngle;
        obstacleMask = enemyData.ObstacleMask;
        eyeHeight = enemyData.EyeHeight;
        patrolWaitDuration = enemyData.PatrolWaitDuration;
    }

    internal void BeginIdle()
    {
        idleEndTime = Time.time + patrolWaitDuration;
        hasChaseSnapshot = false;
        hasChaseDestination = false;
    }

    internal bool IsIdleWaitComplete()
    {
        return Time.time >= idleEndTime;
    }

    internal void BeginChase()
    {
        hasChaseSnapshot = false;
        hasChaseDestination = false;
        nextChaseUpdateTime = 0f;
    }

    internal bool CanDetectTarget()
    {
        if (!TryGetSqrDistanceToTarget(out float sqrDistance))
            return false;

        if (sqrDistance > detectionRange * detectionRange)
            return false;

        return IsInViewAngle() && HasLineOfSight();
    }

    internal bool ShouldGiveUpTarget()
    {
        RefreshChaseSnapshot();
        return shouldGiveUpTarget;
    }

    internal bool CanAttack()
    {
        RefreshChaseSnapshot();
        return canAttack;
    }

    internal bool TryGetChaseDestination(out Vector3 destination)
    {
        RefreshChaseSnapshot();

        if (!hasChaseDestination)
        {
            destination = default;
            return false;
        }

        destination = chaseDestination;
        hasChaseDestination = false;
        return true;
    }

    internal void MarkAttackStarted()
    {
        nextAttackTime = Time.time + attackCooltime;
    }

    internal bool TryGetTargetPosition(out Vector3 targetPosition)
    {
        if (target == null)
        {
            targetPosition = default;
            return false;
        }

        targetPosition = target.position;
        return true;
    }

    internal bool TryGetNextPatrolDestination(out Vector3 destination)
    {
        for (int i = 0; i < patrolPoints.Count; i++)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Count;
            Transform patrolPoint = patrolPoints[currentPatrolIndex];

            if (patrolPoint == null)
                continue;

            destination = patrolPoint.position;
            return true;
        }

        destination = default;
        return false;
    }

    private void RefreshChaseSnapshot()
    {
        if (hasChaseSnapshot && Time.time < nextChaseUpdateTime)
            return;

        hasChaseSnapshot = true;
        nextChaseUpdateTime = Time.time + ChaseUpdateInterval;
        hasChaseDestination = false;

        if (!TryGetSqrDistanceToTarget(out float sqrDistance))
        {
            shouldGiveUpTarget = true;
            canAttack = false;
            return;
        }

        shouldGiveUpTarget = sqrDistance > giveUpRange * giveUpRange;
        canAttack = !shouldGiveUpTarget
            && Time.time >= nextAttackTime
            && sqrDistance <= attackRange * attackRange;

        if (shouldGiveUpTarget)
            return;

        chaseDestination = target.position;
        hasChaseDestination = true;
    }

    private bool TryGetSqrDistanceToTarget(out float sqrDistance)
    {
        if (target == null)
        {
            sqrDistance = default;
            return false;
        }

        sqrDistance = (target.position - transform.position).sqrMagnitude;
        return true;
    }

    private bool IsInViewAngle()
    {
        Vector3 directionToTarget = target.position - transform.position;
        directionToTarget.y = 0f;

        if (directionToTarget.sqrMagnitude <= Mathf.Epsilon)
            return true;

        Vector3 forward = transform.forward;
        forward.y = 0f;

        float dot = Vector3.Dot(
            forward.normalized,
            directionToTarget.normalized);

        float threshold = Mathf.Cos(viewAngle * 0.5f * Mathf.Deg2Rad);
        return dot >= threshold;
    }

    private bool HasLineOfSight()
    {
        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 toTarget = target.position - origin;
        float distanceToTarget = toTarget.magnitude;

        if (distanceToTarget <= Mathf.Epsilon)
            return true;

        return !Physics.Raycast(
            origin,
            toTarget / distanceToTarget,
            distanceToTarget,
            obstacleMask,
            QueryTriggerInteraction.Ignore);
    }
}
