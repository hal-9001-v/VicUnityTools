using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DirectInteractable : MonoBehaviour
{
    public UnityEvent InteractEvent;
    public Action<Interactor> OnInteractCallback;

    public void Interact(Interactor interactor)
    {
        InteractEvent?.Invoke();
        OnInteractCallback?.Invoke(interactor);
    }
}
