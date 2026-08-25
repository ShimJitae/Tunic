using UnityEngine;

[RequireComponent(typeof(StateMachine))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private StateMachine stateMachine;

    private void Awake()
    {
        if (stateMachine == null)
        {
            stateMachine = GetComponent<StateMachine>();
        }
    }
}
