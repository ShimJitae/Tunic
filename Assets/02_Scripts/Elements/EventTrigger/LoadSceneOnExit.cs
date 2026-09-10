using UnityEngine;

[RequireComponent(typeof(TriggerEventBlock))]
public class LoadSceneOnExit : MonoBehaviour
{
    [SerializeField] private string sceneName;
    private TriggerEventBlock eventTrigger;

    private void Awake()
    {
        eventTrigger = GetComponent<TriggerEventBlock>();
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

        SceneLoadManager.Instance.LoadScene(sceneName);
    }
}
