using UnityEngine;

[CreateAssetMenu(menuName = "Items/Item Definition", fileName = "NewItem")]
public class ItemDefinition : ScriptableObject
{
    [Header("Identity")]
    public string id;
    public string itemName;

    [Header("Presentation")]
    public Sprite icon;
    [TextArea]
    public string description;
}
