using UnityEngine;

[RequireComponent(typeof(TriggerEventBlock))]
public class ShowMessageOnEnter : MonoBehaviour
{
    [SerializeField] private TopMessageUI messageUI;
    [SerializeField, TextArea] private string message;

    private TriggerEventBlock eventTrigger;

    private void Awake()
    {
        eventTrigger = GetComponent<TriggerEventBlock>();
    }

    private void OnEnable()
    {
        eventTrigger.TriggerEntered += ShowMessage;
    }

    private void OnDisable()
    {
        eventTrigger.TriggerEntered -= ShowMessage;
    }

    private void ShowMessage(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        messageUI.Show(message);
    }
}
