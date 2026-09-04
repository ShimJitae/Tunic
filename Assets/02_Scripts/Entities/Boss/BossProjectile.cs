using UnityEngine;

namespace Tunic.BossCombat
{
    public sealed class BossProjectile : MonoBehaviour
    {
        [SerializeField, Min(0.01f)] private float radius = 0.18f;
        private BossController owner;
        private Vector3 direction;
        private float damage;
        private float speed;
        private float expiresAt;
        private bool active;

        public void Initialize(BossController source, Vector3 heading, float hitDamage, float velocity, float lifetime)
        {
            owner = source;
            direction = heading.normalized;
            damage = hitDamage;
            speed = velocity;
            expiresAt = Time.time + lifetime;
            active = true;
        }

        private void Update()
        {
            if (!active) return;
            if (owner == null || !owner.isActiveAndEnabled || owner.IsDead || !owner.HasTarget || Time.time >= expiresAt)
            { Despawn(); return; }
            int mask = owner.Data.targetLayers | owner.Data.obstacleLayers;
            // Initial overlap also handles a player touching the muzzle.
            foreach (var collider in Physics.OverlapSphere(transform.position, radius, mask, QueryTriggerInteraction.Ignore))
            {
                if (Ignore(collider)) continue;
                Hit(collider);
                return;
            }
            float distance = speed * Time.deltaTime;
            RaycastHit? closest = null;
            foreach (var hit in Physics.SphereCastAll(transform.position, radius, direction, distance, mask, QueryTriggerInteraction.Ignore))
                if (!Ignore(hit.collider) && (!closest.HasValue || hit.distance < closest.Value.distance)) closest = hit;
            if (closest.HasValue) { Hit(closest.Value.collider); return; }
            transform.position += direction * distance;
        }

        private bool Ignore(Collider collider) => collider.transform.IsChildOf(owner.transform) || collider.transform.IsChildOf(transform);

        private void Hit(Collider collider)
        {
            if (((1 << collider.gameObject.layer) & owner.Data.targetLayers.value) != 0)
                collider.GetComponentInParent<IDamageable>()?.TakeDamage(damage);
            Despawn();
        }

        public void Despawn()
        {
            active = false;
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }
}
