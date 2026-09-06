using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class BossAttackModule : MonoBehaviour, IAttackZoneController
{
    [SerializeField]
    private Weapon weapon;

    [SerializeField]
    private float attackDamage = 10f;

    public event Action<bool> OnAttackZoneChanged;

    private void Awake()
    {
        // 인스펙터에서 직접 연결하지 않았을 경우
        // Boss의 자식 오브젝트에서 Weapon을 찾는다.
        if (weapon == null)
        {
            weapon = GetComponentInChildren<Weapon>(true);
        }

        if (weapon == null)
        {
            Debug.LogError($"{nameof(BossAttackModule)} : " + $"{gameObject.name}의 자식에서 Weapon을 찾지 못했습니다.", this);

            return;
        }

        weapon.Damage = attackDamage;
    }

    private void Start()
    {
        // 게임 시작 시 공격 콜라이더가 켜져 있지 않도록 한다.
        SetAttackZoneActive(false);
    }

    public void SetAttackZoneActive(bool isActive)
    {
        if (weapon == null)
        {
            Debug.LogError($"{nameof(BossAttackModule)} : Weapon이 연결되지 않았습니다.", this);

            return;
        }

        Collider attackZone = weapon.AttackZone;

        if (attackZone == null)
        {
            Debug.LogError($"{nameof(BossAttackModule)} : " + $"{weapon.gameObject.name}의 공격 콜라이더를 찾지 못했습니다.", weapon);

            return;
        }

        // 이미 원하는 상태라면 중복 처리하지 않는다.
        if (attackZone.enabled == isActive)
        {
            return;
        }

        attackZone.enabled = isActive;
        OnAttackZoneChanged?.Invoke(isActive);
    }

    private void OnDisable()
    {
        // 공격 중 Boss가 비활성화되더라도
        // 공격 콜라이더가 켜진 채 남지 않게 한다.
        if (weapon == null || weapon.AttackZone == null)
        {
            return;
        }

        if (!weapon.AttackZone.enabled)
        {
            return;
        }

        weapon.AttackZone.enabled = false;
        OnAttackZoneChanged?.Invoke(false);
    }
}