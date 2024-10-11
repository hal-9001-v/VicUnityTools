using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FSMState
{
    public string Name { get => name; }
    [SerializeField] string name;

    protected Action enter;
    protected Action action;
    protected Action exit;

    public bool blockTransitions;

    List<Transition> transitions;

    public FSMState(string name, Action enter, Action action = null, Action exit = null)
    {
        this.name = name;

        this.action = action;

        transitions = new List<Transition>();
        this.enter = enter;
        this.exit = exit;
    }

    public void AddTransition(FSMCondition condition, FSMState state)
    {
        transitions.Add(new Transition(state, condition));
    }

    public void AddTransition(FSMState state, FSMCondition condition)
    {
        AddTransition(condition, state);
    }

    public bool CheckTransitionToChildren(out FSMState nextState)
    {
        if (blockTransitions)
        {
            nextState = this;
            return false;
        }

        foreach (Transition transition in transitions)
        {
            if (transition.Check())
            {
                nextState = transition.state;
                return true;
            }
        }

        nextState = this;

        return false;
    }

    public void Enter()
    {
        foreach (var transition in transitions)
        {
            transition.Reset();
        }

        enter?.Invoke();
    }

    public void Execute()
    {
        action?.Invoke();
    }

    public void Exit()
    {
        exit?.Invoke();
    }

    class Transition
    {
        public FSMState state;
        FSMCondition condition;

        public Transition(FSMState to, FSMCondition condition)
        {
            this.state = to;
            this.condition = condition;

            if (to == null)
            {
                Debug.LogWarning("State is null for " + condition);
            }

        }

        public bool Check()
        {
            if (condition == null)
                return false;

            return condition.Check();
        }

        public void Reset()
        {
            if (condition != null)
                condition.Reset();

        }
    }
}