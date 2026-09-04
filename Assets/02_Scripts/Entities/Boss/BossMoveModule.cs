using UnityEngine;
using UnityEngine.AI;

namespace Tunic.BossCombat
{
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class BossMoveModule : MonoBehaviour
    {
        private NavMeshAgent agent;
        private BossController boss;
        private NavMeshPath path;
        private readonly RaycastHit[] hits = new RaycastHit[32];
        private float nextPathTime;
        private int distanceMode;
        private Vector3 chargeDirection;
        private float remainingCharge;
        private bool chargeBlocked;
        public bool IsMoving => agent != null && agent.isOnNavMesh && agent.velocity.sqrMagnitude > 0.01f;
        public Vector3 ChargeDirection => chargeDirection;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            path = new NavMeshPath();
            boss = GetComponent<BossController>();
            agent.updateRotation = false;
        }

        public void FaceTarget(bool snap)
        {
            if (!boss.HasTarget) return;
            Vector3 direction = boss.Target.position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f) return;
            Quaternion desired = Quaternion.LookRotation(direction);
            transform.rotation = snap ? desired : Quaternion.RotateTowards(transform.rotation, desired, boss.Data.turnSpeed * Time.deltaTime);
        }

        public void MaintainDistance()
        {
            FaceTarget(false);
            if (agent == null || !agent.isOnNavMesh || !boss.HasTarget) return;
            float distance = boss.Distance;
            float center = (boss.Data.minimumDistance + boss.Data.maximumDistance) * 0.5f;
            if (distanceMode == -1 && distance >= center || distanceMode == 1 && distance <= center) distanceMode = 0;
            if (distanceMode == 0)
            {
                if (distance < boss.Data.minimumDistance) distanceMode = -1;
                else if (distance > boss.Data.maximumDistance) distanceMode = 1;
                else { StopAgent(); return; }
            }
            if (Time.time < nextPathTime) return;
            nextPathTime = Time.time + Mathf.Max(0.02f, boss.Data.decisionInterval);
            Vector3 away = transform.position - boss.Target.position;
            away.y = 0f;
            if (away.sqrMagnitude < 0.001f) away = -transform.forward;
            away.Normalize();
            agent.speed = distanceMode < 0 ? boss.Data.retreatSpeed : boss.Data.approachSpeed;
            Vector3 desired = boss.Target.position + away * center;
            if (TryDestination(desired, distanceMode < 0)) return;
            if (distanceMode < 0)
            {
                foreach (float angle in new[] { 45f, -45f, 75f, -75f })
                {
                    Vector3 direction = Quaternion.Euler(0f, angle, 0f) * away;
                    if (TryDestination(transform.position + direction * Mathf.Max(1f, center - distance), true)) return;
                }
            }
            StopAgent();
        }

        private bool TryDestination(Vector3 desired, bool retreating)
        {
            var filter = new NavMeshQueryFilter { agentTypeID = agent.agentTypeID, areaMask = agent.areaMask };
            if (!NavMesh.SamplePosition(desired, out var sample, 1.5f, filter)) return false;
            if (retreating && BossController.PlanarDistance(sample.position, boss.Target.position) <= boss.Distance + 0.05f) return false;
            if (!agent.CalculatePath(sample.position, path) || path.status != NavMeshPathStatus.PathComplete) return false;
            agent.isStopped = false;
            return agent.SetPath(path);
        }

        public void Stop()
        {
            distanceMode = 0;
            nextPathTime = 0f;
            StopAgent();
        }

        private void StopAgent()
        {
            if (agent == null || !agent.isOnNavMesh) return;
            agent.isStopped = true;
            agent.ResetPath();
        }

        public void BeginCharge(float distance)
        {
            Stop();
            chargeDirection = transform.forward;
            chargeDirection.y = 0f;
            chargeDirection.Normalize();
            remainingCharge = distance;
            chargeBlocked = false;
        }

        public void TickCharge(float speed)
        {
            if (chargeBlocked || remainingCharge <= 0f || agent == null || !agent.isOnNavMesh) return;
            float step = Mathf.Min(speed * Time.deltaTime, remainingCharge);
            Vector3 destination = transform.position + chargeDirection * step;
            if (agent.Raycast(destination, out var edge))
            {
                step = Mathf.Max(0f, BossController.PlanarDistance(transform.position, edge.position) - 0.05f);
                chargeBlocked = true;
            }
            float radius = agent.radius * 0.95f;
            Vector3 bottom = transform.position + Vector3.up * (radius + 0.08f);
            Vector3 top = transform.position + Vector3.up * Mathf.Max(radius + 0.08f, agent.height - radius);
            int count = Physics.CapsuleCastNonAlloc(bottom, top, radius, chargeDirection, hits, step + 0.05f,
                boss.Data.obstacleLayers, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                if (hits[i].collider.transform.IsChildOf(transform)) continue;
                step = Mathf.Min(step, Mathf.Max(0f, hits[i].distance - 0.05f));
                chargeBlocked = true;
            }
            if (step > 0f) agent.Move(chargeDirection * step);
            remainingCharge -= step;
        }

        private void OnDisable() => Stop();
    }
}
