using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    [SerializeField] private GameObject bulletModel;
    private Rigidbody rb;
    float speed;
    float releaseTimer;

    private Tween releaseTween;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    private void OnDisable()
    {
        releaseTween?.Kill();
        releaseTween = null;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    public void Shoot(Vector3 direction, float _speed, float _releaseTimer = 5f)
    {
        if (direction.sqrMagnitude <= 0.0001f || _speed <= 0f)
            return;

        direction.Normalize();

        speed = _speed;
        releaseTimer = _releaseTimer;
        transform.forward = direction;
        rb.rotation = Quaternion.LookRotation(transform.forward); // rigidbody 회전값을 초기화
        rb.linearVelocity = direction * speed;

        // 기존 반환 예약 제거 후 다시 예약
        releaseTween?.Kill();

        releaseTween = DOVirtual.DelayedCall(
        releaseTimer,
        Release,
        false // Time.timeScale의 영향을 받음
    );
    }

    private void Release()
    {
        if (!gameObject.activeSelf)
        {
            return;
        }

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        releaseTween = null;
        gameObject.SetActive(false);
    }
}
