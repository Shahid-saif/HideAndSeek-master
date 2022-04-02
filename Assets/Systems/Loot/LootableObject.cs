using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(InteractionEventRegisterer))]
public class LootableObject : MonoBehaviour, IInteractable {

    public LootableObjectSO data;

    private new string name;
    private LootItemType itemType;
    private int animationId;
    private float timeToLoot;
    private List<CollectableItem> collectables = new List<CollectableItem>();

    void Start() {
        name = data.name;
        itemType = data.itemType;
        animationId = data.animationId;
        timeToLoot = data.timeToLoot;
        collectables = data.collectables;
    }

    void Update() {

    }

    public void OnPlayerInteraction() {
        if (timeToLoot > 0f) {
            timeToLoot -= Time.deltaTime;
            Debug.Log(timeToLoot);
        } else {
            Destroy(gameObject);
            collectables.ForEach(c => InventoryManager.instance.AddToInventory(c));
        }
    }
}
