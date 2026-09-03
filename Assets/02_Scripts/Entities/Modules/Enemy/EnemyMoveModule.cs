using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMoveModule : MonoBehaviour, IMoveStrategy
{
    private NavMeshAgent agent;

    public Vector3 MoveInfo { get; set; }

    [SerializeField] private float angularSpeed = 360f;
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.angularSpeed = angularSpeed;
    }

    public void Move()
    {
        agent.isStopped = false;
        agent.SetDestination(MoveInfo);
    }

    public void Stop()
    {
        if (agent == null || !agent.isOnNavMesh)
            return;

        agent.isStopped = true;
        agent.ResetPath();
    }

    public bool HasReachedDestination()
    {
        if (agent.pathPending)
            return false;

        if (agent.remainingDistance >
            agent.stoppingDistance)
        {
            return false;
        }

        if (agent.hasPath &&
            agent.velocity.sqrMagnitude > 0.01f)
        {
            return false;
        }

        return true;
    }
}
