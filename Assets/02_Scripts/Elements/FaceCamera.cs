using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    private void LateUpdate()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
            return;

        Vector3 direction = transform.position - mainCamera.transform.position;
        if (direction.sqrMagnitude <= Mathf.Epsilon)
            return;

        transform.rotation = Quaternion.LookRotation(direction, mainCamera.transform.up);
    }
}
