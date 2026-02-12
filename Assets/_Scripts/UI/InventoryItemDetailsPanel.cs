using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemDetailsPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI itemNameLabel;
    [SerializeField] private TextMeshProUGUI descriptionLabel;
    [SerializeField] private Button actionButton;
    [SerializeField] private TextMeshProUGUI actionButtonLabel;
    [SerializeField] private Button discardButton;

    private int currentSlotIndex = -1;
    private ItemDefinition currentItem;
    private InventoryCategory currentCategory = InventoryCategory.Consumable;

    private void OnEnable()
    {
        Clear();
    }

    public void ShowItem(InventoryCategory category, int slotIndex, InventorySlot slot)
    {
        currentCategory = category;
        currentSlotIndex = slotIndex;

        if (slot == null || slot.IsEmpty || slot.item == null)
        {
            Clear();
            return;
        }

        currentItem = slot.item;

        if (itemNameLabel != null)
        {
            itemNameLabel.text = currentItem.itemName;
        }

        if (descriptionLabel != null)
        {
            descriptionLabel.text = currentItem.description;
        }

        SetupActionButton();
    }

    public void Clear()
    {
        currentSlotIndex = -1;
        currentItem = null;

        if (itemNameLabel != null)
        {
            itemNameLabel.text = string.Empty;
        }

        if (descriptionLabel != null)
        {
            descriptionLabel.text = string.Empty;
        }

        if (actionButton != null)
        {
            actionButton.gameObject.SetActive(false);
        }

        if (discardButton != null)
        {
            discardButton.gameObject.SetActive(false);
        }
    }

    private void SetupActionButton()
    {
        if (actionButton == null || actionButtonLabel == null)
            return;

        if (currentItem == null)
        {
            actionButton.gameObject.SetActive(false);
            return;
        }

        string label = string.Empty;

        switch (currentItem.itemType)
        {
            case ItemType.Consumable:
                label = "Use";
                break;

            case ItemType.Weapon:
                if (EquipmentManager.Instance != null && EquipmentManager.Instance.EquippedWeapon == currentItem)
                {
                    label = "Unequip";
                }
                else
                {
                    label = "Equip";
                }
                break;

            case ItemType.Armor:
                if (EquipmentManager.Instance != null && EquipmentManager.Instance.EquippedArmor == currentItem)
                {
                    label = "Unequip";
                }
                else
                {
                    label = "Equip";
                }
                break;

            default:
                label = string.Empty;
                break;
        }

        if (string.IsNullOrEmpty(label))
        {
            actionButton.gameObject.SetActive(false);
        }
        else
        {
            actionButton.gameObject.SetActive(true);
            actionButtonLabel.text = label;
        }

        if (discardButton != null)
        {
            discardButton.gameObject.SetActive(true);
        }
    }

    public void OnActionButtonClicked()
    {
        if (currentItem == null || currentSlotIndex < 0)
            return;

        if (InventoryManager.Instance == null)
            return;

        bool used = InventoryManager.Instance.UseItem(currentCategory, currentSlotIndex);

        if (!used)
        {
            return;
        }

        // Refresh details after inventory change
        var slots = InventoryManager.Instance.GetSlots(currentCategory);
        if (currentSlotIndex >= 0 && currentSlotIndex < slots.Count)
        {
            ShowItem(currentCategory, currentSlotIndex, (InventorySlot)slots[currentSlotIndex]);
        }
        else
        {
            Clear();
        }
    }

    public void OnDiscardButtonClicked()
    {
        if (currentSlotIndex < 0 || InventoryManager.Instance == null)
            return;

        InventoryManager.Instance.ClearSlot(currentCategory, currentSlotIndex);
        Clear();
    }

    public void RefreshSelection()
    {
        if (currentSlotIndex < 0 || InventoryManager.Instance == null)
            return;

        var slots = InventoryManager.Instance.GetSlots(currentCategory);
        if (currentSlotIndex >= 0 && currentSlotIndex < slots.Count)
        {
            ShowItem(currentCategory, currentSlotIndex, (InventorySlot)slots[currentSlotIndex]);
        }
        else
        {
            Clear();
        }
    }
}

