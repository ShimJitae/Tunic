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
            Debug.LogError($"{nameof(EnemyController)} requires a {nameof(EnemyMoveModule)} component.", this);
            enabled = false;
        }

        if (!TryGetComponent(out attackModule))
        {
            Debug.LogError($"{nameof(EnemyController)} requires a {nameof(EnemyAttackModule)} component.", this);
            enabled = false;
        }

        if (!TryGetComponent(out brain))
        {
            Debug.LogError($"{nameof(EnemyController)} requires a {nameof(EnemyBrain)} component.", this);
            enabled = false;
        }

        animationModule = GetComponentInChildren<EnemyAnimationModule>();
        if (animationModule == null)
        {
            Debug.LogError($"{nameof(EnemyController)} requires a {nameof(EnemyAnimationModule)} component.", this);
            enabled = false;
        }

        if (dataSetUp == null && !TryGetComponent(out dataSetUp))
        {
            Debug.LogError($"{nameof(EnemyController)} requires a {nameof(DataSetUp_Enemy)} component.", this);
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
        {
            Health.OnDamaged += HandleDamaged;
        }
    }

    protected override void OnDisable()
    {
        if (Health != null)
        {
            Health.OnDamaged -= HandleDamaged;
        }

        if (moveModule != null)
            moveModule.Stop();

        if (attackModule != null)
            attackModule.SetAttackZoneActive(false);

        base.OnDisable();
    }

    protected override StateBase<EntityLifeStateId> CreateAliveState()
    {
        aliveFsm = new StateMachine<EntityLifeStateId, EnemyAliveStateId, EnemyStateEvent>();

        aliveFsm.StateChanged += _ => OnAliveStateEntered?.Invoke(aliveFsm.ActiveStateName);

        aliveFsm.AddState(EnemyAliveStateId.Idle, new EnemyIdleState(moveModule, animationModule, brain));
        aliveFsm.AddState(EnemyAliveStateId.Patrol, new EnemyPatrolState(moveModule, animationModule, brain));
        aliveFsm.AddState(EnemyAliveStateId.Chase, new EnemyChaseState(moveModule, animationModule, brain));
        aliveFsm.AddState(EnemyAliveStateId.Attack, new EnemyAttackState(transform, moveModule, animationModule, attackModule, brain));
        aliveFsm.AddState(EnemyAliveStateId.Hit, new EnemyHitState(moveModule, animationModule, attackModule));

        aliveFsm.SetStartState(EnemyAliveStateId.Idle);

        RegisterNormalTransitions();

        // Any -> Hit, 피격 시 즉시
        aliveFsm.AddTriggerTransitionFromAny(EnemyStateEvent.Damaged, EnemyAliveStateId.Hit, forceInstantly: true);

        return aliveFsm;
    }

    protected override StateBase<EntityLifeStateId> CreateDeadState()
    {
        return new EnemyDeadState(moveModule, animationModule, attackModule);
    }

    private void RegisterNormalTransitions()
    {
        // Idle -> Chase, 대상을 감지했을 시
        aliveFsm.AddTransition(EnemyAliveStateId.Idle, EnemyAliveStateId.Chase, _ => brain.CanDetectTarget());

        // Idle -> Patrol, 순찰 지점이 있고 대기 시간이 끝났을 시
        aliveFsm.AddTransition(EnemyAliveStateId.Idle, EnemyAliveStateId.Patrol, _ => brain.HasPatrolPoint && brain.IsIdleWaitComplete());

        // Patrol -> Chase, 대상을 감지했을 시
        aliveFsm.AddTransition(EnemyAliveStateId.Patrol, EnemyAliveStateId.Chase, _ => brain.CanDetectTarget());

        // Patrol -> Idle, 순찰 목적지에 도착했을 시
        aliveFsm.AddTransition(EnemyAliveStateId.Patrol, EnemyAliveStateId.Idle, _ => moveModule.HasReachedDestination());

        // Chase -> Idle, 추적 대상을 포기해야 할 시
        aliveFsm.AddTransition(EnemyAliveStateId.Chase, EnemyAliveStateId.Idle, _ => brain.ShouldGiveUpTarget());

        // Chase -> Attack, 공격 가능한 상태일 시
        aliveFsm.AddTransition(EnemyAliveStateId.Chase, EnemyAliveStateId.Attack, _ => brain.CanAttack());

        // Attack -> Chase, 공격 애니메이션이 종료됐을 시
        aliveFsm.AddTransition(EnemyAliveStateId.Attack, EnemyAliveStateId.Chase, _ => animationModule.IsAnimationComplete(animationModule.Attack));

        // Hit -> Chase, 피격 애니메이션이 종료됐을 시
        aliveFsm.AddTransition(EnemyAliveStateId.Hit, EnemyAliveStateId.Chase, _ => animationModule.IsAnimationComplete(animationModule.Hit));
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

    protected override void HandleDied()
    {
        base.HandleDied();

        if (gameObject.TryGetComponent(out Rigidbody rigidbody))
        {
            rigidbody.linearVelocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
            rigidbody.detectCollisions = false;
        }
    }
}
