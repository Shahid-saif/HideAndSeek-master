[System.Serializable]
public class CollectableItem {
    public Collectable item;
    public int count;

    public string name { get { return item.name; } }
}

public enum CollectableType { Wood }
