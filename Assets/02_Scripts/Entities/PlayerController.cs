using UnityEngine;
using UnityEngine.InputSystem;
using UnityHFSM;

[RequireComponent(typeof(PlayerMoveModule))]
public class PlayerController : EntityController
{
    public override bool HasMoveInput => MoveModule.MoveInfo.sqrMagnitude > 0.01f;

    protected override void Awake()
    {
        base.Awake();

        if (GetComponent<PlayerMoveModule>() == null)
            gameObject.AddComponent<PlayerMoveModule>();

        MoveModule = GetComponent<PlayerMoveModule>();
    }

    protected override void RegisterSpecificStates()
    {
        Fsm.AddState(EntityStateId.Dodge, new DodgeState(this));
    }


    // =========================================================
    // Player Transition Registration
    // =========================================================

    protected override void RegisterSpecificTransitions()
    {
        RegisterAttackTransitions();

        RegisterDodgeTransitions();

        RegisterHitTransitions();
    }


    private void RegisterAttackTransitions()
    {
        // =====================================================
        // Attack Request
        // =====================================================
        Fsm.AddTriggerTransition(EntityEvent.Attack, new IdleAttackTransition(this));
        Fsm.AddTriggerTransition(EntityEvent.Attack, new MoveAttackTransition(this));
        Fsm.AddTriggerTransitionFromAny(EntityEvent.AttackFinished, new MoveIdleTransition(this));
    }

    private void RegisterDodgeTransitions()
    {
        // =====================================================
        // Dodge Request
        // =====================================================
        Fsm.AddTriggerTransition(EntityEvent.Dodge, new IdleDodgeTransition(this));
        Fsm.AddTriggerTransition(EntityEvent.Dodge, new MoveDodgeTransition(this));
        Fsm.AddTriggerTransitionFromAny(EntityEvent.DodgeFinished, new DodgeIdleTransition(this));
    }


    private void RegisterHitTransitions()
    {
        /*
         * Hit 상태가 끝났을 때도
         * 현재 입력 여부에 따라 복귀 상태를 결정.
         */

        Fsm.AddTriggerTransitionFromAny(
            EntityEvent.HitFinished,
            EntityStateId.Hit,
            EntityStateId.Move,
            _ => HasMoveInput
        );

        Fsm.AddTriggerTransitionFromAny(
            EntityEvent.HitFinished,
            EntityStateId.Hit,
            EntityStateId.Idle,
            _ => !HasMoveInput
        );
    }


    // =========================================================
    // Dodge Event
    // =========================================================

    public void NotifyDodgeFinished()
    {
        Fsm.Trigger(EntityEvent.DodgeFinished);
    }
}
