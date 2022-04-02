using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour {
    public static InventoryManager instance;

    void Awake() {
        instance = this;
    }

    public Dictionary<string, CollectableItem> collectables = new Dictionary<string, CollectableItem>();

    private void Update() {
        PrintInventory();
    }

    public void AddToInventory(CollectableItem newItem) {
        if (!collectables.ContainsKey(newItem.name)) {
            collectables.Add(newItem.name, newItem);
        } else {
            collectables[newItem.name].count += newItem.count;
        }
    }

    public void RemoveFromInventory(CollectableItem item) {
        collectables.Remove(item.name);
    }

    public void PrintInventory() {
        foreach (var item in collectables) {
            Debug.Log($"{item.Key}: {item.Value.count}");
        }
    }
}
