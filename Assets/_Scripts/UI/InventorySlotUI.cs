using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [Header("UI References")]
    [SerializeField] private Image icon;
    [SerializeField] private GameObject selectionHighlight;
    [SerializeField] private TextMeshProUGUI equippedLabel;

    // Logical index into InventoryManager.Slots
    private int slotIndex;
    private InventoryUI inventoryUI;

    public int SlotIndex => slotIndex;

    public void Initialize(InventoryUI ui, int index)
    {
        inventoryUI = ui;
        slotIndex = index;
        SetSelected(false);
    }

    public void SetData(InventorySlot slot)
    {
        if (slot == null || slot.IsEmpty || slot.item == null)
        {
            if (icon != null)
            {
                icon.enabled = false;
                icon.sprite = null;
            }

            return;
        }

        if (icon != null)
        {
            icon.enabled = true;
            icon.sprite = slot.item.icon;
        }
    }

    public void SetEquipped(bool equipped)
    {
        if (equippedLabel == null)
            return;

        equippedLabel.gameObject.SetActive(equipped);
        equippedLabel.text = equipped ? "E" : string.Empty;
    }

    public void SetSelected(bool selected)
    {
        if (selectionHighlight != null)
        {
            selectionHighlight.SetActive(selected);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (inventoryUI == null) return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            inventoryUI.OnSlotClicked(slotIndex);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            inventoryUI.OnSlotRightClicked(slotIndex);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (inventoryUI == null) return;
        inventoryUI.BeginDrag(slotIndex, eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (inventoryUI == null) return;
        inventoryUI.Drag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (inventoryUI == null) return;
        inventoryUI.EndDrag(eventData);
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (inventoryUI == null) return;
        inventoryUI.DropOnSlot(slotIndex, eventData);
    }
}

