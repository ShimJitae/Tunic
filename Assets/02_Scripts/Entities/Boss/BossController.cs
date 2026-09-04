using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;

namespace Tunic.BossCombat
{
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(Health), typeof(BehaviorGraphAgent))]
    [RequireComponent(typeof(BossMoveModule), typeof(BossAnimationModule), typeof(BossAttackModule))]
    public sealed class BossController : MonoBehaviour
    {
        [SerializeField] private BossCombatData data;
        [SerializeField] private Transform target;
        [SerializeField] private BossSpecialAttack specialAttack;
        private Health targetHealth;
        private BehaviorGraphAgent graph;
        private readonly List<BossAttackData> candidates = new List<BossAttackData>();
        private readonly HashSet<BossAttackData> warnedAttacks = new HashSet<BossAttackData>();
        private BossAttackData lastAttack;
        private float nextCandidateTime;
        private bool previouslyHadTarget;
        private bool initialized;

        public BossCombatData Data => data;
        public Transform Target => target;
        public Health Health { get; private set; }
        public BossMoveModule Movement { get; private set; }
        public BossAnimationModule Animation { get; private set; }
        public BossAttackModule Attack { get; private set; }
        public BossAttackData SelectedAttack { get; private set; }
        public int MotionIndex { get; private set; }
        public bool ComboCancelled { get; private set; }
        public bool ComboActive { get; private set; }
        public float NextAttackTime { get; private set; }
        public bool SpecialPending { get; private set; }
        public bool SpecialUsed { get; private set; }
        public bool IsCastingSpecial { get; private set; }
        public BossTask CurrentTask { get; internal set; }
        public bool IsDead => Health != null && (Health.IsDied || initialized && Health.CurrHP <= 0f);
        public bool HasTarget => target != null && target.gameObject.activeInHierarchy &&
            targetHealth != null && !targetHealth.IsDied && targetHealth.CurrHP > 0f;
        public bool Ready => initialized && data != null;
        public bool CanStartSpecial => SpecialPending && !SpecialUsed && !IsDead && HasTarget &&
            specialAttack != null && specialAttack.IsAvailable && !Attack.IsMotionPlaying;
        public float Distance => HasTarget ? PlanarDistance(transform.position, target.position) : float.PositiveInfinity;
        public BossMotion CurrentMotion => SelectedAttack != null && MotionIndex < SelectedAttack.motions.Length ? SelectedAttack.motions[MotionIndex] : null;
        public bool HasNextMotion => SelectedAttack != null && MotionIndex < SelectedAttack.motions.Length && !ComboCancelled;

        private void Awake()
        {
            Health = GetComponent<Health>();
            graph = GetComponent<BehaviorGraphAgent>();
            Movement = GetComponent<BossMoveModule>();
            Animation = GetComponent<BossAnimationModule>();
            Attack = GetComponent<BossAttackModule>();
        }

        private void OnEnable()
        {
            Health.OnDamaged += OnDamaged;
            Health.OnDied += OnDied;
            if (initialized)
            {
                NextAttackTime = Time.time + data.attackInterval;
                graph.enabled = true;
                graph.Restart();
            }
        }

        private void Start()
        {
            if (data == null)
            {
                Debug.LogError("Boss Combat Data is not assigned.", this);
                graph.enabled = false;
                enabled = false;
                return;
            }
            Health.SetUpData(data);
            if (target == null) target = GameObject.FindGameObjectWithTag("Player")?.transform;
            SetTarget(target);
            initialized = true;
            NextAttackTime = Time.time + data.attackInterval;
            PublishBlackboard();
        }

        public void SetTarget(Transform value)
        {
            target = value;
            targetHealth = target == null ? null : target.GetComponentInParent<Health>();
        }

        private void Update()
        {
            if (!Ready) return;
            bool hasTarget = HasTarget;
            if (previouslyHadTarget && !hasTarget) InterruptCombat(true);
            if (!previouslyHadTarget && hasTarget) NextAttackTime = Time.time + data.attackInterval;
            previouslyHadTarget = hasTarget;
            ObserveComboDistance();
            PublishBlackboard();
        }

        public void ObserveComboDistance()
        {
            if (ComboActive && SelectedAttack != null && (!HasTarget || Distance > SelectedAttack.comboBreakRange))
                ComboCancelled = true; // Latched for the entire combo, including gaps.
        }

        public bool AttackOpportunity()
        {
            if (!Ready || !HasTarget || IsDead || Time.time < NextAttackTime) return false;
            if (Time.time < nextCandidateTime) return candidates.Count > 0;
            nextCandidateTime = Time.time + Mathf.Max(0.02f, data.decisionInterval);
            CollectCandidates();
            return candidates.Count > 0;
        }

        private void CollectCandidates()
        {
            candidates.Clear();
            if (!HasTarget || !HasLineOfSight()) return;
            foreach (var attack in data.attacks)
            {
                if (attack == null || !attack.available || attack.weight <= 0f || !attack.ContainsDistance(Distance)) continue;
                if (!Attack.IsValid(attack))
                {
                    if (warnedAttacks.Add(attack)) Debug.LogWarning($"Boss attack '{attack.name}' has missing or invalid motion/projectile data; excluded.", attack);
                    continue;
                }
                candidates.Add(attack);
            }
        }

        public bool SelectAttack()
        {
            CollectCandidates(); // Revalidate at commitment, never reuse a stale target/range.
            if (candidates.Count == 0) return false;
            float total = 0f;
            foreach (var candidate in candidates) total += CandidateWeight(candidate);
            float roll = Random.value * total;
            SelectedAttack = candidates[candidates.Count - 1];
            foreach (var candidate in candidates)
            {
                roll -= CandidateWeight(candidate);
                if (roll <= 0f) { SelectedAttack = candidate; break; }
            }
            MotionIndex = 0;
            ComboCancelled = false;
            ComboActive = SelectedAttack.kind == BossAttackKind.Melee;
            return true;
        }

        private float CandidateWeight(BossAttackData candidate) => candidate.weight *
            (candidates.Count > 1 && candidate == lastAttack ? data.repeatedAttackWeight : 1f);

        public bool HasLineOfSight()
        {
            if (!HasTarget) return false;
            Vector3 start = transform.position + Vector3.up * 1.4f;
            Vector3 end = target.position + Vector3.up;
            Vector3 delta = end - start;
            foreach (var hit in Physics.RaycastAll(start, delta.normalized, delta.magnitude, data.obstacleLayers, QueryTriggerInteraction.Ignore))
                if (!hit.transform.IsChildOf(transform) && !hit.transform.IsChildOf(target)) return false;
            return true;
        }

        public void MarkMotionStarted() => lastAttack = SelectedAttack;
        public void AdvanceMotion() { MotionIndex++; ObserveComboDistance(); }
        public void CancelCombo() => ComboCancelled = true;
        public void FinishCombo() => ComboActive = false;
        public void FinishAttack()
        {
            ComboActive = false;
            SelectedAttack = null;
            NextAttackTime = Time.time + data.attackInterval;
            candidates.Clear();
        }

        private void OnDamaged(float amount)
        {
            if (Health.CurrHP > 0f && Health.CurrHP < Health.MaxHP * 0.5f && !SpecialUsed) SpecialPending = true;
        }

        private void OnDied() => InterruptCombat(true);

        public bool BeginSpecial()
        {
            if (!CanStartSpecial) return false;
            InterruptCombat(false);
            SelectedAttack = null;
            SpecialPending = false;
            SpecialUsed = true;
            IsCastingSpecial = true;
            Health.IsInvincible = true;
            try { specialAttack.Begin(this); }
            catch (System.Exception exception)
            {
                Debug.LogException(exception, specialAttack);
                EndSpecial(true);
                return false;
            }
            return true;
        }

        public bool TickSpecial()
        {
            if (!IsCastingSpecial || specialAttack == null || !specialAttack.IsAvailable) return true;
            try { return specialAttack.Tick(Time.deltaTime); }
            catch (System.Exception exception) { Debug.LogException(exception, specialAttack); return true; }
        }

        public void EndSpecial(bool interrupted)
        {
            bool wasCasting = IsCastingSpecial;
            IsCastingSpecial = false;
            if (Health != null) Health.IsInvincible = false;
            if (!wasCasting || specialAttack == null) return;
            try { specialAttack.End(interrupted); }
            catch (System.Exception exception) { Debug.LogException(exception, specialAttack); }
        }

        public void InterruptCombat(bool removeProjectiles)
        {
            ComboActive = false;
            ComboCancelled = true;
            Movement?.Stop();
            Attack?.StopMotion();
            if (removeProjectiles) Attack?.ClearProjectiles();
            EndSpecial(true);
        }

        private void OnDisable()
        {
            Health.OnDamaged -= OnDamaged;
            Health.OnDied -= OnDied;
            InterruptCombat(true);
            if (graph != null) { graph.End(); graph.enabled = false; }
        }

        private void PublishBlackboard()
        {
            if (graph == null || graph.Graph == null) return;
            graph.SetVariableValue("Target", HasTarget ? target.gameObject : null);
            graph.SetVariableValue("SelectedAttack", SelectedAttack);
            graph.SetVariableValue("MotionIndex", MotionIndex);
            graph.SetVariableValue("ComboCancelled", ComboCancelled);
            graph.SetVariableValue("NextAttackTime", NextAttackTime);
            graph.SetVariableValue("SpecialPending", SpecialPending);
            graph.SetVariableValue("SpecialUsed", SpecialUsed);
        }

        public static float PlanarDistance(Vector3 a, Vector3 b)
        {
            a.y = b.y = 0f;
            return Vector3.Distance(a, b);
        }

        private void OnDrawGizmosSelected()
        {
            if (data == null) return;
            Gizmos.color = Color.cyan;
            DrawCircle(data.minimumDistance);
            DrawCircle(data.maximumDistance);
            if (SelectedAttack != null) { Gizmos.color = Color.red; DrawCircle(SelectedAttack.comboBreakRange); }
        }

        private void DrawCircle(float radius)
        {
            Vector3 previous = transform.position + Vector3.forward * radius;
            for (int i = 1; i <= 48; i++)
            {
                Vector3 next = transform.position + Quaternion.Euler(0f, i * 7.5f, 0f) * Vector3.forward * radius;
                Gizmos.DrawLine(previous, next);
                previous = next;
            }
        }
    }
}
