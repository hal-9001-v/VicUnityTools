using System;
using UnityEngine;

[Serializable]
public class FSMTimeCondition : FSMCondition
{
    [SerializeField] TimeCounter timeCounter;

    public FSMTimeCondition(float duration) : base((Func<bool>)null)
    {
        timeCounter = new TimeCounter(duration);

        //Usually Reset() calls for Reset Action, but since this condition is useful to be set on the inspector, the constructor might not be called, so Reset() is holding the Action
        ResetAction = Reset;
    }

    public FSMTimeCondition(FSMCondition toReverse) : base(toReverse)
    {
        throw new Exception("FSMTimeCondition: Reverse condition not supported");
    }

    public FSMTimeCondition(FSMCondition a, FSMCondition b, ConditionType conditionType = ConditionType.AND) : base(a, b, conditionType)
    {
        throw new Exception("FSMTimeCondition: ConditionType not supported");
    }


    public override bool Check()
    {

        if (timeCounter.Update())
        {

            return true;
        }

        return false;
    }

    public override void Reset()
    {
        timeCounter.Reset();
    }
}
