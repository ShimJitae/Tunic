using UnityEngine;

public class EntityAnimClipRelay : MonoBehaviour
{
    protected EntityController entityController;

    private void Awake()
    {
        entityController = transform.GetComponentInParent<EntityController>();
        if (entityController == null)
        {
            Debug.LogError($"EntityAnimClipRelay : {gameObject.name}의 부모 오브젝트 중에 entityController가 없습니다.");
        }
    }

    public void OpenAttackZone()
    {
        if (entityController.AttackModule == null)
        {
            Debug.LogError($"EntityAnimClipRelay : {gameObject.name}의 attackModule이 비어있습니다.");
            return;
        }
        entityController.AttackModule.ActiveAttackZone(true);
    }

    public void CloseAttackZone()
    {
        if (entityController.AttackModule == null)
        {
            Debug.LogError($"EntityAnimClipRelay : {gameObject.name}의 attackModule이 비어있습니다.");
            return;
        }
        entityController.AttackModule.ActiveAttackZone(false);
    }

    public void AttackFinished()
    {
        entityController.NotifyAttackFinished();
    }

    public void HitFinished()
    {
        entityController.NotifyHitFinished();
    }
}
