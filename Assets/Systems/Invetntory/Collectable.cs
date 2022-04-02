using UnityEngine;


[CreateAssetMenu(fileName = "Collectable Item", menuName = "ScriptableObject/Collectable Item")]
public class Collectable : ScriptableObject {
    public new string name;
    public CollectableType type;
    // public GameObject model;
    // public Sprite icon;
}
