using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyController))]
public class EnemyBrain : MonoBehaviour
{
    private EnemyController enemyController;

    [Header("판단 거리")]
    [SerializeField, Min(0f)] private float detectionRange = 6f;
    [SerializeField, Min(0f)] private float giveUpRange = 7.5f;
    [SerializeField, Min(0f)] private float attackRange = 1.5f;
    [SerializeField, Min(0f)] private float attackCooltime = 1.5f;

    [Header("시야 설정")]
    [SerializeField, Range(0f, 360f)] private float viewAngle = 120f;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField, Min(0f)] private float eyeHeight = 1f;

    [Header("추적 대상")]
    [SerializeField] private Transform target;

    [Header("순찰")]
    [SerializeField] private List<Transform> patrolPoints = new();
    [SerializeField, Min(0f)] private float patrolWaitDuration = 2f;

    private float idleDuration;
    private int currentPatrolIndex = -1;

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
    }

    private void Update()
    {
        // 공통 Entity 상태가 Idle일 때의 판단
        if (enemyController.CurrentState == EntityStateId.Idle)
        {
            if (CanDetectTarget())
            {
                EnterChase();
                return;
            }

            UpdateIdle();
            return;
        }

        // Attack, Hit, Die 상태에서는 이동 판단을 하지 않음
        if (enemyController.CurrentState != EntityStateId.Move)
            return;

        // 순찰 중 플레이어를 발견하면 Chase로 변경
        if (enemyController.MoveType == EnemyMoveType.Patrol &&
            CanDetectTarget())
        {
            EnterChase();
            return;
        }

        switch (enemyController.MoveType)
        {
            case EnemyMoveType.Patrol:
                UpdatePatrol();
                break;

            case EnemyMoveType.Chase:
                UpdateChase();
                break;
        }
    }

    private void UpdateIdle()
    {
        idleDuration += Time.deltaTime;

        if (idleDuration < patrolWaitDuration)
            return;

        EnterNextPatrol();
    }

    private void UpdatePatrol()
    {
        if (!enemyController.HasReachedDestination())
            return;

        EnterIdle();
    }

    private void UpdateChase()
    {
        if (target == null)
        {
            EnterIdle();
            return;
        }

        float sqrDistance =
            (target.position - transform.position).sqrMagnitude;

        if (sqrDistance > giveUpRange * giveUpRange)
        {
            EnterIdle();
            return;
        }

        // 추적 대상은 움직이므로 목적지를 계속 갱신
        enemyController.SetMoveDestination(target.position);
    }

    private void EnterIdle()
    {
        idleDuration = 0f;

        // MoveType을 Idle로 바꾸지 않는다.
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

        enemyController.SetMoveType(
            EnemyMoveType.Patrol);

        enemyController.SetMoveDestination(
            patrolPoint.position);
    }

    private void EnterChase()
    {
        if (target == null)
            return;

        enemyController.SetMoveType(
            EnemyMoveType.Chase);

        enemyController.SetMoveDestination(
            target.position);
    }

    private bool CanDetectTarget()
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