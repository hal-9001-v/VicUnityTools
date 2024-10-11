using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CountdownEvent : MonoBehaviour
{

    [SerializeField][Range(0, 20)] float duration = 1;
    
    [SerializeField] UnityEvent onCountdownFinishedEvent;
    public Action OnCountdownFinishedCallback;
    
    [SerializeField] bool startOnAwake = false;
    

    private void Start()
    {
        if (startOnAwake)
        {
            StartCountdown();
        }
    }

    public void StartCountdown(float duration)
    {
        this.duration = duration;
        StartCountdown();
    }

    public void StartCountdown()
    {
        StopAllCoroutines();
        StartCoroutine(Countdown());
    }

    public void StopCountdown()
    {
        StopAllCoroutines();
    }

    IEnumerator Countdown()
    {
        yield return new WaitForSeconds(duration);
        onCountdownFinishedEvent.Invoke();
        OnCountdownFinishedCallback?.Invoke();
    }
}
