using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Boss Is Dead", story: "[Agent] is dead", category: "Conditions", id: "e24a86dfaa1ea1dbc3cef805c538af58")]
public partial class BossIsDeadCondition : Condition
{
    [SerializeReference]
    public BlackboardVariable<GameObject> Agent;

    [NonSerialized]
    private GameObject cachedAgent;

    [NonSerialized]
    private Health cachedHealth;

    public override bool IsTrue()
    {
        GameObject currentAgent = Agent?.Value;

        if (currentAgent == null)
        {
            cachedAgent = null;
            cachedHealth = null;
            return false;
        }

        // Agent가 바뀌었거나 아직 Health를 찾지 않았다면 다시 찾는다.
        if (cachedAgent != currentAgent || cachedHealth == null)
        {
            cachedAgent = currentAgent;
            currentAgent.TryGetComponent(out cachedHealth);
        }

        return cachedHealth != null && cachedHealth.IsDied;
    }
}
