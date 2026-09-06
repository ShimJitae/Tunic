using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Back Step",
    description: "Moves the Agent away from the Target using NavMeshAgent.",
    story: "[Agent] back steps away from [Target] at [Speed]",
    category: "Action",
    id: "27d0e492f5071a60e9066a3307bc0edb")]
public partial class BackStepAction : Action
{
    [SerializeReference]
    public BlackboardVariable<GameObject> Agent;

    [SerializeReference]
    public BlackboardVariable<GameObject> Target;

    [SerializeReference]
    public BlackboardVariable<float> Speed =
        new BlackboardVariable<float>(4f);

    private NavMeshAgent navMeshAgent;

    private bool previousUpdateRotation;
    private bool previousIsStopped;

    protected override Status OnStart()
    {
        if (Agent == null || Agent.Value == null)
        {
            LogFailure("Back Step: Agent가 연결되지 않았습니다.");
            return Status.Failure;
        }

        if (Target == null || Target.Value == null)
        {
            LogFailure("Back Step: Target이 연결되지 않았습니다.");
            return Status.Failure;
        }

        navMeshAgent = Agent.Value.GetComponent<NavMeshAgent>();

        if (navMeshAgent == null)
        {
            LogFailure(
                $"Back Step: {Agent.Value.name}에 NavMeshAgent가 없습니다.");

            return Status.Failure;
        }

        if (!navMeshAgent.enabled || !navMeshAgent.isOnNavMesh)
        {
            LogFailure("Back Step: Agent가 NavMesh 위에 없습니다.");
            return Status.Failure;
        }

        // BackStep이 끝난 뒤 원래 설정으로 되돌리기 위해 보관한다.
        previousUpdateRotation = navMeshAgent.updateRotation;
        previousIsStopped = navMeshAgent.isStopped;

        // Navigate To Target이 만들었던 기존 경로를 제거한다.
        navMeshAgent.ResetPath();

        // 경로 이동은 멈추되 NavMeshAgent.Move를 사용할 수 있게 한다.
        navMeshAgent.isStopped = false;

        // 회전은 병렬로 실행 중인 Look At 노드가 담당한다.
        navMeshAgent.updateRotation = false;

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (navMeshAgent == null ||
            !navMeshAgent.enabled ||
            !navMeshAgent.isOnNavMesh)
        {
            return Status.Failure;
        }

        if (Target == null || Target.Value == null)
        {
            return Status.Failure;
        }

        Vector3 awayDirection =
            navMeshAgent.transform.position -
            Target.Value.transform.position;

        // 높이 차이를 제외하고 XZ 평면에서만 방향을 계산한다.
        awayDirection.y = 0f;

        // 보스와 플레이어의 중심이 겹쳤을 때를 대비한다.
        if (awayDirection.sqrMagnitude < 0.0001f)
        {
            awayDirection = -navMeshAgent.transform.forward;
            awayDirection.y = 0f;
        }

        if (awayDirection.sqrMagnitude < 0.0001f)
        {
            return Status.Running;
        }

        awayDirection.Normalize();

        float moveSpeed = Mathf.Max(0f, Speed.Value);

        // NavMesh 위에서 플레이어 반대 방향으로 이동한다.
        Vector3 movement =
            awayDirection * moveSpeed * Time.deltaTime;

        navMeshAgent.Move(movement);

        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (navMeshAgent == null)
        {
            return;
        }

        if (navMeshAgent.enabled && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.ResetPath();
            navMeshAgent.isStopped = previousIsStopped;
        }

        navMeshAgent.updateRotation = previousUpdateRotation;
        navMeshAgent = null;
    }
}