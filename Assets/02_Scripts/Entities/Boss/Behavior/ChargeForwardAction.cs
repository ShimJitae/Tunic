using System;
using DG.Tweening;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Charge Forward",
    story: "[Agent] charges forward [ChargeDistance] units over [ChargeDuration] seconds",
    category: "Action",
    id: "d752c02d8c322bd9057e0fab7912e9dc")]
public partial class ChargeForwardAction : Action
{
    [SerializeReference]
    public BlackboardVariable<GameObject> Agent;

    [SerializeReference]
    public BlackboardVariable<float> ChargeDistance;

    [SerializeReference]
    public BlackboardVariable<float> ChargeDuration;

    private NavMeshAgent navMeshAgent;
    private Tween chargeTween;

    private Vector3 chargeDirection;
    private float safeDistance;
    private float movedDistance;

    private bool isCompleted;
    private bool previousIsStopped;
    private bool previousUpdateRotation;

    protected override Status OnStart()
    {
        if (Agent?.Value == null)
            return Status.Failure;

        navMeshAgent = Agent.Value.GetComponent<NavMeshAgent>();

        if (navMeshAgent == null ||
            !navMeshAgent.enabled ||
            !navMeshAgent.isOnNavMesh)
        {
            return Status.Failure;
        }

        chargeDirection = Agent.Value.transform.forward;
        chargeDirection.y = 0f;
        chargeDirection.Normalize();

        float distance = Mathf.Max(0f, ChargeDistance.Value);
        float duration = Mathf.Max(0.01f, ChargeDuration.Value);

        Vector3 startPosition = Agent.Value.transform.position;
        Vector3 endPosition =
            startPosition + chargeDirection * distance;

        safeDistance = distance;

        if (NavMesh.Raycast(
                startPosition,
                endPosition,
                out NavMeshHit hit,
                navMeshAgent.areaMask))
        {
            Vector3 toHit = hit.position - startPosition;
            toHit.y = 0f;

            safeDistance = Mathf.Clamp(
                Vector3.Dot(toHit, chargeDirection),
                0f,
                distance);
        }

        previousIsStopped = navMeshAgent.isStopped;
        previousUpdateRotation = navMeshAgent.updateRotation;

        navMeshAgent.ResetPath();
        navMeshAgent.isStopped = false;
        navMeshAgent.updateRotation = false;

        movedDistance = 0f;
        isCompleted = false;

        chargeTween = DOVirtual
            .Float(0f, distance, duration, currentDistance =>
            {
                float limitedDistance =
                    Mathf.Min(currentDistance, safeDistance);

                float deltaDistance =
                    limitedDistance - movedDistance;

                navMeshAgent.Move(
                    chargeDirection * deltaDistance);

                movedDistance = limitedDistance;
            })
            .SetEase(Ease.InQuad)
            .OnComplete(() => isCompleted = true);

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        return isCompleted
            ? Status.Success
            : Status.Running;
    }

    protected override void OnEnd()
    {
        if (chargeTween != null && chargeTween.IsActive())
            chargeTween.Kill();

        if (navMeshAgent != null &&
            navMeshAgent.enabled &&
            navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.ResetPath();
            navMeshAgent.isStopped = previousIsStopped;
            navMeshAgent.updateRotation = previousUpdateRotation;
        }

        chargeTween = null;
        navMeshAgent = null;
    }
}