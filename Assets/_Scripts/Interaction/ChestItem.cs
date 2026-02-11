using UnityEngine;

public class ChestItem : InteractiveElement
{
    [Header("Chest")]
    public ItemDefinition loot;

    bool opened = false;

    public override void OnInteract()
    {
        if (opened)
            return;

        if (loot != null)
        {
            InventoryManager.Instance?.Add(loot);
            InteractionManager.Instance?.ShowCollected(loot);
        }

        opened = true;

        InteractionManager.Instance?.ClearFocus(this);

        var col = GetComponent<SphereCollider>();
        if (col != null) col.enabled = false;
        this.enabled = false;
    }

    public override void OnFocus()
    {
        
    }

    public override void OnDefocus()
    {
    }
}
