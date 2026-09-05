using System;
using UnityEngine;

public class EnemyAttackModule : MonoBehaviour, IAttackZoneController
{
    [SerializeField] private Weapon weapon;
    public event Action<bool> OnAttackZoneChanged;

    private void Awake()
    {
        weapon = GetComponentInChildren<Weapon>();
    }

    private void Start()
    {
        SetAttackZoneActive(false);
    }

    public void SetUpData(EnemyData enemyData)
    {
        weapon.Damage = enemyData.AttackDamage;
    }

    public void SetAttackZoneActive(bool isActive)
    {
        if (weapon == null)
        {
            Debug.LogError($"EnemyAttackModule : {gameObject.name}의 Weapon이 비어있습니다.");
            return;
        }
        if (weapon.AttackZone == null)
        {
            Debug.LogError($"EnemyAttackModule : {gameObject.name}의 Weapon.AttackZone이 비어있습니다.");
            return;
        }
        if (weapon.AttackZone.enabled == isActive)
            return;

        weapon.AttackZone.enabled = isActive;
        OnAttackZoneChanged?.Invoke(isActive);
    }
}
