using System;
using System.Collections.Generic;
using UnityEngine;

public enum InventoryCategory
{
    Consumable,
    Equipment
}

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
    [SerializeField] private int consumableCapacity = 24;
    [SerializeField] private int equipmentCapacity = 24;
    [SerializeField] private ItemDatabase itemDatabase;

    [Header("State")]
    [SerializeField] private List<InventorySlot> consumableSlots;
    [SerializeField] private List<InventorySlot> equipmentSlots;

    public event Action OnInventoryChanged;

    public IReadOnlyList<InventorySlot> ConsumableSlots => consumableSlots;
    public IReadOnlyList<InventorySlot> EquipmentSlots => equipmentSlots;

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

    private void Start()
    {
        TryLoadInventory();
    }

    private void OnApplicationQuit()
    {
        SaveInventory();
    }

    private void InitializeSlots()
    {
        if (consumableSlots == null)
        {
            consumableSlots = new List<InventorySlot>(consumableCapacity);
        }

        if (consumableSlots.Count != consumableCapacity)
        {
            consumableSlots.Clear();
            for (int i = 0; i < consumableCapacity; i++)
            {
                consumableSlots.Add(new InventorySlot());
            }
        }

        if (equipmentSlots == null)
        {
            equipmentSlots = new List<InventorySlot>(equipmentCapacity);
        }

        if (equipmentSlots.Count != equipmentCapacity)
        {
            equipmentSlots.Clear();
            for (int i = 0; i < equipmentCapacity; i++)
            {
                equipmentSlots.Add(new InventorySlot());
            }
        }
    }

    private void NotifyInventoryChanged()
    {
        OnInventoryChanged?.Invoke();
    }

    public int ConsumableCapacity => consumableCapacity;
    public int EquipmentCapacity => equipmentCapacity;

    public void Add(ItemDefinition def)
    {
        TryAddItem(def, 1);
    }

    public bool TryAddItem(ItemDefinition def, int amount = 1)
    {
        if (def == null || amount <= 0) return false;

        InventoryCategory category = ResolveCategory(def);
        var list = GetList(category);

        int added = 0;

        for (int n = 0; n < amount; n++)
        {
            int emptyIndex = -1;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].IsEmpty)
                {
                    emptyIndex = i;
                    break;
                }
            }

            if (emptyIndex == -1)
            {
                Debug.LogWarning($"Inventory full in {category} slots, cannot add item: {def.itemName}");
                break;
            }

            var slot = list[emptyIndex];
            slot.item = def;
            added++;
        }

        if (added > 0)
        {
            NotifyInventoryChanged();
            SaveInventory();
            return true;
        }

        return false;
    }

    public int GetCount(ItemDefinition def)
    {
        if (def == null) return 0;
        int c = 0;
        foreach (var slot in consumableSlots)
        {
            if (!slot.IsEmpty && slot.item == def)
            {
                c += 1;
            }
        }
        foreach (var slot in equipmentSlots)
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

        remaining = RemoveFromList(consumableSlots, def, remaining);
        if (remaining > 0)
        {
            remaining = RemoveFromList(equipmentSlots, def, remaining);
        }

        if (remaining < amount)
        {
            NotifyInventoryChanged();
            SaveInventory();
            return true;
        }

        return false;
    }

    public void ClearSlot(InventoryCategory category, int index)
    {
        var list = GetList(category);
        if (!IsValidIndex(list, index)) return;
        var slot = list[index];
        if (!slot.IsEmpty)
        {
            slot.Clear();
            NotifyInventoryChanged();
            SaveInventory();
        }
    }

    // Backwards-compat: defaults to Consumable category
    public void ClearSlot(int index) => ClearSlot(InventoryCategory.Consumable, index);

    public void MoveSlot(InventoryCategory category, int fromIndex, int toIndex)
    {
        var list = GetList(category);
        if (!IsValidIndex(list, fromIndex) || !IsValidIndex(list, toIndex)) return;
        if (fromIndex == toIndex) return;

        var from = list[fromIndex];
        var to = list[toIndex];

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
        SaveInventory();
    }

    public void MoveSlot(int fromIndex, int toIndex) => MoveSlot(InventoryCategory.Consumable, fromIndex, toIndex);

    public bool UseItem(InventoryCategory category, int slotIndex)
    {
        var list = GetList(category);
        if (!IsValidIndex(list, slotIndex)) return false;

        var slot = list[slotIndex];
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
            SaveInventory();
        }

        return used;
    }

    // Backwards-compat: defaults to Consumable category
    public bool UseItem(int slotIndex) => UseItem(InventoryCategory.Consumable, slotIndex);

    public IReadOnlyList<InventorySlot> GetSlots(InventoryCategory category)
    {
        return category == InventoryCategory.Consumable
            ? (IReadOnlyList<InventorySlot>)consumableSlots
            : equipmentSlots;
    }

    private InventoryCategory ResolveCategory(ItemDefinition def)
    {
        switch (def.itemType)
        {
            case ItemType.Consumable:
                return InventoryCategory.Consumable;
            case ItemType.Weapon:
            case ItemType.Armor:
                return InventoryCategory.Equipment;
            default:
                return InventoryCategory.Consumable;
        }
    }

    private List<InventorySlot> GetList(InventoryCategory category)
    {
        return category == InventoryCategory.Consumable ? consumableSlots : equipmentSlots;
    }

    private static int RemoveFromList(List<InventorySlot> list, ItemDefinition def, int remaining)
    {
        for (int i = 0; i < list.Count && remaining > 0; i++)
        {
            var slot = list[i];
            if (!slot.IsEmpty && slot.item == def)
            {
                slot.Clear();
                remaining--;
            }
        }

        return remaining;
    }

    private bool IsValidIndex(List<InventorySlot> list, int index)
    {
        return list != null && index >= 0 && index < list.Count;
    }

    // --- Save / Load ---

    public InventorySaveData BuildSaveData(EquipmentManager equipmentManager)
    {
        var data = new InventorySaveData();

        for (int i = 0; i < consumableSlots.Count; i++)
        {
            var slot = consumableSlots[i];
            if (slot != null && !slot.IsEmpty && slot.item != null)
            {
                data.consumableSlots.Add(new InventorySlotSaveData { slotIndex = i, itemId = slot.item.id });
            }
        }

        for (int i = 0; i < equipmentSlots.Count; i++)
        {
            var slot = equipmentSlots[i];
            if (slot != null && !slot.IsEmpty && slot.item != null)
            {
                data.equipmentSlots.Add(new InventorySlotSaveData { slotIndex = i, itemId = slot.item.id });
            }
        }

        if (equipmentManager != null)
        {
            data.equippedWeaponId = equipmentManager.EquippedWeapon?.id;
            data.equippedArmorId = equipmentManager.EquippedArmor?.id;
        }

        return data;
    }

    public void ApplySaveData(InventorySaveData data, ItemDatabase db, EquipmentManager equipmentManager)
    {
        if (data == null) return;

        InitializeSlots();

        if (db != null)
        {
            if (data.consumableSlots != null)
            {
                foreach (var entry in data.consumableSlots)
                {
                    if (entry.slotIndex >= 0 && entry.slotIndex < consumableSlots.Count && !string.IsNullOrEmpty(entry.itemId))
                    {
                        var item = db.GetItemById(entry.itemId);
                        if (item != null)
                            consumableSlots[entry.slotIndex].item = item;
                    }
                }
            }

            if (data.equipmentSlots != null)
            {
                foreach (var entry in data.equipmentSlots)
                {
                    if (entry.slotIndex >= 0 && entry.slotIndex < equipmentSlots.Count && !string.IsNullOrEmpty(entry.itemId))
                    {
                        var item = db.GetItemById(entry.itemId);
                        if (item != null)
                            equipmentSlots[entry.slotIndex].item = item;
                    }
                }
            }

            if (equipmentManager != null)
            {
                if (!string.IsNullOrEmpty(data.equippedWeaponId))
                {
                    var weapon = db.GetItemById(data.equippedWeaponId);
                    equipmentManager.SetEquippedWeapon(weapon);
                }
                if (!string.IsNullOrEmpty(data.equippedArmorId))
                {
                    var armor = db.GetItemById(data.equippedArmorId);
                    equipmentManager.SetEquippedArmor(armor);
                }
            }
        }

        NotifyInventoryChanged();
    }

    public void SaveInventory()
    {
        var equipmentManager = EquipmentManager.Instance;
        InventorySaveData data = BuildSaveData(equipmentManager);
        InventorySaveSystem.SaveToFile(data);
    }

    public void TryLoadInventory()
    {
        InventorySaveData data = InventorySaveSystem.LoadFromFile();
        if (data == null)
        {
            NotifyInventoryChanged();
            return;
        }

        var equipmentManager = EquipmentManager.Instance;
        ApplySaveData(data, itemDatabase, equipmentManager);
    }
}

