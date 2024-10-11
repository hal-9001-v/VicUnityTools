using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class ClickListener : MonoBehaviour
{
    [SerializeField] UnityEvent onClickEvent;
    public Action OnClickCallback;

    [SerializeField] UnityEvent onMouseEnterEvent;
    public Action OnMouseEnterCallback;

    [SerializeField] UnityEvent onMouseExitEvent;
    public Action OnMouseExitCallback;


    [SerializeField] bool debug;
    private void OnMouseDown()
    {
        if (debug)
            Debug.Log("Clicked on " + gameObject.name);

        onClickEvent.Invoke();
        OnClickCallback?.Invoke();
    }

    private void OnMouseEnter()
    {
        if (debug)
            Debug.Log("Mouse enter on " + gameObject.name);

        onMouseEnterEvent.Invoke();
        OnMouseEnterCallback?.Invoke();
    }

    private void OnMouseExit()
    {
        if (debug)
            Debug.Log("Mouse exit on " + gameObject.name);

        onMouseExitEvent.Invoke();
        OnMouseExitCallback?.Invoke();
    }
}
