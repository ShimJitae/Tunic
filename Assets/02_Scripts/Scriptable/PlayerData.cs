using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Scriptable Objects/PlayerData")]
public class PlayerData : EntityData
{
    [Header("Dodge")]
    [SerializeField] private float dodgeDist = 4f;
    [SerializeField] private float dodgeDuration = 0.25f;
    [Header("Gravity")]
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float groundedGravity = -2f;
}
