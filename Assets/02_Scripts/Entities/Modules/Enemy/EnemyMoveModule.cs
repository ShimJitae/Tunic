using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class EnemyMoveModule : MonoBehaviour, IMoveStrategy
{
    public Vector3 MoveInfo { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    public void Move()
    {
    }
}
