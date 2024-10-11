using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TriggerInteractable : MonoBehaviour
{
    [SerializeField] protected UnityEvent<Interactor> onEnterEvent;
    [SerializeField] protected UnityEvent<Interactor> onExitEvent;

    public Action<Interactor> OnEnterAction;
    public Action<Interactor> OnStayAction;
    public Action<Interactor> OnExitAction;

    [SerializeField] protected bool debug;

    [HideInInspector] public List<Interactor> Interactors;

    [SerializeField] InteractorTag targetTag;

    public Collider Collider => GetComponent<Collider>();

    [SerializeField] bool onlyOnce;
    bool done;

    private void Awake()
    {
        Interactors = new List<Interactor>();
        Collider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (onlyOnce && done) return;
        Interactor interactor = other.GetComponent<Interactor>();
        if (interactor != null)
        {
            if (IsInteractorValid(interactor) == false) return;

            Interactors.Add(interactor);
            onEnterEvent.Invoke(interactor);
            OnEnterAction?.Invoke(interactor);
            if (debug)
            {
                Debug.Log("TriggerInteractable: " + name + " enter triggered by " + interactor.name);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(onlyOnce && done) return;
        Interactor interactor = other.GetComponent<Interactor>();
        if (interactor != null)
        {
            if (IsInteractorValid(interactor) == false) return;

            Interactors.Remove(interactor);
            onExitEvent.Invoke(interactor);
            OnExitAction?.Invoke(interactor);

            if (debug)
            {
                Debug.Log("TriggerInteractable: " + name + " exit triggered by " + interactor.name);
            }

            done = true;
        }

    }

    private void OnTriggerStay(Collider other)
    {
        if (onlyOnce && done) return;
        Interactor interactor = other.GetComponent<Interactor>();
        if (interactor != null)
        {
            if (IsInteractorValid(interactor) == false) return;

            OnStayAction?.Invoke(interactor);
        }
    }

    bool IsInteractorValid(Interactor interactor)
    {
        if (interactor.InteractorTag == InteractorTag.Any) return true;
        if (targetTag == InteractorTag.Any) return true;
        if (interactor.InteractorTag == targetTag) return true;

        return false;
    }

}
