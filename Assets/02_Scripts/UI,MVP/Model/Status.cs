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
        SetStaminaData();
    }

    private void SetStaminaData()
    {
        // 여기에 나중에 SO로 Stamina 초기화 하는 코드 추가하기.

        currStamina = maxStamina;
    }

    public bool TakeStamina(float value)
    {
        if (IsDied || value <= 0f || currStamina < value)
            return false;

        currStamina -= value;

        staminaRecoveryStartTime = Time.time + staminaRecoveryDelay;

        OnStaminaChanged?.Invoke();
        return true;
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
