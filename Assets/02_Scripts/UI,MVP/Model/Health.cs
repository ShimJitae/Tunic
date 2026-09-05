using UnityEngine;
using System;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] protected float maxHP = 100f;
    public float MaxHP => maxHP;
    protected float currHP;
    public float CurrHP => currHP;

    public bool IsDied { get; protected set; }
    public bool IsInvincible { get; set; }

    public event Action<float> OnDamaged;
    public event Action<float> OnRestored;
    public event Action OnDied;

    protected virtual void Awake()
    {
        SetHealthData();
    }

    private void SetHealthData()
    {
        currHP = maxHP;
    }

    public void SetUpData(EntityData entityData)
    {
        maxHP = entityData.MaxHP;
        SetHealthData();
    }

    public void TakeDamage(float value)
    {
        if (IsDied || IsInvincible)
            return;

        float damage = -value;

        currHP = Mathf.Max(0f, currHP + damage);

        bool diedFromDamage = currHP <= 0f;
        if (diedFromDamage)
            IsDied = true;

        OnDamaged?.Invoke(damage);

        if (diedFromDamage)
            OnDied?.Invoke();
    }

    public void RestoreHp(float value)
    {
        if (IsDied)
            return;
        else if (value <= 0f)
        {
            Debug.LogError("해당 Health의 RestoreHp에 value가 음수가 들어옴.");
            return;
        }

        currHP = Mathf.Min(maxHP, currHP + value);

        OnRestored?.Invoke(value);
    }
}
