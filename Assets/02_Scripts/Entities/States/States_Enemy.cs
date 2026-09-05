using UnityEngine;
using UnityHFSM;

public sealed class EnemyIdleState : StateBase<EnemyAliveStateId>
{
    private readonly EnemyMoveModule moveModule;
    private readonly EnemyAnimationModule animationModule;
    private readonly EnemyBrain brain;

    public EnemyIdleState(
        EnemyMoveModule moveModule,
        EnemyAnimationModule animationModule,
        EnemyBrain brain)
        : base(needsExitTime: false)
    {
        this.moveModule = moveModule;
        this.animationModule = animationModule;
        this.brain = brain;
    }

    public override void OnEnter()
    {
        moveModule.Stop();
        brain.BeginIdle();
        animationModule.PlayIdle();
    }
}

public sealed class EnemyPatrolState : StateBase<EnemyAliveStateId>
{
    private readonly EnemyMoveModule moveModule;
    private readonly EnemyAnimationModule animationModule;
    private readonly EnemyBrain brain;

    public EnemyPatrolState(
        EnemyMoveModule moveModule,
        EnemyAnimationModule animationModule,
        EnemyBrain brain)
        : base(needsExitTime: false)
    {
        this.moveModule = moveModule;
        this.animationModule = animationModule;
        this.brain = brain;
    }

    public override void OnEnter()
    {
        if (brain.TryGetNextPatrolDestination(out Vector3 destination))
            moveModule.MoveTo(destination);
        else
            moveModule.Stop();

        animationModule.PlayMove();
    }

    public override void OnExit()
    {
        moveModule.Stop();
    }
}

public sealed class EnemyChaseState : StateBase<EnemyAliveStateId>
{
    private readonly EnemyMoveModule moveModule;
    private readonly EnemyAnimationModule animationModule;
    private readonly EnemyBrain brain;

    public EnemyChaseState(
        EnemyMoveModule moveModule,
        EnemyAnimationModule animationModule,
        EnemyBrain brain)
        : base(needsExitTime: false)
    {
        this.moveModule = moveModule;
        this.animationModule = animationModule;
        this.brain = brain;
    }

    public override void OnEnter()
    {
        moveModule.Stop();
        brain.BeginChase();
        animationModule.PlayMove();
        RefreshDestination();
    }

    public override void OnLogic()
    {
        RefreshDestination();
    }

    public override void OnExit()
    {
        moveModule.Stop();
    }

    private void RefreshDestination()
    {
        if (!brain.TryGetChaseDestination(out Vector3 destination))
            return;

        moveModule.MoveTo(destination);
    }
}

public sealed class EnemyAttackState : StateBase<EnemyAliveStateId>
{
    private readonly Transform owner;
    private readonly EnemyMoveModule moveModule;
    private readonly EnemyAnimationModule animationModule;
    private readonly EnemyAttackModule attackModule;
    private readonly EnemyBrain brain;

    public EnemyAttackState(
        Transform owner,
        EnemyMoveModule moveModule,
        EnemyAnimationModule animationModule,
        EnemyAttackModule attackModule,
        EnemyBrain brain)
        : base(needsExitTime: false)
    {
        this.owner = owner;
        this.moveModule = moveModule;
        this.animationModule = animationModule;
        this.attackModule = attackModule;
        this.brain = brain;
    }

    public override void OnEnter()
    {
        moveModule.Stop();
        attackModule.SetAttackZoneActive(false);
        FaceTarget();
        brain.MarkAttackStarted();
        animationModule.PlayAttack();
    }

    public override void OnExit()
    {
        attackModule.SetAttackZoneActive(false);
    }

    private void FaceTarget()
    {
        if (!brain.TryGetTargetPosition(out Vector3 targetPosition))
            return;

        Vector3 direction = targetPosition - owner.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= Mathf.Epsilon)
            return;

        owner.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
    }
}

public sealed class EnemyHitState : StateBase<EnemyAliveStateId>
{
    private readonly EnemyMoveModule moveModule;
    private readonly EnemyAnimationModule animationModule;
    private readonly EnemyAttackModule attackModule;

    public EnemyHitState(
        EnemyMoveModule moveModule,
        EnemyAnimationModule animationModule,
        EnemyAttackModule attackModule)
        : base(needsExitTime: false)
    {
        this.moveModule = moveModule;
        this.animationModule = animationModule;
        this.attackModule = attackModule;
    }

    public override void OnEnter()
    {
        moveModule.Stop();
        attackModule.SetAttackZoneActive(false);
        animationModule.PlayHit();
    }
}

public sealed class EnemyDeadState : StateBase<EntityLifeStateId>
{
    private readonly EnemyMoveModule moveModule;
    private readonly EnemyAnimationModule animationModule;
    private readonly EnemyAttackModule attackModule;

    public EnemyDeadState(
        EnemyMoveModule moveModule,
        EnemyAnimationModule animationModule,
        EnemyAttackModule attackModule)
        : base(needsExitTime: false)
    {
        this.moveModule = moveModule;
        this.animationModule = animationModule;
        this.attackModule = attackModule;
    }

    public override void OnEnter()
    {
        moveModule.Stop();
        attackModule.SetAttackZoneActive(false);
        animationModule.PlayDie();
    }
}
