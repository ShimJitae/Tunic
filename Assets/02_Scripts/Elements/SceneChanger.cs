using UnityEngine;

public class SceneChanger : MonoBehaviour
{
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Skrrr");
        }
    }
}
