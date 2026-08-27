using UnityEngine;

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

    [SerializeField] private Transform cameraTransform;

    private CharacterController characterController;

    private float currentSpeed;
    private float rotationVelocity;
    private float verticalVelocity;
    private int lastMoveFrame = -1;
    private bool hasLoggedMissingCamera;

    public Vector3 MoveInfo { get; set; }

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        TryResolveCamera();
    }

    private void LateUpdate()
    {
        // 상태 머신이 Move()를 호출하지 않는 Idle/Attack/Hit/Dead 상태에서도
        // 중력과 CharacterController의 접지 상태를 매 프레임 갱신한다.
        if (lastMoveFrame == Time.frameCount)
            return;

        currentSpeed = 0f;
        ApplyMotion(Vector3.zero);
    }

    public void Move()
    {
        if (lastMoveFrame == Time.frameCount)
            return;

        lastMoveFrame = Time.frameCount;

        Vector3 inputDirection = new Vector3(MoveInfo.x, 0f, MoveInfo.z);
        float inputMagnitude = Mathf.Clamp01(inputDirection.magnitude);

        if (inputMagnitude <= 0.001f ||
            !TryGetCameraBasis(out Vector3 cameraForward, out Vector3 cameraRight))
        {
            currentSpeed = 0f;
            ApplyMotion(Vector3.zero);
            return;
        }

        Vector3 moveDirection =
            cameraForward * inputDirection.z +
            cameraRight * inputDirection.x;

        if (moveDirection.sqrMagnitude <= 0.0001f)
        {
            currentSpeed = 0f;
            ApplyMotion(Vector3.zero);
            return;
        }

        moveDirection.Normalize();

        float targetSpeed = moveSpeed * inputMagnitude;
        currentSpeed = Mathf.MoveTowards(
            currentSpeed,
            targetSpeed,
            acceleration * Time.deltaTime
        );

        RotateTowards(moveDirection);
        ApplyMotion(moveDirection * currentSpeed);
    }

    private bool TryResolveCamera()
    {
        if (cameraTransform != null)
        {
            hasLoggedMissingCamera = false;
            return true;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            cameraTransform = mainCamera.transform;
            hasLoggedMissingCamera = false;
            return true;
        }

        if (!hasLoggedMissingCamera)
        {
            Debug.LogError(
                "PlayerMoveModule could not find a camera tagged MainCamera.",
                this
            );
            hasLoggedMissingCamera = true;
        }

        return false;
    }

    private bool TryGetCameraBasis(out Vector3 cameraForward, out Vector3 cameraRight)
    {
        cameraForward = Vector3.zero;
        cameraRight = Vector3.zero;

        if (!TryResolveCamera())
            return false;

        cameraForward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up);

        // 완전한 탑다운 카메라는 forward의 수평 투영값이 0이므로
        // 화면의 위쪽을 나타내는 camera up을 대체 방향으로 사용한다.
        if (cameraForward.sqrMagnitude <= 0.0001f)
            cameraForward = Vector3.ProjectOnPlane(cameraTransform.up, Vector3.up);

        if (cameraForward.sqrMagnitude <= 0.0001f)
            return false;

        cameraForward.Normalize();
        cameraRight = Vector3.Cross(Vector3.up, cameraForward).normalized;
        return true;
    }

    private void RotateTowards(Vector3 moveDirection)
    {
        float targetAngle = Mathf.Atan2(
            moveDirection.x,
            moveDirection.z
        ) * Mathf.Rad2Deg;

        float smoothAngle = Mathf.SmoothDampAngle(
            transform.eulerAngles.y,
            targetAngle,
            ref rotationVelocity,
            rotationSmoothTime
        );

        transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
    }

    private void ApplyMotion(Vector3 horizontalVelocity)
    {
        if (characterController.isGrounded && verticalVelocity < 0f)
            verticalVelocity = groundedGravity;
        else
            verticalVelocity += gravity * Time.deltaTime;

        horizontalVelocity.y = verticalVelocity;
        characterController.Move(horizontalVelocity * Time.deltaTime);
    }
}
