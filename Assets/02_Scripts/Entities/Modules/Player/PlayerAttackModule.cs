using System;
using UnityEngine;

public class PlayerAttackModule : MonoBehaviour, IAttackStrategy
{
    [SerializeField] private Weapon weapon;

    public event Action OnAttack;

    public Weapon Weapon { get => weapon; set => weapon = value; }

    private void Awake()
    {
        weapon = GetComponentInChildren<Weapon>();
    }

    void Start()
    {
        ActiveAttackZone(false);
    }

    public void SetUpData(PlayerData playerData)
    {
        weapon.Damage = playerData.AttackDamage;
    }

    public void ActiveAttackZone(bool enable)
    {
        if (Weapon == null)
        {
            Debug.LogError($"PlayerAttackModule : {gameObject.name}의 Weapon이 비어있습니다.");
            return;
        }
        if (Weapon.AttackZone == null)
        {
            Debug.LogError($"PlayerAttackModule : {gameObject.name}의 Weapon.AttackZone이 비어있습니다.");
            return;
        }
        Weapon.AttackZone.enabled = enable;
    }

    public void Attack()
    {
        OnAttack?.Invoke();
    }
}
