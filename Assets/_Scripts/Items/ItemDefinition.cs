using UnityEngine;

public enum ItemType
{
    Misc,
    Consumable,
    Weapon,
    Armor
}

[CreateAssetMenu(menuName = "Items/Item Definition", fileName = "NewItem")]
public class ItemDefinition : ScriptableObject
{
    [Header("Identity")]
    public string id;
    public string itemName;

    [Header("Type")]
    public ItemType itemType = ItemType.Misc;

    [Header("Presentation")]
    public Sprite icon;
    [TextArea]
    public string description;
}
