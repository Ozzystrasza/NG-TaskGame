using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Item Database", fileName = "ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    [SerializeField] private List<ItemDefinition> allItems = new List<ItemDefinition>();

    private Dictionary<string, ItemDefinition> _idToItem;

    public void BuildLookup()
    {
        _idToItem = new Dictionary<string, ItemDefinition>();
        if (allItems == null) return;

        foreach (ItemDefinition item in allItems)
        {
            if (item == null || string.IsNullOrEmpty(item.id)) continue;
            _idToItem[item.id] = item;
        }
    }

    public ItemDefinition GetItemById(string id)
    {
        if (_idToItem == null)
            BuildLookup();

        if (string.IsNullOrEmpty(id))
            return null;

        return _idToItem != null && _idToItem.TryGetValue(id, out ItemDefinition item) ? item : null;
    }
}
