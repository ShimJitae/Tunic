using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMoveModule : MonoBehaviour, IMoveStrategy
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 15f;

    [Header("Rotation")]
    [SerializeField] private float rotationSmoothTime = 0.1f;

    [Header("Gravity")]
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float groundedGravity = -2f;

    private CharacterController characterController;

    private float currentSpeed;
    private float rotationVelocity;
    private float verticalVelocity;

    public Vector3 MoveInfo { get; set; }

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    // 실제 Move는 PlayerController에서 상태 Update로 실행되고 있음.
    public void Move()
    {
        Vector3 moveDir = MoveInfo;
        moveDir.y = 0f;

        if (moveDir.sqrMagnitude > 0.001f)
        {
            float targetAngle = Mathf.Atan2(
                moveDir.x,
                moveDir.z
            ) * Mathf.Rad2Deg;

            float smoothAngle = Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                targetAngle,
                ref rotationVelocity,
                rotationSmoothTime
            );

            transform.rotation = Quaternion.Euler(
                0f,
                smoothAngle,
                0f
            );
        }

        if (characterController.isGrounded &&
            verticalVelocity < 0f)
        {
            verticalVelocity = groundedGravity;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        Vector3 velocity = moveDir.normalized * currentSpeed;

        velocity.y = verticalVelocity;

        characterController.Move(
            velocity * Time.deltaTime
        );
    }

    // private void HandleMovement()
    // {
    //     Vector2 input = moveAction.action.ReadValue<Vector2>();

    //     // -------------------------
    //     // 1. 카메라 기준 이동 방향 계산
    //     // -------------------------

    //     Vector3 cameraForward = cameraTransform.forward;
    //     Vector3 cameraRight = cameraTransform.right;

    //     // 카메라의 위/아래 기울기는 이동 방향에 영향을 주면 안 되므로
    //     // Y축을 제거해서 XZ 평면의 방향만 사용합니다.
    //     cameraForward.y = 0f;
    //     cameraRight.y = 0f;

    //     cameraForward.Normalize();
    //     cameraRight.Normalize();

    //     // W/S = 카메라 Forward
    //     // A/D = 카메라 Right
    //     Vector3 moveDirection =
    //         cameraForward * input.y +
    //         cameraRight * input.x;

    //     // 대각선 이동 속도가 더 빨라지는 것을 방지
    //     if (moveDirection.sqrMagnitude > 1f)
    //     {
    //         moveDirection.Normalize();
    //     }
    // }
}