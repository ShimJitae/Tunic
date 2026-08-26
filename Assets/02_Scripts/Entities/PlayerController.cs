using UnityEngine;
using UnityEngine.InputSystem;
using UnityHFSM;

public class PlayerController : EntityController
{
    private Vector2 moveInput;


    // =========================================================
    // Conditions
    // =========================================================

    protected override bool HasMoveInput =>
        moveInput.sqrMagnitude > 0.01f;


    // =========================================================
    // Input
    // =========================================================

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        /*
         * bool WantsAttack = true;
         *
         * 같은 방식으로 저장하지 않습니다.
         *
         * "공격 버튼을 눌렀다"는 사건이므로 Trigger.
         */
        Fsm.Trigger(EntityEvent.Attack);
    }

    public void OnDodge(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        Fsm.Trigger(EntityEvent.Dodge);
    }


    // =========================================================
    // Player State Registration
    // =========================================================

    protected override void RegisterSpecificStates()
    {
        Fsm.AddState(
            EntityStateId.Dodge,
            new State<EntityStateId, EntityEvent>(
                onEnter: _ => EnterDodge(),
                onLogic: _ => UpdateDodge(),
                onExit: _ => ExitDodge()
            )
        );
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

        Fsm.AddTriggerTransition(
            EntityEvent.Attack,
            EntityStateId.Idle,
            EntityStateId.Attack
        );

        Fsm.AddTriggerTransition(
            EntityEvent.Attack,
            EntityStateId.Move,
            EntityStateId.Attack
        );


        // =====================================================
        // Attack Finished
        // =====================================================

        /*
         * 공격 종료 후 이동 입력이 있으면 Move
         */
        Fsm.AddTriggerTransition(
            EntityEvent.AttackFinished,
            EntityStateId.Attack,
            EntityStateId.Move,
            _ => HasMoveInput
        );


        /*
         * 없으면 Idle
         */
        Fsm.AddTriggerTransition(
            EntityEvent.AttackFinished,
            EntityStateId.Attack,
            EntityStateId.Idle,
            _ => !HasMoveInput
        );
    }


    private void RegisterDodgeTransitions()
    {
        // =====================================================
        // Dodge Request
        // =====================================================

        Fsm.AddTriggerTransition(
            EntityEvent.Dodge,
            EntityStateId.Idle,
            EntityStateId.Dodge
        );

        Fsm.AddTriggerTransition(
            EntityEvent.Dodge,
            EntityStateId.Move,
            EntityStateId.Dodge
        );


        // =====================================================
        // Dodge Finished
        // =====================================================

        Fsm.AddTriggerTransition(
            EntityEvent.DodgeFinished,
            EntityStateId.Dodge,
            EntityStateId.Move,
            _ => HasMoveInput
        );

        Fsm.AddTriggerTransition(
            EntityEvent.DodgeFinished,
            EntityStateId.Dodge,
            EntityStateId.Idle,
            _ => !HasMoveInput
        );
    }


    private void RegisterHitTransitions()
    {
        /*
         * Hit 상태가 끝났을 때도
         * 현재 입력 여부에 따라 복귀 상태를 결정.
         */

        Fsm.AddTriggerTransition(
            EntityEvent.HitFinished,
            EntityStateId.Hit,
            EntityStateId.Move,
            _ => HasMoveInput
        );

        Fsm.AddTriggerTransition(
            EntityEvent.HitFinished,
            EntityStateId.Hit,
            EntityStateId.Idle,
            _ => !HasMoveInput
        );
    }


    // =========================================================
    // Common State Implementation
    // =========================================================

    protected override void EnterIdle()
    {
        // animator.Play("Idle");
    }

    protected override void EnterMove()
    {
        // animator.Play("Run");
    }

    protected override void UpdateMove()
    {
        /*
         * 실제 구현에서는:
         *
         * movementModule.Move(moveInput);
         */
    }


    protected override void EnterAttack()
    {
        /*
         * attackModule.Execute();
         * animator.Play("Attack");
         */
    }

    protected override void EnterHit()
    {
        /*
         * movementModule.Stop();
         * animator.Play("Hit");
         */
    }

    protected override void EnterDead()
    {
        /*
         * movementModule.Stop();
         * animator.Play("Dead");
         */
    }


    // =========================================================
    // Dodge State
    // =========================================================

    private void EnterDodge()
    {
        /*
         * dodgeModule.Execute(moveInput);
         * animator.Play("Dodge");
         */
    }

    private void UpdateDodge()
    {
    }

    private void ExitDodge()
    {
    }


    // =========================================================
    // Dodge Event
    // =========================================================

    public void NotifyDodgeFinished()
    {
        Fsm.Trigger(EntityEvent.DodgeFinished);
    }
}