using UnityEngine;

public class AttackAnimationEventRelay : MonoBehaviour
{
    private IAttackZoneController attackZoneController;

    protected IAttackZoneController AttackZoneController => attackZoneController;

    protected virtual void Awake()
    {
        attackZoneController = GetComponentInParent<IAttackZoneController>();
        if (attackZoneController == null)
        {
            Debug.LogError(
                $"{nameof(AttackAnimationEventRelay)} : " +
                $"{gameObject.name}의 부모 오브젝트에서 공격 판정 모듈을 찾지 못했습니다.",
                this);
        }
    }

    public void OpenAttackZone()
    {
        attackZoneController?.SetAttackZoneActive(true);
    }

    public void CloseAttackZone()
    {
        attackZoneController?.SetAttackZoneActive(false);
    }
}
