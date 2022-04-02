using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractionEventManager : MonoBehaviour {

    public static InteractionEventManager instance;

    public UnityEvent Event;

    public void InvokeEvent() {
        Event?.Invoke();
    }

    private void Awake() {
        instance = this;
    }

    public void AddEvent(UnityAction action) {
        Event.AddListener(action);
    }

    public void RemoveEvent(UnityAction action) {
        Event.RemoveListener(action);
    }

    public void RemoveAllEvent(UnityAction action) {
        Event.RemoveAllListeners();
    }
}

public enum InteractableType { Loot, Talk, Collect }

