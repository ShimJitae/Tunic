using System;
using UnityEngine;

public class EnemyAttackModule : MonoBehaviour, IAttackStrategy
{
    [SerializeField] private Weapon weapon;
    public Weapon Weapon { get => weapon; set => weapon = value; }
    public event Action OnAttack;

    private void Awake()
    {
        weapon = GetComponentInChildren<Weapon>();
    }

    void Start()
    {
        ActiveAttackZone(false);
    }

    public void SetUpData(EnemyData enemyData)
    {
        weapon.Damage = enemyData.AttackDamage;
    }

    public void ActiveAttackZone(bool enable)
    {
        if (Weapon == null)
        {
            Debug.LogError($"EnemyAttackModule : {gameObject.name}의 Weapon이 비어있습니다.");
            return;
        }
        if (Weapon.AttackZone == null)
        {
            Debug.LogError($"EnemyAttackModule : {gameObject.name}의 Weapon.AttackZone이 비어있습니다.");
            return;
        }
        Weapon.AttackZone.enabled = enable;
    }

    public void Attack()
    {
        OnAttack?.Invoke();
    }
}
