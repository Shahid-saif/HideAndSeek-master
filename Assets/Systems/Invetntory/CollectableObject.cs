using UnityEngine;

[RequireComponent(typeof(InteractionEventRegisterer))]
public class CollectableObject : MonoBehaviour, IInteractable {

    public CollectableItem item;
    private bool collected = false;

    public void OnPlayerInteraction() {
        if (collected) return;

        InventoryManager.instance.AddToInventory(item);
        collected = true;
        Debug.Log("I Got: " + item.name);
        Destroy(gameObject);
    }
}
