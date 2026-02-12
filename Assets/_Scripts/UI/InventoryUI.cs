using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private InventoryItemDetailsPanel detailsPanel;

    [Header("Grid")]
    [SerializeField] private RectTransform slotsParent;
    [SerializeField] private GameObject slotPrefab;

    [Header("Drag Visual")]
    [SerializeField] private Canvas rootCanvas;
    [SerializeField] private Image dragIcon;

    // UI elements for each *visible* (non-empty) slot
    private readonly List<InventorySlotUI> slotUIs = new List<InventorySlotUI>();

    // Selected logical slot index within the current category list
    private int selectedSlotIndex = -1;

    private InventoryCategory currentCategory = InventoryCategory.Consumable;

    private bool isDragging;
    private int draggingFromIndex = -1;

    private void Awake()
    {
        if (inventoryManager == null)
        {
            inventoryManager = InventoryManager.Instance;
        }
    }

    private void OnEnable()
    {
        if (inventoryManager == null)
        {
            inventoryManager = InventoryManager.Instance;
        }

        if (inventoryManager != null)
        {
            inventoryManager.OnInventoryChanged += HandleInventoryChanged;
        }

        UIInputBlocker.Push();

        HandleInventoryChanged();

        if (dragIcon != null)
        {
            dragIcon.enabled = false;
        }
    }

    private void OnDisable()
    {
        UIInputBlocker.Pop();

        if (inventoryManager != null)
        {
            inventoryManager.OnInventoryChanged -= HandleInventoryChanged;
        }
    }

    private void HandleInventoryChanged()
    {
        if (inventoryManager == null)
            return;

        // If selected slot became empty or out of range, clear selection
        var slots = inventoryManager.GetSlots(currentCategory);
        if (selectedSlotIndex >= 0)
        {
            if (selectedSlotIndex >= slots.Count ||
                ((InventorySlot)slots[selectedSlotIndex]).IsEmpty)
            {
                selectedSlotIndex = -1;
            }
        }

        // Rebuild compact list of only non-empty slots
        RebuildSlotsFromInventory();

        UpdateSelectionHighlight();

        if (inventoryManager == null || detailsPanel == null)
            return;

        if (detailsPanel != null)
        {
            detailsPanel.RefreshSelection();
        }
    }

    private void RebuildSlotsFromInventory()
    {
        if (inventoryManager == null || slotsParent == null || slotPrefab == null)
            return;

        // Clear existing UI
        for (int i = slotsParent.childCount - 1; i >= 0; i--)
        {
            Destroy(slotsParent.GetChild(i).gameObject);
        }

        slotUIs.Clear();

        var slots = inventoryManager.GetSlots(currentCategory);
        for (int i = 0; i < slots.Count; i++)
        {
            var slot = (InventorySlot)slots[i];
            if (slot == null || slot.IsEmpty || slot.item == null)
                continue; // only create UI for filled slots

            var slotGO = Instantiate(slotPrefab, slotsParent);
            var slotUI = slotGO.GetComponent<InventorySlotUI>();
            if (slotUI != null)
            {
                // Pass the *logical* index into InventoryManager.Slots
                slotUI.Initialize(this, i);
                slotUI.SetData(slot);

                bool isEquipped = EquipmentManager.Instance != null &&
                                  EquipmentManager.Instance.IsEquipped(slot.item);
                slotUI.SetEquipped(isEquipped);

                slotUIs.Add(slotUI);
            }
        }
    }

    public void OnSlotClicked(int logicalIndex)
    {
        selectedSlotIndex = logicalIndex;
        UpdateSelectionHighlight();

        if (inventoryManager == null || detailsPanel == null)
            return;

        var slots = inventoryManager.GetSlots(currentCategory);
        if (logicalIndex >= 0 && logicalIndex < slots.Count)
        {
            detailsPanel.ShowItem(currentCategory, logicalIndex, (InventorySlot)slots[logicalIndex]);
        }
    }

    public void OnSlotRightClicked(int logicalIndex)
    {
        if (inventoryManager == null)
            return;

        inventoryManager.UseItem(currentCategory, logicalIndex);
    }

    private void UpdateSelectionHighlight()
    {
        for (int i = 0; i < slotUIs.Count; i++)
        {
            var ui = slotUIs[i];
            ui.SetSelected(ui.SlotIndex == selectedSlotIndex);
        }
    }

    #region Drag & Drop

    public void BeginDrag(int logicalIndex, PointerEventData eventData)
    {
        if (inventoryManager == null)
            return;

        var slots = inventoryManager.GetSlots(currentCategory);
        if (logicalIndex < 0 || logicalIndex >= slots.Count)
            return;

        var slot = (InventorySlot)slots[logicalIndex];
        if (slot == null || slot.IsEmpty || slot.item == null)
            return;

        isDragging = true;
        draggingFromIndex = logicalIndex;

        if (dragIcon != null)
        {
            dragIcon.enabled = true;
            dragIcon.sprite = slot.item.icon;
            UpdateDragIconPosition(eventData);
        }
    }

    public void Drag(PointerEventData eventData)
    {
        if (!isDragging || dragIcon == null)
            return;

        UpdateDragIconPosition(eventData);
    }

    public void EndDrag(PointerEventData eventData)
    {
        if (!isDragging)
            return;

        isDragging = false;
        draggingFromIndex = -1;

        if (dragIcon != null)
        {
            dragIcon.enabled = false;
            dragIcon.sprite = null;
        }
    }

    public void DropOnSlot(int targetLogicalIndex, PointerEventData eventData)
    {
        if (!isDragging || inventoryManager == null)
            return;

        if (draggingFromIndex == targetLogicalIndex)
        {
            EndDrag(eventData);
            return;
        }

        // Drag & drop is only between visible entries in the current category.
        inventoryManager.MoveSlot(currentCategory, draggingFromIndex, targetLogicalIndex);
        EndDrag(eventData);
    }

    private void UpdateDragIconPosition(PointerEventData eventData)
    {
        if (rootCanvas == null || dragIcon == null)
            return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.transform as RectTransform,
            eventData.position,
            rootCanvas.worldCamera,
            out var localPoint);

        dragIcon.rectTransform.localPosition = localPoint;
    }

    #endregion

    // Tab switching – hook these to UI buttons in the inspector
    public void ShowConsumablesTab()
    {
        currentCategory = InventoryCategory.Consumable;
        selectedSlotIndex = -1;
        if (detailsPanel != null)
        {
            detailsPanel.Clear();
        }
        HandleInventoryChanged();
    }

    public void ShowEquipmentTab()
    {
        currentCategory = InventoryCategory.Equipment;
        selectedSlotIndex = -1;
        if (detailsPanel != null)
        {
            detailsPanel.Clear();
        }
        HandleInventoryChanged();
    }
}

