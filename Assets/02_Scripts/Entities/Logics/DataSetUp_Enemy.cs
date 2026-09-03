using UnityEngine;

public class DataSetUp_Enemy : MonoBehaviour, IDataSetUp
{
    [SerializeField] private EnemyData enemyData;
    EnemyMoveModule enemyMoveModule;
    EnemyAttackModule enemyAttackModule;
    Health enemyHealth;

    void Awake()
    {
        if (!gameObject.TryGetComponent(out enemyMoveModule))
        {
            Debug.LogError($"DataSetUp_Enemy : {gameObject.name}에 EnemyMoveModule이 없습니다.");
        }
        if (!gameObject.TryGetComponent(out enemyAttackModule))
        {
            Debug.LogError($"DataSetUp_Enemy : {gameObject.name}에 EnemyAttackModule이 없습니다.");
        }
        if (!gameObject.TryGetComponent(out enemyHealth))
        {
            Debug.LogError($"DataSetUp_Enemy : {gameObject.name}에 Health가 없습니다.");
        }
    }

    public void SetUpData()
    {
        enemyMoveModule.SetUpData(enemyData);
        enemyAttackModule.SetUpData(enemyData);
        enemyHealth.SetUpData(enemyData);
    }
}
