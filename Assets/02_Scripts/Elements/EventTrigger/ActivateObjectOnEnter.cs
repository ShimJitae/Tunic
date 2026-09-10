using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EventTrigger))]
public class ActivateObjectOnEnter : MonoBehaviour
{
    [SerializeField] private List<GameObject> targets = new();

    private EventTrigger eventTrigger;

    private void Awake()
    {
        eventTrigger = GetComponent<EventTrigger>();
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
