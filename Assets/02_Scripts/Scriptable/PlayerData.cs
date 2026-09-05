using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Scriptable Objects/PlayerData")]
public class PlayerData : EntityData
{
    [Header("Stamina")]
    [SerializeField] private float maxStamina = 50f;
    public float MaxStamina => maxStamina;
    [SerializeField] private float staminaRecoveryDelay = 1.5f;
    public float StaminaRecoveryDelay => staminaRecoveryDelay;
    [SerializeField] private float staminaRecoveryPerSecond = 15f;
    public float StaminaRecoveryPerSecond => staminaRecoveryPerSecond;

    [Header("Action Costs")]
    [SerializeField, Min(0f)] private float attackStaminaCost = 10f;
    public float AttackStaminaCost => attackStaminaCost;
    [SerializeField, Min(0f)] private float dodgeStaminaCost = 5f;
    public float DodgeStaminaCost => dodgeStaminaCost;

    [Header("Dodge")]
    [SerializeField] private float dodgeDist = 4f;
    public float DodgeDist => dodgeDist;
    [SerializeField] private float dodgeDuration = 0.25f;
    public float DodgeDuration => dodgeDuration;
    [Header("Gravity")]
    [SerializeField] private float gravity = -20f;
    public float Gravity => gravity;
    [SerializeField] private float groundedGravity = -2f;
    public float GroundedGravity => groundedGravity;
}
