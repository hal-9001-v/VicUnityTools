using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PeriodEvent : MonoBehaviour
{
    [SerializeField][Range(0, 60)] float period;
    [SerializeField] bool executeOnStart;
    public UnityEvent callback;

    private void Awake()
    {
        StopAllCoroutines();
        StartCoroutine(PeriodicEvent());
    }

    IEnumerator PeriodicEvent()
    {
        if (executeOnStart)
            callback.Invoke();

        while (true)
        {
            yield return new WaitForSeconds(period);
            callback.Invoke();
        }
    }
}
