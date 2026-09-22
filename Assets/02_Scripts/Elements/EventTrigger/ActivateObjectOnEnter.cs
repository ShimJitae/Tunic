using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TriggerEventBlock))]
public class ActivateObjectOnEnter : MonoBehaviour
{
    [SerializeField] private List<GameObject> targets = new();

    private TriggerEventBlock eventTrigger;

    private void Awake()
    {
        eventTrigger = GetComponent<TriggerEventBlock>();
    }

    void Start()
    {
        foreach (GameObject target in targets)
        {
            if (target != null)
                target.SetActive(false);
        }
    }

    private void OnEnable()
    {
        eventTrigger.TriggerEntered += ActivateObjects;
    }

    private void OnDisable()
    {
        eventTrigger.TriggerEntered -= ActivateObjects;
    }

    private void ActivateObjects(Collider other)
    {
        foreach (GameObject target in targets)
        {
            if (target != null)
                target.SetActive(true);
        }
    }
}
