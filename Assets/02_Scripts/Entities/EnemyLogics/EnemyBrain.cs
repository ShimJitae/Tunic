using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyController))]
public class EnemyBrain : MonoBehaviour
{
    private EnemyController enemyController;

    [Header("판단 거리")]
    [SerializeField, Min(0f)] private float detectionRange = 6f;
    [SerializeField, Min(0f)] private float giveUpRange = 7.5f;
    [Header("공격")]
    [SerializeField, Min(0f)] private float attackRange = 1.5f;
    [SerializeField, Min(0f)] private float attackCooltime = 1.5f;
    private float nextAttackTime;

    [Header("시야 설정")]
    [SerializeField, Range(0f, 360f)] private float viewAngle = 120f;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField, Min(0f)] private float eyeHeight = 1f;

    [Header("추적 대상")]
    [SerializeField] private Transform target;
    private float chaseUpdateDuration;

    [Header("순찰")]
    [SerializeField] private List<Transform> patrolPoints = new();
    [SerializeField, Min(0f)] private float patrolWaitDuration = 2f;
    private int currentPatrolIndex = -1;
    private float idleDuration;


    private void Start()
    {
        if (!TryGetComponent(out enemyController))
        {
#if UNITY_EDITOR
            Debug.LogError(
                $"{nameof(EnemyBrain)} requires a " +
                $"{nameof(EnemyController)} component.",
                this);
#endif
            enabled = false;
            return;
        }

        EnterIdle();

        target = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void Update()
    {
        if (enemyController.CurrentState != EntityStateId.Idle)
            return;

        if (CanDetectTarget())
        {
            EnterChase();
            return;
        }

        UpdateIdle();
    }

    private void UpdateIdle()
    {
        idleDuration += Time.deltaTime;

        if (idleDuration < patrolWaitDuration)
            return;

        EnterNextPatrol();
    }

    internal void UpdatePatrol()
    {
        if (!enemyController.HasReachedDestination())
            return;

        EnterIdle();
    }

    internal bool UpdateChase()
    {
        if (!ShouldUpdateChase())
            return false;

        if (target == null)
        {
            EnterIdle();
            return false;
        }

        float sqrDistance = (target.position - transform.position).sqrMagnitude;

        if (sqrDistance > giveUpRange * giveUpRange)
        {
            EnterIdle();
            return false;
        }

        if (TryRequestAttack(sqrDistance))
            return false;

        RefreshChaseDestination();
        return true;
    }

    private bool ShouldUpdateChase()
    {
        chaseUpdateDuration += Time.deltaTime;

        if (chaseUpdateDuration < 0.1f)
            return false;

        chaseUpdateDuration = 0f;
        return true;
    }

    private bool TryRequestAttack(float sqrDistance)
    {
        if (sqrDistance > attackRange * attackRange)
            return false;

        if (Time.time < nextAttackTime)
            return false;

        nextAttackTime = Time.time + attackCooltime;

        enemyController.StopMove();
        enemyController.RequestAttack();

        return true;
    }

    private void EnterIdle()
    {
        idleDuration = 0f;

        // MoveInfo를 비워서 기존 Transition이 Idle로 전환하게 한다.
        enemyController.StopMove();
    }

    private void EnterNextPatrol()
    {
        if (patrolPoints.Count == 0)
            return;

        currentPatrolIndex =
            (currentPatrolIndex + 1) % patrolPoints.Count;

        Transform patrolPoint =
            patrolPoints[currentPatrolIndex];

        if (patrolPoint == null)
            return;

        enemyController.RequestMoveState(
            EntityStateId.Patrol);

        enemyController.SetMoveDestination(
            patrolPoint.position);
    }

    private void EnterChase()
    {
        if (target == null)
            return;

        enemyController.RequestMoveState(EntityStateId.Chase);

        RefreshChaseDestination();
    }

    internal void RefreshChaseDestination()
    {
        if (target == null)
            return;

        chaseUpdateDuration = 0f;
        enemyController.SetMoveDestination(target.position);
    }

    internal bool CanDetectTarget()
    {
        if (target == null)
            return false;

        float sqrDistance =
            (target.position - transform.position).sqrMagnitude;

        if (sqrDistance >
            detectionRange * detectionRange)
        {
            return false;
        }

        return IsInViewAngle() && HasLineOfSight();
    }

    // 기존 IsInViewAngle() 그대로 사용
    private bool IsInViewAngle()
    {
        Vector3 directionToTarget =
            target.position - transform.position;

        directionToTarget.y = 0f;

        if (directionToTarget.sqrMagnitude <= Mathf.Epsilon)
            return true;

        Vector3 forward = transform.forward;
        forward.y = 0f;

        float dot = Vector3.Dot(
            forward.normalized,
            directionToTarget.normalized);

        float threshold = Mathf.Cos(
            viewAngle * 0.5f * Mathf.Deg2Rad);

        return dot >= threshold;
    }

    // 기존 HasLineOfSight() 그대로 사용
    private bool HasLineOfSight()
    {
        Vector3 origin =
            transform.position + Vector3.up * eyeHeight;

        Vector3 toTarget =
            target.position - origin;

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
