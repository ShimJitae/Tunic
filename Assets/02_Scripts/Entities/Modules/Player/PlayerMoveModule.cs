using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMoveModule : MonoBehaviour
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
    private Tween dodgeTween;

    private void Awake()
    {
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        if (cameraTransform == null)
        {
            Debug.LogError("PlayerMoveModule : Main 카메라를 찾지 못했습니다. cameraTransform에는 null 값이 할당되어 있습니다.");
        }

        characterController = GetComponent<CharacterController>();
    }

    private void OnDisable()
    {
        Stop();
        CancelDodge();
    }

    private void LateUpdate()
    {
        // 상태 머신이 Move()를 호출하지 않는 Idle/Attack/Hit/Dead 상태에서도
        // 중력과 CharacterController의 접지 상태를 매 프레임 갱신한다.
        ApplyGravity();
    }

    public void SetUpData(PlayerData playerData)
    {
        moveSpeed = playerData.MoveSpeed;
        dodgeDist = playerData.DodgeDist;
        dodgeDuration = playerData.DodgeDuration;
        gravity = playerData.Gravity;
        groundedGravity = playerData.GroundedGravity;
    }

    public void Move(Vector3 input)
    {
        Vector3 inputDirection = new Vector3(input.x, 0f, input.z);
        float inputMagnitude = Mathf.Clamp01(inputDirection.magnitude);

        if (inputMagnitude <= 0.001f ||
            !TryGetCameraBasis(out Vector3 cameraForward, out Vector3 cameraRight))
        {
            Stop();
            return;
        }

        Vector3 moveDirection = (cameraForward * inputDirection.z + cameraRight * inputDirection.x).normalized;

        float targetSpeed = moveSpeed * inputMagnitude;

        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.deltaTime);

        RotateTowards(moveDirection);
        characterController.Move(moveDirection * currentSpeed * Time.deltaTime);
    }

    public void Stop()
    {
        currentSpeed = 0f;
    }

    public void FaceInputDirection(Vector3 input)
    {
        Vector3 inputDirection = new Vector3(input.x, 0f, input.z);
        if (inputDirection.sqrMagnitude <= 0.0001f ||
            !TryGetCameraBasis(out Vector3 cameraForward, out Vector3 cameraRight))
        {
            return;
        }

        Vector3 lookDirection = cameraForward * inputDirection.z + cameraRight * inputDirection.x;
        lookDirection.y = 0f;

        if (lookDirection.sqrMagnitude <= 0.0001f)
            return;

        rotationVelocity = 0f;
        transform.rotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
    }

    public void StartDodge(Vector3 inputDirection)
    {
        CancelDodge();

        inputDirection.y = 0f;

        Vector3 moveDirection;

        if (inputDirection.sqrMagnitude > 0.0001f)
        {
            if (!TryGetCameraBasis(out Vector3 cameraForward, out Vector3 cameraRight))
            {
                return;
            }

            // 현재 플레이어 방향과 관계없이 카메라 기준 입력 방향 사용
            moveDirection = cameraForward * inputDirection.z + cameraRight * inputDirection.x;
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
        transform.rotation = Quaternion.LookRotation(moveDirection, Vector3.up);

        float movedDistance = 0f;

        dodgeTween = DOVirtual
            .Float(0f, dodgeDist, dodgeDuration, distance =>
            {
                float deltaDistance = distance - movedDistance;
                movedDistance = distance;

                characterController.Move(moveDirection * deltaDistance);
            })
            .SetEase(Ease.OutCubic)
            .SetTarget(this)
            .OnComplete(() => dodgeTween = null);
    }

    public void CancelDodge()
    {
        if (dodgeTween != null && dodgeTween.IsActive())
            dodgeTween.Kill();

        dodgeTween = null;
    }

    private bool TryGetCameraBasis(out Vector3 cameraForward, out Vector3 cameraRight)
    {
        cameraForward = Vector3.zero;
        cameraRight = Vector3.zero;

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        if (cameraTransform == null)
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
        float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;

        float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref rotationVelocity, rotationSmoothTime);

        transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
    }

    private void ApplyGravity()
    {
        if (characterController.isGrounded)
            verticalVelocity = groundedGravity;
        else
            verticalVelocity += gravity * Time.deltaTime;

        characterController.Move(Vector3.up * verticalVelocity * Time.deltaTime);
    }
}
