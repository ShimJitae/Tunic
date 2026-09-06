using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMoveModule : MonoBehaviour
{
    private NavMeshAgent agent;

    [SerializeField] private float angularSpeed = 360f;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.angularSpeed = angularSpeed;
    }

    public void SetUpData(EnemyData enemyData)
    {
        agent.speed = enemyData.MoveSpeed;
    }

    public void MoveTo(Vector3 destination)
    {
        if (agent == null || !agent.isOnNavMesh)
            return;

        agent.isStopped = false;
        agent.SetDestination(destination);
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
        if (agent == null || !agent.isOnNavMesh)
            return false;

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
