using System;
using UnityEngine;
using UnityHFSM;

[RequireComponent(typeof(EnemyMoveModule))]
[RequireComponent(typeof(EnemyAttackModule))]
[RequireComponent(typeof(EnemyBrain))]
public class EnemyController : EntityController
{
    [SerializeField] private DataSetUp_Enemy dataSetUp;

    private EnemyMoveModule moveModule;
    private EnemyAnimationModule animationModule;
    private EnemyAttackModule attackModule;
    private EnemyBrain brain;

    private StateMachine<EntityLifeStateId, EnemyAliveStateId, EnemyStateEvent> aliveFsm;

    public event Action<EnemyAliveStateId> OnAliveStateEntered;

    public EnemyAliveStateId CurrentAliveState => aliveFsm.ActiveStateName;

    protected override void Awake()
    {
        base.Awake();

        if (!TryGetComponent(out moveModule))
        {
            Debug.LogError(
                $"{nameof(EnemyController)} requires a {nameof(EnemyMoveModule)} component.",
                this);
            enabled = false;
        }

        if (!TryGetComponent(out attackModule))
        {
            Debug.LogError(
                $"{nameof(EnemyController)} requires a {nameof(EnemyAttackModule)} component.",
                this);
            enabled = false;
        }

        if (!TryGetComponent(out brain))
        {
            Debug.LogError(
                $"{nameof(EnemyController)} requires a {nameof(EnemyBrain)} component.",
                this);
            enabled = false;
        }

        animationModule = GetComponentInChildren<EnemyAnimationModule>();
        if (animationModule == null)
        {
            Debug.LogError(
                $"{nameof(EnemyController)} requires a {nameof(EnemyAnimationModule)} component.",
                this);
            enabled = false;
        }

        if (dataSetUp == null && !TryGetComponent(out dataSetUp))
        {
            Debug.LogError(
                $"{nameof(EnemyController)} requires a {nameof(DataSetUp_Enemy)} component.",
                this);
            enabled = false;
        }
    }

    protected override void Start()
    {
        if (!enabled)
            return;

        dataSetUp.SetUpData();
        base.Start();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        if (Health != null)
            Health.OnDamaged += HandleDamaged;
    }

    protected override void OnDisable()
    {
        if (Health != null)
            Health.OnDamaged -= HandleDamaged;

        if (moveModule != null)
            moveModule.Stop();

        if (attackModule != null)
            attackModule.SetAttackZoneActive(false);

        base.OnDisable();
    }

    protected override StateBase<EntityLifeStateId> CreateAliveState()
    {
        aliveFsm = new StateMachine<
            EntityLifeStateId,
            EnemyAliveStateId,
            EnemyStateEvent>();

        aliveFsm.StateChanged += _ =>
            OnAliveStateEntered?.Invoke(aliveFsm.ActiveStateName);

        aliveFsm.AddState(
            EnemyAliveStateId.Idle,
            new EnemyIdleState(moveModule, animationModule, brain));
        aliveFsm.AddState(
            EnemyAliveStateId.Patrol,
            new EnemyPatrolState(moveModule, animationModule, brain));
        aliveFsm.AddState(
            EnemyAliveStateId.Chase,
            new EnemyChaseState(moveModule, animationModule, brain));
        aliveFsm.AddState(
            EnemyAliveStateId.Attack,
            new EnemyAttackState(
                transform,
                moveModule,
                animationModule,
                attackModule,
                brain));
        aliveFsm.AddState(
            EnemyAliveStateId.Hit,
            new EnemyHitState(moveModule, animationModule, attackModule));

        aliveFsm.SetStartState(EnemyAliveStateId.Idle);

        RegisterNormalTransitions();

        aliveFsm.AddTriggerTransitionFromAny(
            EnemyStateEvent.Damaged,
            EnemyAliveStateId.Hit,
            forceInstantly: true);

        return aliveFsm;
    }

    protected override StateBase<EntityLifeStateId> CreateDeadState()
    {
        return new EnemyDeadState(moveModule, animationModule, attackModule);
    }

    private void RegisterNormalTransitions()
    {
        aliveFsm.AddTransition(
            EnemyAliveStateId.Idle,
            EnemyAliveStateId.Chase,
            _ => brain.CanDetectTarget());
        aliveFsm.AddTransition(
            EnemyAliveStateId.Idle,
            EnemyAliveStateId.Patrol,
            _ => brain.HasPatrolPoint && brain.IsIdleWaitComplete());

        aliveFsm.AddTransition(
            EnemyAliveStateId.Patrol,
            EnemyAliveStateId.Chase,
            _ => brain.CanDetectTarget());
        aliveFsm.AddTransition(
            EnemyAliveStateId.Patrol,
            EnemyAliveStateId.Idle,
            _ => moveModule.HasReachedDestination());

        aliveFsm.AddTransition(
            EnemyAliveStateId.Chase,
            EnemyAliveStateId.Idle,
            _ => brain.ShouldGiveUpTarget());
        aliveFsm.AddTransition(
            EnemyAliveStateId.Chase,
            EnemyAliveStateId.Attack,
            _ => brain.CanAttack());

        aliveFsm.AddTransition(
            EnemyAliveStateId.Attack,
            EnemyAliveStateId.Chase,
            _ => animationModule.IsAnimationComplete(animationModule.Attack));
        aliveFsm.AddTransition(
            EnemyAliveStateId.Hit,
            EnemyAliveStateId.Chase,
            _ => animationModule.IsAnimationComplete(animationModule.Hit));
    }

    private void HandleDamaged(float _)
    {
        if (!IsAlive || Health.IsDied)
            return;

        if (aliveFsm.ActiveStateName == EnemyAliveStateId.Hit)
        {
            aliveFsm.RequestStateChange(EnemyAliveStateId.Hit, forceInstantly: true);
            return;
        }

        aliveFsm.Trigger(EnemyStateEvent.Damaged);
    }
}
