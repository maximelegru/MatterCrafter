using UnityEngine;

[CreateAssetMenu(fileName = "InventoryItemData", menuName = "Inventory/ItemData")]
public class InventoryItemData : ScriptableObject
{
    public string id;
    public Sprite icon;
    public GameObject prefabModel;
}
