using UnityEngine;

[RequireComponent(typeof(EventTrigger))]
public class LoadSceneOnExit : MonoBehaviour
{
    private EventTrigger eventTrigger;

    private void Awake()
    {
        eventTrigger = GetComponent<EventTrigger>();
    }

    private void OnEnable()
    {
        eventTrigger.TriggerExited += LoadScene;
    }

    private void OnDisable()
    {
        eventTrigger.TriggerExited -= LoadScene;
    }

    private void LoadScene(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        SceneLoadManager.Instance.LoadBossScene();
    }
}
