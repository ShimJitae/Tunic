using UnityEngine;
using UnityHFSM;

[RequireComponent(typeof(PlayerMoveModule))]
[RequireComponent(typeof(PlayerAnimationModule))]
public class PlayerController : EntityController
{
    private InputManager inputManager;
    private StateMachine<EntityStateId, EntityEvent> moveFsm;

    internal bool IsDodging => moveFsm != null && moveFsm.IsInitialized && moveFsm.ActiveStateName == EntityStateId.Dodge;

    public override bool ShouldExitMoveState => !IsDodging && base.ShouldExitMoveState;

    public override bool CanAttackFromMoveState => !IsDodging;

    protected override void Awake()
    {
        if (!TryGetComponent(out PlayerMoveModule playerMoveModule))
        {
            Debug.LogError($"{nameof(PlayerController)} requires a {nameof(PlayerMoveModule)} component.", this);
            enabled = false;
            return;
        }

        MoveModule = playerMoveModule;

        AnimationModule = GetComponentInChildren<PlayerAnimationModule>();
        if (AnimationModule == null)
        {
            Debug.LogError($"{nameof(PlayerController)} requires a {nameof(PlayerAnimationModule)} component.", this);
            enabled = false;
            return;
        }

        base.Awake();
    }

    protected override void Update()
    {
        if (inputManager == null)
            inputManager = InputManager.Instance;

        MoveModule.MoveInfo = inputManager != null ? inputManager.MoveInput : Vector3.zero;

        base.Update();
    }

    private void OnDisable()
    {
        if (MoveModule != null)
            MoveModule.MoveInfo = Vector3.zero;
    }

    protected override StateBase<EntityStateId> CreateMoveState()
    {
        moveFsm = new StateMachine<EntityStateId, EntityEvent>();

        moveFsm.AddState(EntityStateId.Move, new MoveState(this));
        moveFsm.AddState(EntityStateId.Dodge, new DodgeState(this));

        moveFsm.SetStartState(EntityStateId.Move);

        moveFsm.AddTriggerTransition(EntityEvent.Dodge, new MoveDodgeTransition(this));
        moveFsm.AddTriggerTransition(EntityEvent.DodgeFinished, new DodgeMoveTransition(this));

        return moveFsm;
    }

    // =========================================================
    // Player Transition Registration
    // =========================================================

    protected override void RegisterTransitions()
    {
        base.RegisterTransitions();

        RegisterDodgeTransitions();
    }

    private void RegisterDodgeTransitions()
    {
        // =====================================================
        // Dodge Request
        // =====================================================
        Fsm.AddTriggerTransition(EntityEvent.Dodge, new IdleDodgeTransition(this));
        Fsm.AddTriggerTransition(EntityEvent.DodgeFinished, new DodgeIdleTransition(this));
    }

    // =========================================================
    // Dodge Event
    // =========================================================

    public void RequestDodge()
    {
        if (CurrentState == EntityStateId.Idle)
        {
            moveFsm.SetStartState(EntityStateId.Dodge);
            Fsm.Trigger(EntityEvent.Dodge);
            moveFsm.SetStartState(EntityStateId.Move);
            return;
        }

        Fsm.Trigger(EntityEvent.Dodge);
    }

    public void NotifyDodgeFinished()
    {
        Fsm.Trigger(EntityEvent.DodgeFinished);
    }
}
