using Unity.Behavior;
using UnityEngine;

public class DataSetUp_Boss : MonoBehaviour
{
    [SerializeField] private BossData bossData;
    [SerializeField] private BehaviorGraphAgent behaviorAgent;
    private Health bossHealth;
    private BossAttackModule bossAttackModule;

    private void Awake()
    {
        if (behaviorAgent == null && !gameObject.TryGetComponent(out behaviorAgent))
        {
            Debug.LogError($"DataSetUp_Boss : {gameObject.name}에 BehaviorGraphAgent가 없습니다.");
        }

        if (bossData == null)
        {
            Debug.LogError($"DataSetUp_Boss : {gameObject.name}에 BossData가 연결되지 않았습니다.");
        }

        if (!gameObject.TryGetComponent(out bossHealth))
        {
            Debug.LogError($"DataSetUp_Boss : {gameObject.name}에 Health가 없습니다.");
        }

        if (!gameObject.TryGetComponent(out bossAttackModule))
        {
            Debug.LogError($"DataSetUp_Boss : {gameObject.name}에 BossAttackModule가 없습니다.");
        }
    }

    private void Start()
    {
        bossHealth.SetUpData(bossData);
        bossAttackModule.SetUpData(bossData);
        ApplyConfig();
    }

    public void ApplyConfig()
    {
        behaviorAgent.SetVariableValue("MoveSpeed", bossData.MoveSpeed);
        behaviorAgent.SetVariableValue("MeleeAttackRange", bossData.MeleeAttackRange);
        behaviorAgent.SetVariableValue("AttackCooldown", bossData.AttackCooldown);
        behaviorAgent.SetVariableValue("ChaseDistance", bossData.ChaseDistance);
        behaviorAgent.SetVariableValue("ChargeDistance", bossData.ChargeDistance);
        behaviorAgent.SetVariableValue("ChargeDuration", bossData.ChargeDuration);
    }
}
