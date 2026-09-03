using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    private Collider attackZone;

    [SerializeField]
    private List<LayerMask> targetLayers = new();

    public Collider AttackZone => attackZone;

    public float Damage { get; set; } = 10f;

    private void Awake()
    {
        attackZone = GetComponentInChildren<Collider>();

        if (attackZone == null)
        {
            Debug.LogError(
                $"{nameof(Weapon)} : " +
                $"{gameObject.name}의 하위에서 Collider를 찾지 못했습니다.",
                this);

            enabled = false;
            return;
        }

        attackZone.enabled = false;

        if (!attackZone.isTrigger)
        {
            Debug.LogError(
                $"{nameof(Weapon)} : " +
                $"{attackZone.gameObject.name}의 Collider는 Is Trigger가 활성화되어야 합니다.",
                attackZone);

            enabled = false;
            return;
        }

        EntityController entityController =
            GetComponentInParent<EntityController>();

        if (entityController is PlayerController)
        {
            AddTargetLayer("Enemy");
        }
        else if (entityController is EnemyController)
        {
            AddTargetLayer("Player");
        }
        else
        {
        }

        if (targetLayers.Count == 0)
        {
            Debug.LogError(
                $"{nameof(Weapon)} : 공격 대상 레이어가 등록되어 있지 않습니다.",
                this);

            enabled = false;
        }
    }

    private void AddTargetLayer(string layerName)
    {
        LayerMask targetLayer = LayerMask.GetMask(layerName);

        // GetMask는 해당 레이어가 없으면 0을 반환한다.
        if (targetLayer.value == 0)
        {
            Debug.LogError(
                $"{nameof(Weapon)} : {layerName} 레이어가 존재하지 않습니다.",
                this);

            return;
        }

        if (!targetLayers.Contains(targetLayer))
        {
            targetLayers.Add(targetLayer);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        int otherLayerMask = 1 << other.gameObject.layer;

        foreach (LayerMask targetLayer in targetLayers)
        {
            // 현재 리스트 항목과 맞지 않으면 다음 항목 검사
            if ((targetLayer.value & otherLayerMask) == 0)
                continue;

            if (!other.TryGetComponent<IDamageable>(out var damageable))
                return;

            damageable.TakeDamage(Damage);
            return;
        }
    }
}
