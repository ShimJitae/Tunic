using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StateMachine : MonoBehaviour
{
    // Enter가 된 이후부터 Update 실행
    private bool completeEnter;
    public IState CurrentState { get; private set; }
    [SerializeField] private List<StateTransitionBlocker> transitionBlockConditions;

    public void ChangeState(IState nextState)
    {
        if (CurrentState == nextState)
            return;

        CurrentState?.Exit();

        CurrentState = nextState;

        CurrentState?.Enter();

        completeEnter = true;
    }

    public void Update()
    {
        if (!completeEnter)
            return;

        CurrentState?.Tick();
    }
}