using UnityEngine;
using UnityHFSM;

[RequireComponent(typeof(Rigidbody))]
public class EnemyController : EntityController
{
    private EnemyMoveModule enemyMoveModule;

    protected override void Awake()
    {
        if (!TryGetComponent(out enemyMoveModule))
        {
            Debug.LogError($"{nameof(EnemyController)} requires a {nameof(EnemyMoveModule)} component.", this);
            enabled = false;
            return;
        }

        MoveModule = enemyMoveModule;

        AnimationModule = GetComponentInChildren<EnemyAnimationModule>();
        if (AnimationModule == null)
        {
            Debug.LogError($"{nameof(EnemyController)} requires a {nameof(EnemyAnimationModule)} component.", this);
            enabled = false;
            return;
        }

        base.Awake();
    }

    protected override void RegisterTransitions()
    {
        base.RegisterTransitions();

        RegisterChaseTransitions();
    }

    private void RegisterChaseTransitions()
    {
    }
}
