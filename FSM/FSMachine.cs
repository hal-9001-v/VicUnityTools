using System;
using UnityEngine;

[Serializable]
public class FSMachine : FSMState
{
    [SerializeField] FSMState currentState;
    FSMState startingState;
    [SerializeField] bool debug;

    public FSMachine(FSMState startingState, bool debug = false, string stateName = "SFM", Action enter = null, Action exit = null) : base(stateName, enter, null, exit)
    {
        currentState = startingState;
        this.startingState = startingState;
        currentState.Enter();

        this.debug = debug;
        if (debug)
        {
            Debug.Log(stateName + " Machine - Current State: " + currentState.Name);
        }

        base.action = Update;
    }

    public void Update()
    {
        FSMState nextState;

        if (currentState.CheckTransitionToChildren(out nextState))
        {
            currentState.Exit();
            currentState = nextState;
            currentState.Enter();

            if (debug)
            {
                Debug.Log(Name + " Machine - Current State: " + currentState.Name);
            }
        }

        try
        {
            currentState.Execute();
        }
        catch (Exception e)
        {
            Debug.LogError(Name + " Machine - Error in FSM state " + currentState.Name + "--> " + e);
        }
    }

    void ChangeState(FSMState newState)
    {
        currentState.Exit();
        currentState = newState;
        currentState.Enter();
    }

    public void Reset()
    {
        currentState.Exit();
        currentState = startingState;
        currentState.Enter();
    }
}