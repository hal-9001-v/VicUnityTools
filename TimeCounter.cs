using System;
using UnityEngine;

[Serializable]
public class TimeCounter
{
    public float ElapsedTime { get; private set; }
    public float TargetTime => targetTime;

    [SerializeField][Range(0, 5)] float targetTime;

    public bool IsFinished => ElapsedTime >= targetTime;

    public TimeCounter(float target)
    {
        ElapsedTime = 0;

        targetTime = target;
    }

    public void Reset()
    {
        ElapsedTime = 0;
    }

    public void Reset(float targetTime)
    {
        this.targetTime = targetTime;
        ElapsedTime = 0;
    }


    /// <summary>
    /// Updates elapsed time and returns true if it is greater than targetTime
    /// </summary>
    /// <returns></returns>
    public bool Update(bool useUnscaled = false)
    {
        if (useUnscaled)
            ElapsedTime += Time.unscaledDeltaTime;
        else
            ElapsedTime += Time.deltaTime;

        if (IsFinished)
        {
            ElapsedTime = targetTime * 1.1f;
            return true;
        }

        return false;
    }

}
