using UnityEngine;
using UnityEngine.Events;

public class InteractionEventRegisterer : MonoBehaviour {

    public UnityEvent Event;
    private void InvokeEvent() {
        Event?.Invoke();
    }

    private void Start() {
        // check for errors
        Event.AddListener(GetComponent<IInteractable>().OnPlayerInteraction);
    }

    public InteractableType interactableType;

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            InteractionEventManager.instance.AddEvent(InvokeEvent);
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player")) {
            InteractionEventManager.instance.RemoveEvent(InvokeEvent);
        }
    }
}
