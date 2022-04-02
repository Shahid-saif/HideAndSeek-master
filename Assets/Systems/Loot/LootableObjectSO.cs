using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LootItem", menuName = "ScriptableObject/Loot Object")]
public class LootableObjectSO : ScriptableObject {
    public new string name;
    public LootItemType itemType;
    public int animationId;
    public float timeToLoot;
    public bool regenratable;
    // public GameObject model;
    // public Sprite icon;
    public List<CollectableItem> collectables;
}

public enum LootItemType {
    Tree,
    Stone
}
