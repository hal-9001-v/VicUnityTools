using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSMCondition
{
    public enum ConditionType
    {
        AND,
        OR,
        NOR,
        XOR,
        NOT
    }

    public Func<bool> Condition { get; protected set; }

    /// <summary>
    /// Reset action to be called when the using state is entered. Optional.
    /// </summary>
    public Action ResetAction { get; protected set; }

    public FSMCondition(Func<bool> condition, Action resetAction = null)
    {
        Condition = condition;
        ResetAction = resetAction;
    }

    /// <summary>
    /// Reverse condition
    /// </summary>
    /// <param name="toReverse"></param>
    public FSMCondition(FSMCondition toReverse, Action resetAction = null)
    {
        if (toReverse == null)
        {
            Debug.LogError("FSMCondition: toReverse is null");
        }

        Condition = () => !toReverse.Check();

        if (resetAction != null)
        {
            ResetAction = resetAction;
        }
        else
        {
            ResetAction = toReverse.ResetAction;
        }
    }

    public FSMCondition(FSMCondition a, FSMCondition b, ConditionType conditionType = ConditionType.AND, Action resetAction = null)
    {
        if (a == null)
        {
            Debug.LogError("FSMCondition: a is null");
        }

        if (b == null)
        {
            Debug.LogError("FSMCondition: b is null");
        }


        if (resetAction != null)
        {
            ResetAction = resetAction;
        }
        else
        {
            ResetAction = () =>
            {
                a.Reset();
                b.Reset();
            };
        }

        switch (conditionType)
        {
            case ConditionType.AND:
                Condition = () =>
                {
                    return a.Check() && b.Check();
                };
                break;

            case ConditionType.OR:
                Condition = () =>
                {
                    return a.Check() || b.Check();
                };
                break;

            case ConditionType.NOR:
                Condition = () =>
                {
                    return !(a.Check() || b.Check());
                };
                break;

            case ConditionType.XOR:
                Condition = () =>
                {
                    return a.Check() ^ b.Check();
                };
                break;

            case ConditionType.NOT:
                Condition = () =>
                {
                    return !a.Check();
                };
                break;
        }

    }

    public FSMCondition Reversed()
    {
        return new FSMCondition(this);
    }

    public static FSMCondition TrueCond => new FSMCondition(() => true);
  
    public static FSMCondition FalseCond => new FSMCondition(() => false);

    public virtual bool Check()
    {
        return Condition();
    }

    public virtual void Reset()
    {
        ResetAction?.Invoke();
    }

    public static FSMCondition operator *(FSMCondition a, FSMCondition b)
    {
        return new FSMCondition(a, b, ConditionType.AND);
    }

    public static FSMCondition operator +(FSMCondition a, FSMCondition b)
    {
        return new FSMCondition(a, b, ConditionType.OR);
    }

    public static FSMCondition operator !(FSMCondition a)
    {
        return new FSMCondition(a);
    }
}
