using UnityEngine;

public class PickupItem : InteractiveElement
{
    [Header("Pickup")]
    public ItemDefinition itemDefinition;

    public override void OnInteract()
    {
        if (itemDefinition != null)
        {
            InventoryManager.Instance?.Add(itemDefinition);
            InteractionManager.Instance?.ShowCollected(itemDefinition);
        }

        InteractionManager.Instance?.ClearFocus(this);
        Destroy(gameObject);
    }

    public override void OnFocus()
    {
    }

    public override void OnDefocus()
    {
    }
}
