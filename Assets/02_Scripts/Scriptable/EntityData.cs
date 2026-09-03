using UnityEngine;

[CreateAssetMenu(fileName = "EntityData", menuName = "Scriptable Objects/EntityData")]
public class EntityData : ScriptableObject
{
    [SerializeField] protected float maxHP = 100f;
    public float MaxHP => maxHP;
    [SerializeField] protected float moveSpeed = 5f;
    public float MoveSpeed => moveSpeed;
}
