using UnityEngine;
using UnityHFSM;

[RequireComponent(typeof(PlayerMoveModule))]
public class PlayerController : EntityController
{
    private InputManager inputManager;
    private PlayerMoveModule playerMoveModule;

    protected override void Awake()
    {
        if (!TryGetComponent(out playerMoveModule))
        {
            Debug.LogError($"{nameof(PlayerController)} requires a {nameof(PlayerMoveModule)} component.", this);
            enabled = false;
            return;
        }

        MoveModule = playerMoveModule;

        base.Awake();
    }

    protected override void Update()
    {
        if (inputManager == null)
            inputManager = InputManager.Instance;

        MoveModule.MoveInfo = inputManager != null
            ? inputManager.MoveInput
            : Vector3.zero;

        base.Update();
    }

    private void OnDisable()
    {
        if (MoveModule != null)
            MoveModule.MoveInfo = Vector3.zero;
    }

    protected override void RegisterStates()
    {
        base.RegisterStates();

        Fsm.AddState(EntityStateId.Dodge, new DodgeState(this));
    }

    // =========================================================
    // Player Transition Registration
    // =========================================================

    protected override void RegisterTransitions()
    {
        base.RegisterTransitions();

        // RegisterDodgeTransitions();
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

    // =========================================================
    // Dodge Event
    // =========================================================

    public void NotifyDodgeFinished()
    {
        Fsm.Trigger(EntityEvent.DodgeFinished);
    }
}
