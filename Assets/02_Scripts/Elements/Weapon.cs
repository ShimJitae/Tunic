using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    private Collider attackZone;
    // 공격이 실행하는 순간에만 AttackZone을 활성화 시킨다.
    public Collider AttackZone => attackZone;
    private List<LayerMask> targetLayers = new();
    [SerializeField] float damage = 10f;

    void Awake()
    {
        attackZone = GetComponentInChildren<Collider>();
        if (attackZone == null)
        {
            Debug.LogError($"Weapon : {gameObject.name}의 하위 오브젝트에 Collider가 존재하지 않습니다.");
        }

        EntityController entityController = transform.GetComponentInParent<EntityController>();
        if (entityController is PlayerController p_Controller)
        {
            LayerMask enemyLayer = LayerMask.NameToLayer("Enemy");
            if (!targetLayers.Contains(enemyLayer))
            {
                targetLayers.Add(enemyLayer);
            }
        }
        else if (entityController is EnemyController e_Controlle)
        {
            LayerMask playerLayer = LayerMask.NameToLayer("Player");
            if (!targetLayers.Contains(playerLayer))
            {
                targetLayers.Add(playerLayer);
            }
        }
        else
        {
            // player/enemy 컨트롤러가 없으면 따로 추가해주지는 않음
        }

        AttackZone.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        foreach (LayerMask targetLayer in targetLayers)
        {
            // 1. 공격 가능한 Layer인지 확인
            if ((targetLayer.value & (1 << other.gameObject.layer)) == 0)
                return;

            // 3. 데미지를 받을 수 있는 대상인지 확인
            if (!other.TryGetComponent<IDamageable>(out var damageable))
                return;

            damageable.TakeDamage(damage);
        }
    }
}
