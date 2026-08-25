using System;
using UnityEngine;

[Serializable]
public struct StateTransitionBlocker
{
    public StateType CurrentState;
    public StateType NextState;
}
