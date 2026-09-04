using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace Tunic.BossCombat
{
    [BlackboardEnum]
    public enum BossTask
    {
        WaitForOpportunity, SelectAttack, CheckComboRange, FaceTarget, PlayOneMotion,
        AdvanceMotion, ComboGap, PrepareAttack, Recovery, ScheduleNextAttack,
        CastSpecial, WaitForTarget, Die
    }

    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Boss Action", story: "[Task]", category: "Boss", id: "1460d7dca370440c8fc85ca468ed11b0")]
    public partial class BossAction : Action
    {
        [SerializeReference] public BlackboardVariable<BossTask> Task = new BlackboardVariable<BossTask>();
        private BossController boss;
        private float endTime;
        private bool completed;

        protected override Status OnStart()
        {
            boss = GameObject.GetComponent<BossController>();
            completed = false;
            if (boss == null) return Status.Failure;
            boss.CurrentTask = Task.Value;
            switch (Task.Value)
            {
                case BossTask.WaitForOpportunity: return Status.Running;
                case BossTask.SelectAttack: return boss.SelectAttack() ? Status.Success : Status.Failure;
                case BossTask.CheckComboRange:
                    boss.ObserveComboDistance();
                    return boss.HasTarget && boss.HasNextMotion ? Status.Success : Status.Failure;
                case BossTask.FaceTarget: boss.Movement.FaceTarget(true); return Status.Success;
                case BossTask.PlayOneMotion:
                    if (boss.Attack.BeginMotion()) return Status.Running;
                    boss.CancelCombo(); return Status.Failure;
                case BossTask.AdvanceMotion: boss.AdvanceMotion(); return Status.Success;
                case BossTask.ComboGap:
                    if (!boss.HasNextMotion) return Status.Success;
                    endTime = Time.time + boss.SelectedAttack.comboGap;
                    return Status.Running;
                case BossTask.PrepareAttack:
                    boss.Movement.Stop(); boss.Animation.Locomotion(false);
                    endTime = Time.time + boss.SelectedAttack.preparation;
                    return Status.Running;
                case BossTask.Recovery:
                    boss.FinishCombo(); boss.Movement.Stop(); boss.Animation.Locomotion(false);
                    endTime = Time.time + (boss.SelectedAttack != null ? boss.SelectedAttack.recovery : 0.7f);
                    return Status.Running;
                case BossTask.ScheduleNextAttack: boss.FinishAttack(); return Status.Success;
                case BossTask.CastSpecial: return boss.BeginSpecial() ? Status.Running : Status.Failure;
                case BossTask.WaitForTarget:
                    boss.InterruptCombat(true); boss.Animation.Locomotion(false); return Status.Running;
                case BossTask.Die:
                    boss.InterruptCombat(true); boss.Animation.Die(); return Status.Running;
                default: return Status.Failure;
            }
        }

        protected override Status OnUpdate()
        {
            switch (Task.Value)
            {
                case BossTask.WaitForOpportunity:
                    if (!boss.Ready) return Status.Running;
                    if (boss.AttackOpportunity()) return Status.Success;
                    boss.Movement.MaintainDistance(); boss.Animation.Locomotion(boss.Movement.IsMoving);
                    return Status.Running;
                case BossTask.PlayOneMotion:
                    if (!boss.Attack.TickMotion()) return Status.Running;
                    completed = true; return Status.Success;
                case BossTask.ComboGap:
                    boss.ObserveComboDistance();
                    if (boss.ComboCancelled) return Status.Success;
                    return Time.time >= endTime ? Status.Success : Status.Running;
                case BossTask.PrepareAttack:
                    boss.Movement.FaceTarget(false);
                    return Time.time >= endTime ? Status.Success : Status.Running;
                case BossTask.Recovery: return Time.time >= endTime ? Status.Success : Status.Running;
                case BossTask.CastSpecial:
                    if (!boss.TickSpecial()) return Status.Running;
                    completed = true; return Status.Success;
                case BossTask.WaitForTarget:
                case BossTask.Die: return Status.Running;
                default: return Status.Success;
            }
        }

        protected override void OnEnd()
        {
            if (boss == null) return;
            if (Task.Value == BossTask.PlayOneMotion) boss.Attack.StopMotion();
            if (Task.Value == BossTask.CastSpecial) boss.EndSpecial(!completed);
            if (Task.Value == BossTask.WaitForOpportunity || Task.Value == BossTask.PrepareAttack) boss.Movement.Stop();
        }
    }

    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Boss Priority", story: "Death > No Target > Special > Combat", category: "Boss", id: "4fb7f5c338ec4caea3fe92395d27f499")]
    public partial class BossPriority : Composite
    {
        [SerializeReference] public Node Death;
        [SerializeReference] public Node NoTarget;
        [SerializeReference] public Node Special;
        [SerializeReference] public Node Combat;
        private Node active;
        private BossController boss;

        protected override Status OnStart()
        {
            boss = GameObject.GetComponent<BossController>();
            active = null;
            return boss == null ? Status.Failure : OnUpdate();
        }

        protected override Status OnUpdate()
        {
            Node desired = boss.IsDead ? Death : !boss.Ready || !boss.HasTarget ? NoTarget :
                active == Special && !Finished(active) || boss.CanStartSpecial ? Special : Combat;
            if (active != desired)
            {
                if (active != null && !Finished(active)) EndNode(active);
                boss.InterruptCombat(desired == Death || desired == NoTarget);
                active = desired;
                if (active != null) StartNode(active);
            }
            else if (active != null && Finished(active))
            {
                // Start the next complete cycle on a later tick, never spin in one frame.
                active = null;
            }
            return Status.Running;
        }

        private static bool Finished(Node node) => node != null &&
            (node.CurrentStatus == Status.Success || node.CurrentStatus == Status.Failure || node.CurrentStatus == Status.Interrupted);

        protected override void OnEnd()
        {
            if (active != null && !Finished(active)) EndNode(active);
            boss?.InterruptCombat(true);
            active = null;
        }
    }

    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Boss Attack Branch", story: "Selected attack kind", category: "Boss", id: "38c5528ebaa74f809c21555ea9a1c36e")]
    public partial class BossAttackBranch : Composite
    {
        [SerializeReference] public Node Melee;
        [SerializeReference] public Node Charge;
        [SerializeReference] public Node Ranged;
        private Node active;

        protected override Status OnStart()
        {
            var boss = GameObject.GetComponent<BossController>();
            if (boss.SelectedAttack == null) return Status.Success;
            active = boss.SelectedAttack.kind == BossAttackKind.Melee ? Melee : boss.SelectedAttack.kind == BossAttackKind.Charge ? Charge : Ranged;
            if (active == null) return Status.Success;
            StartNode(active);
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            // Failure still proceeds through recovery/cooldown in the parent sequence.
            return active == null || active.CurrentStatus == Status.Success || active.CurrentStatus == Status.Failure ? Status.Success : Status.Running;
        }

        protected override void OnEnd()
        {
            if (active != null && (active.CurrentStatus == Status.Running || active.CurrentStatus == Status.Waiting)) EndNode(active);
        }
    }

    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Boss Combo Loop", story: "Repeat each motion until complete or cancellation latched", category: "Boss", id: "20c47c3f30d34ce1b0362a0ff4b7454b")]
    public partial class BossComboLoop : Modifier
    {
        private BossController boss;
        protected override Status OnStart()
        {
            boss = GameObject.GetComponent<BossController>();
            if (Child == null || !boss.HasNextMotion) return Status.Success;
            StartNode(Child);
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (Child.CurrentStatus == Status.Failure) return Status.Success;
            if (Child.CurrentStatus != Status.Success) return Status.Running;
            boss.ObserveComboDistance();
            if (!boss.HasNextMotion || boss.CanStartSpecial) return Status.Success;
            StartNode(Child);
            return Status.Running;
        }

        protected override void OnEnd()
        {
            if (Child != null && (Child.CurrentStatus == Status.Running || Child.CurrentStatus == Status.Waiting)) EndNode(Child);
            boss?.FinishCombo();
        }
    }
}
