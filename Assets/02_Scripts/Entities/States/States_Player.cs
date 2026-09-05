using System;
using UnityEngine;
using UnityHFSM;

public sealed class PlayerIdleState : StateBase<PlayerLocomotionStateId>
{
    private readonly PlayerMoveModule moveModule;
    private readonly PlayerAnimationModule animationModule;

    public PlayerIdleState(
        PlayerMoveModule moveModule,
        PlayerAnimationModule animationModule)
        : base(needsExitTime: false)
    {
        this.moveModule = moveModule;
        this.animationModule = animationModule;
    }

    public override void OnEnter()
    {
        moveModule.Stop();
        animationModule.PlayIdle();
    }
}

public sealed class PlayerMoveState : StateBase<PlayerLocomotionStateId>
{
    private readonly PlayerMoveModule moveModule;
    private readonly PlayerAnimationModule animationModule;
    private readonly Func<Vector3> getMoveInput;

    public PlayerMoveState(
        PlayerMoveModule moveModule,
        PlayerAnimationModule animationModule,
        Func<Vector3> getMoveInput)
        : base(needsExitTime: false)
    {
        this.moveModule = moveModule;
        this.animationModule = animationModule;
        this.getMoveInput = getMoveInput;
    }

    public override void OnEnter()
    {
        animationModule.PlayMove();
    }

    public override void OnLogic()
    {
        moveModule.Move(getMoveInput());
    }

    public override void OnExit()
    {
        moveModule.Stop();
    }
}

public sealed class PlayerDodgeState : StateBase<PlayerLocomotionStateId>
{
    private readonly PlayerMoveModule moveModule;
    private readonly PlayerAnimationModule animationModule;
    private readonly Health health;
    private readonly Func<Vector3> getMoveInput;

    public PlayerDodgeState(
        PlayerMoveModule moveModule,
        PlayerAnimationModule animationModule,
        Health health,
        Func<Vector3> getMoveInput)
        : base(needsExitTime: false)
    {
        this.moveModule = moveModule;
        this.animationModule = animationModule;
        this.health = health;
        this.getMoveInput = getMoveInput;
    }

    public override void OnEnter()
    {
        animationModule.PlayDodge();
        health.IsInvincible = true;
        moveModule.StartDodge(getMoveInput());
    }

    public override void OnExit()
    {
        moveModule.CancelDodge();
        health.IsInvincible = false;
    }
}

public sealed class PlayerAttackState : StateBase<PlayerCombatStateId>
{
    private readonly PlayerCombatStateId attackState;
    private readonly PlayerMoveModule moveModule;
    private readonly PlayerAnimationModule animationModule;
    private readonly PlayerAttackModule attackModule;
    private readonly Func<Vector3> getMoveInput;

    public PlayerAttackState(
        PlayerCombatStateId attackState,
        PlayerMoveModule moveModule,
        PlayerAnimationModule animationModule,
        PlayerAttackModule attackModule,
        Func<Vector3> getMoveInput)
        : base(needsExitTime: false)
    {
        this.attackState = attackState;
        this.moveModule = moveModule;
        this.animationModule = animationModule;
        this.attackModule = attackModule;
        this.getMoveInput = getMoveInput;
    }

    public override void OnEnter()
    {
        attackModule.SetAttackZoneActive(false);
        moveModule.FaceInputDirection(getMoveInput());
        animationModule.PlayAttack(attackState);
    }

    public override void OnExit()
    {
        attackModule.SetAttackZoneActive(false);
    }
}

public sealed class PlayerHitState : StateBase<PlayerAliveStateId>
{
    private readonly PlayerAnimationModule animationModule;
    private readonly PlayerAttackModule attackModule;

    public PlayerHitState(
        PlayerAnimationModule animationModule,
        PlayerAttackModule attackModule)
        : base(needsExitTime: false)
    {
        this.animationModule = animationModule;
        this.attackModule = attackModule;
    }

    public override void OnEnter()
    {
        attackModule.SetAttackZoneActive(false);
        animationModule.PlayHit();
    }
}

public sealed class PlayerDeadState : StateBase<EntityLifeStateId>
{
    private readonly PlayerMoveModule moveModule;
    private readonly PlayerAnimationModule animationModule;
    private readonly PlayerAttackModule attackModule;
    private readonly Health health;

    public PlayerDeadState(
        PlayerMoveModule moveModule,
        PlayerAnimationModule animationModule,
        PlayerAttackModule attackModule,
        Health health)
        : base(needsExitTime: false)
    {
        this.moveModule = moveModule;
        this.animationModule = animationModule;
        this.attackModule = attackModule;
        this.health = health;
    }

    public override void OnEnter()
    {
        moveModule.Stop();
        moveModule.CancelDodge();
        attackModule.SetAttackZoneActive(false);
        health.IsInvincible = false;
        animationModule.PlayDie();
    }
}
