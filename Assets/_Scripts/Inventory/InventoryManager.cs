using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InventorySlot
{
    public ItemDefinition item;

    public bool IsEmpty => item == null;

    public void Clear()
    {
        item = null;
    }
}

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Config")]
    [SerializeField] private int capacity = 24;

    [Header("State")]
    [SerializeField] private List<InventorySlot> slots;

    public event Action OnInventoryChanged;

    public IReadOnlyList<InventorySlot> Slots => slots;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InitializeSlots();
    }

    private void InitializeSlots()
    {
        if (slots == null)
        {
            slots = new List<InventorySlot>(capacity);
        }

        if (slots.Count != capacity)
        {
            slots.Clear();
            for (int i = 0; i < capacity; i++)
            {
                slots.Add(new InventorySlot());
            }
        }
    }

    private void NotifyInventoryChanged()
    {
        OnInventoryChanged?.Invoke();
    }

    public int Capacity => capacity;

    public void Add(ItemDefinition def)
    {
        TryAddItem(def, 1);
    }

    public bool TryAddItem(ItemDefinition def, int amount = 1)
    {
        if (def == null || amount <= 0) return false;

        int added = 0;

        for (int n = 0; n < amount; n++)
        {
            int emptyIndex = -1;
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].IsEmpty)
                {
                    emptyIndex = i;
                    break;
                }
            }

            if (emptyIndex == -1)
            {
                Debug.LogWarning("Inventory full, cannot add item: " + def.itemName);
                break;
            }

            var slot = slots[emptyIndex];
            slot.item = def;
            added++;
        }

        if (added > 0)
        {
            NotifyInventoryChanged();
            return true;
        }

        return false;
    }

    public int GetCount(ItemDefinition def)
    {
        if (def == null) return 0;
        int c = 0;
        foreach (var slot in slots)
        {
            if (!slot.IsEmpty && slot.item == def)
            {
                c += 1;
            }
        }
        return c;
    }

    public bool TryRemoveItem(ItemDefinition def, int amount = 1)
    {
        if (def == null || amount <= 0) return false;

        int remaining = amount;

        for (int i = 0; i < slots.Count && remaining > 0; i++)
        {
            var slot = slots[i];
            if (!slot.IsEmpty && slot.item == def)
            {
                slot.Clear();
                remaining--;
            }
        }

        if (remaining < amount)
        {
            NotifyInventoryChanged();
            return true;
        }

        return false;
    }

    public void ClearSlot(int index)
    {
        if (!IsValidIndex(index)) return;
        var slot = slots[index];
        if (!slot.IsEmpty)
        {
            slot.Clear();
            NotifyInventoryChanged();
        }
    }

    public void MoveSlot(int fromIndex, int toIndex)
    {
        if (!IsValidIndex(fromIndex) || !IsValidIndex(toIndex)) return;
        if (fromIndex == toIndex) return;

        var from = slots[fromIndex];
        var to = slots[toIndex];

        if (from.IsEmpty) return;

        // If target empty, move
        if (to.IsEmpty)
        {
            to.item = from.item;
            from.Clear();
        }
        else
        {
            // Swap
            var tempItem = to.item;

            to.item = from.item;

            from.item = tempItem;
        }

        NotifyInventoryChanged();
    }

    public bool UseItem(int slotIndex)
    {
        if (!IsValidIndex(slotIndex)) return false;

        var slot = slots[slotIndex];
        if (slot.IsEmpty || slot.item == null) return false;

        var item = slot.item;
        bool used = false;

        switch (item.itemType)
        {
            case ItemType.Consumable:
                Debug.Log("Using consumable: " + item.itemName);
                used = true;
                slot.Clear();
                break;

            case ItemType.Weapon:
                if (EquipmentManager.Instance != null)
                {
                    if (EquipmentManager.Instance.EquippedWeapon == item)
                    {
                        EquipmentManager.Instance.UnequipWeapon();
                    }
                    else
                    {
                        EquipmentManager.Instance.EquipWeapon(item);
                    }
                    used = true;
                }
                else
                {
                    Debug.LogWarning("Tried to equip weapon but no EquipmentManager in scene.");
                }
                break;

            case ItemType.Armor:
                if (EquipmentManager.Instance != null)
                {
                    if (EquipmentManager.Instance.EquippedArmor == item)
                    {
                        EquipmentManager.Instance.UnequipArmor();
                    }
                    else
                    {
                        EquipmentManager.Instance.EquipArmor(item);
                    }
                    used = true;
                }
                else
                {
                    Debug.LogWarning("Tried to equip armor but no EquipmentManager in scene.");
                }
                break;

            default:
                Debug.Log("Item is not usable/equippable: " + item.itemName);
                break;
        }

        if (used)
        {
            NotifyInventoryChanged();
        }

        return used;
    }

    private bool IsValidIndex(int index)
    {
        return slots != null && index >= 0 && index < slots.Count;
    }
}
