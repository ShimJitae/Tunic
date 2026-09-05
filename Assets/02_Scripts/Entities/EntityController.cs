using System;
using UnityEngine;
using UnityHFSM;

[RequireComponent(typeof(Health))]
public abstract class EntityController : MonoBehaviour
{
    [SerializeField] private Health health;

    private StateMachine<EntityLifeStateId, EntityLifeEvent> lifeFsm;
    private bool hasStarted;

    public event Action<EntityLifeStateId> OnLifeStateEntered;

    public Health Health => health;

    public bool IsAlive => lifeFsm != null
        && lifeFsm.IsInitialized
        && lifeFsm.ActiveStateName == EntityLifeStateId.Alive
        && !health.IsDied;

    public bool IsDead => health != null && health.IsDied;

    public EntityLifeStateId CurrentLifeState => lifeFsm.ActiveStateName;

    protected virtual void Awake()
    {
        if (!TryGetComponent(out health))
        {
            Debug.LogError(
                $"{nameof(EntityController)} requires a {nameof(Health)} component.",
                this);

            enabled = false;
        }
    }

    protected virtual void Start()
    {
        if (!enabled)
            return;

        CreateLifeStateMachine();
        hasStarted = true;
    }

    protected virtual void OnEnable()
    {
        if (health != null)
            health.OnDied += HandleDied;

        if (hasStarted && lifeFsm != null && !lifeFsm.IsInitialized)
            EnterInitialLifeState();
    }

    protected virtual void OnDisable()
    {
        if (health != null)
            health.OnDied -= HandleDied;

        if (lifeFsm != null && lifeFsm.IsInitialized)
            lifeFsm.OnExit();
    }

    protected virtual void Update()
    {
        if (lifeFsm != null && lifeFsm.IsInitialized)
            lifeFsm.OnLogic();
    }

    protected abstract StateBase<EntityLifeStateId> CreateAliveState();

    protected abstract StateBase<EntityLifeStateId> CreateDeadState();

    private void CreateLifeStateMachine()
    {
        lifeFsm = new StateMachine<EntityLifeStateId, EntityLifeEvent>();

        lifeFsm.StateChanged += _ =>
            OnLifeStateEntered?.Invoke(lifeFsm.ActiveStateName);

        lifeFsm.AddState(EntityLifeStateId.Alive, CreateAliveState());
        lifeFsm.AddState(EntityLifeStateId.Dead, CreateDeadState());

        lifeFsm.AddTriggerTransition(
            EntityLifeEvent.Died,
            EntityLifeStateId.Alive,
            EntityLifeStateId.Dead,
            forceInstantly: true);

        EnterInitialLifeState();
    }

    private void EnterInitialLifeState()
    {
        lifeFsm.SetStartState(
            health.IsDied
                ? EntityLifeStateId.Dead
                : EntityLifeStateId.Alive);

        lifeFsm.OnEnter();
    }

    private void HandleDied()
    {
        if (lifeFsm == null || !lifeFsm.IsInitialized)
            return;

        lifeFsm.Trigger(EntityLifeEvent.Died);
    }
}
