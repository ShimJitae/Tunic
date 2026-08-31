using UnityEngine;

public class EntityAnimClipRelay : MonoBehaviour
{
    private IAttackStrategy attackModule;

    private void Awake()
    {
        attackModule = transform.GetComponentInParent<IAttackStrategy>();
        if (attackModule == null)
        {
            Debug.LogError($"EntityAnimClipRelay : {gameObject.name}의 부모 오브젝트 중에 attackModule이 없습니다.");
        }
    }

    public void OpenAttackZone()
    {
        if (attackModule == null)
        {
            Debug.LogError($"EntityAnimClipRelay : {gameObject.name}의 attackModule이 비어있습니다.");
            return;
        }
        attackModule.ActiveAttackZone(true);
    }

    public void CloseAttackZone()
    {
        if (attackModule == null)
        {
            Debug.LogError($"EntityAnimClipRelay : {gameObject.name}의 attackModule이 비어있습니다.");
            return;
        }
        attackModule.ActiveAttackZone(false);
    }
}
