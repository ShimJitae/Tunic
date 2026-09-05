using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class InputManager : MonoBehaviour
{
    private static InputManager instance;
    public static InputManager Instance => instance;

    [SerializeField] private PlayerInput playerInput;

    public Vector3 MoveInput { get; private set; }

    public event Action AttackPressed;
    public event Action DodgePressed;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        if (!TryGetComponent(out playerInput))
        {
            Debug.LogError($"{nameof(InputManager)} requires a {nameof(PlayerInput)} component.", this);
            enabled = false;
        }
    }

    private void OnDisable()
    {
        MoveInput = Vector3.zero;
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    public void OnMoveInput(InputAction.CallbackContext context)
    {
        Vector2 input = context.canceled
            ? Vector2.zero
            : Vector2.ClampMagnitude(context.ReadValue<Vector2>(), 1f);

        MoveInput = new Vector3(input.x, 0f, input.y);
    }

    public void OnAttackInput(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        AttackPressed?.Invoke();
    }

    public void OnDodgeInput(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        DodgePressed?.Invoke();
    }
}
