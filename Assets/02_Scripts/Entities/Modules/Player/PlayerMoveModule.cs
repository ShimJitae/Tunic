using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(CharacterController))]
public class PlayerMoveModule : MonoBehaviour, IMoveStrategy
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 15f;
    [Header("Dodge")]
    [SerializeField] private float dodgeDist = 4f;
    [SerializeField] private float dodgeDuration = 0.25f;
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

    public void Dodge()
    {
        Vector3 inputDirection =
            InputManager.Instance != null
                ? InputManager.Instance.MoveInput
                : MoveInfo;

        inputDirection.y = 0f;

        Vector3 moveDirection;

        if (inputDirection.sqrMagnitude > 0.0001f)
        {
            if (!TryGetCameraBasis(
                    out Vector3 cameraForward,
                    out Vector3 cameraRight
                ))
            {
                return;
            }

            // 현재 플레이어 방향과 관계없이 카메라 기준 입력 방향 사용
            moveDirection =
                cameraForward * inputDirection.z +
                cameraRight * inputDirection.x;
        }
        else
        {
            // 방향 입력이 없을 때만 현재 정면으로 회피
            moveDirection = transform.forward;
        }

        moveDirection.y = 0f;

        if (moveDirection.sqrMagnitude <= 0.0001f)
            return;

        moveDirection.Normalize();

        // 회피 방향을 즉시 바라봄
        transform.rotation = Quaternion.LookRotation(
            moveDirection,
            Vector3.up
        );

        float movedDistance = 0f;

        DOVirtual.Float(0f, dodgeDist, dodgeDuration, distance =>
        {
            float deltaDistance = distance - movedDistance;
            movedDistance = distance;

            characterController.Move(
                moveDirection * deltaDistance
            );
        })
        .SetEase(Ease.OutCubic);
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
