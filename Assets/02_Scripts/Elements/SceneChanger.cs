using UnityEngine;

public class SceneChanger : MonoBehaviour
{
    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        SceneLoadManager.Instance.LoadBossScene();
    }
}
