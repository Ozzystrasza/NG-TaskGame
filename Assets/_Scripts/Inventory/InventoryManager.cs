using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    private List<ItemDefinition> items;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        items = new List<ItemDefinition>();
        DontDestroyOnLoad(gameObject);
    }

    public void Add(ItemDefinition def)
    {
        if (def == null) return;
        items.Add(def);
    }

    public int GetCount(ItemDefinition def)
    {
        if (def == null) return 0;
        int c = 0;
        foreach (var i in items) if (i == def) c++;
        return c;
    }
}
