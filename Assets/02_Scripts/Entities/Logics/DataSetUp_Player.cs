using UnityEngine;

public class DataSetUp_Player : MonoBehaviour, IDataSetUp
{
    [SerializeField] private PlayerData playerData;
    private PlayerMoveModule playerMoveModule;
    private PlayerAttackModule playerAttackModule;
    private Status playerStatus;

    void Awake()
    {
        if (!gameObject.TryGetComponent(out playerMoveModule))
        {
            Debug.LogError($"DataSetUp_Player : {gameObject.name}에 PlayerMoveModule이 없습니다.");
        }
        if (!gameObject.TryGetComponent(out playerAttackModule))
        {
            Debug.LogError($"DataSetUp_Player : {gameObject.name}에 playerAttackModule이 없습니다.");
        }
        if (!gameObject.TryGetComponent(out playerStatus))
        {
            Debug.LogError($"DataSetUp_Player : {gameObject.name}에 playerStatus이 없습니다.");
        }
    }

    public void SetUpData()
    {
        playerMoveModule.SetUpData(playerData);
        playerAttackModule.SetUpData(playerData);
        playerStatus.SetUpData(playerData);
    }
}
