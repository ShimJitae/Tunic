using UnityEngine;
using System;

public class Status : Health
{
    [SerializeField] protected float maxStamina = 50f;
    public float MaxStamina => maxStamina;
    protected float currStamina;
    public float CurrStamina => currStamina;

    [SerializeField, Min(0f)]
    private float staminaRecoveryDelay = 1.5f;

    [SerializeField, Min(0f)]
    private float staminaRecoveryPerSecond = 15f;

    private float staminaRecoveryStartTime;

    public event Action OnStaminaChanged;

    protected override void Awake()
    {
        base.Awake();
    }

    public void SetUpData(PlayerData playerData)
    {
        base.SetUpData(playerData);

        maxStamina = playerData.MaxStamina;
        staminaRecoveryDelay = playerData.StaminaRecoveryDelay;
        staminaRecoveryPerSecond = playerData.StaminaRecoveryPerSecond;

        currStamina = maxStamina;
    }

    public bool TakeStamina(float value)
    {
        if (!HasStamina(value))
            return false;

        if (value == 0f)
            return true;

        currStamina -= value;

        staminaRecoveryStartTime = Time.time + staminaRecoveryDelay;

        OnStaminaChanged?.Invoke();
        return true;
    }

    public bool HasStamina(float value)
    {
        return !IsDied && value >= 0f && currStamina >= value;
    }

    private void Update()
    {
        if (IsDied || currStamina >= maxStamina)
            return;

        if (Time.time < staminaRecoveryStartTime)
            return;

        float nextStamina = Mathf.MoveTowards(
            currStamina,
            maxStamina,
            staminaRecoveryPerSecond * Time.deltaTime
        );

        if (nextStamina == currStamina)
            return;

        currStamina = nextStamina;
        OnStaminaChanged?.Invoke();
    }
}
