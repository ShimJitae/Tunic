using UnityEngine;
using Unity.Behavior;

[RequireComponent(typeof(TriggerEventBlock))]
public class EnterBossArea : MonoBehaviour
{
    [SerializeField] private BehaviorGraphAgent b_agent;
    [SerializeField] private BossHPBarEnabler bossHPBarEnabler;
    private TriggerEventBlock eventTrigger;

    private void Awake()
    {
        eventTrigger = GetComponent<TriggerEventBlock>();
    }

    void Start()
    {
        bossHPBarEnabler.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        eventTrigger.TriggerExited += EnterArea;
    }

    private void OnDisable()
    {
        eventTrigger.TriggerExited -= EnterArea;
    }

    private void EnterArea(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        b_agent.SetVariableValue("IsPlayerEnterArea", true);
        bossHPBarEnabler.gameObject.SetActive(true);
    }
}