using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Shoot(Vector3 direction, float speed)
    {
        transform.rotation = Quaternion.LookRotation(direction);
        // rigidbody 회전값을 초기화
        rb.rotation = Quaternion.LookRotation(transform.forward);
    }

    public void Release()
    {
        if (!gameObject.activeSelf)
        {
            return;
        }

        rb.rotation = Quaternion.identity;
        rb.linearVelocity = rb.angularVelocity = Vector3.zero;
    }
}
