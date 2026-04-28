using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public static bool IsInventoryOpen { get; private set; } = false;

    public GameObject inventoryPanel;
    public PlayerInventory playerInventory;
    public InputSettings inputSettings;
    public UIStateManager uiStateManager;
    public PlayerCondition playerCondition;

    [Header("Inventory Grid")]
    public Transform itemSlotParent;
    public GameObject itemSlotPrefab;

    [Header("Side Panels")]
    public TextMeshProUGUI systemStatusText;
    public TextMeshProUGUI itemDetailsText;

    private readonly List<GameObject> spawnedSlots = new List<GameObject>();
    private ItemType? selectedItemType = null;
    private string lastFeedbackMessage = "";

    void OnEnable()
    {
        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged += HandleInventoryChanged;
        }
    }

    void OnDisable()
    {
        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged -= HandleInventoryChanged;
        }
    }

    void HandleInventoryChanged()
    {
        if (!IsInventoryOpen)
        {
            return;
        }

        RefreshInventoryGrid();
        UpdateItemDetailsText();
    }

    void Update()
    {
        if (inputSettings == null) return;

        if (UIStateManager.CurrentState == UIState.Gameplay && Input.GetKeyDown(inputSettings.inventoryKey))
        {
            OpenInventory();
            return;
        }

        if (IsInventoryOpen && Input.GetKeyDown(inputSettings.inventoryKey))
        {
            CloseInventory();
            return;
        }

        if (IsInventoryOpen)
        {
            UpdateSystemStatusText();

            if (Input.GetKeyDown(inputSettings.interactKey))
            {
                UseSelectedItem();
            }
        }
    }

    public void OpenInventory()
    {
        IsInventoryOpen = true;

        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(true);
        }

        selectedItemType = null;
        lastFeedbackMessage = "";

        RefreshInventoryGrid();
        UpdateSystemStatusText();
        UpdateItemDetailsText();

        if (uiStateManager != null)
        {
            uiStateManager.SetState(UIState.Inventory);
        }
    }

    public void CloseInventory()
    {
        IsInventoryOpen = false;

        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }

        if (uiStateManager != null)
        {
            uiStateManager.ReturnToGameplay();
        }
    }

    public void SelectItem(ItemType itemType)
    {
        selectedItemType = itemType;
        lastFeedbackMessage = "";

        UpdateSlotSelectionVisuals();
        UpdateItemDetailsText();
    }

    public void RefreshInventoryGrid()
    {
        if (playerInventory == null || itemSlotParent == null || itemSlotPrefab == null)
        {
            return;
        }

        ClearSlots();

        foreach (ItemType itemType in ItemDatabase.GetAllItemTypes())
        {
            AddSlot(itemType, playerInventory.GetItemCount(itemType));
        }

        UpdateSlotSelectionVisuals();
    }

    void AddSlot(ItemType itemType, int count)
    {
        if (count <= 0) return;

        GameObject slotObject = Instantiate(itemSlotPrefab, itemSlotParent);
        spawnedSlots.Add(slotObject);

        InventoryItemSlotUI slotUI = slotObject.GetComponent<InventoryItemSlotUI>();

        if (slotUI != null)
        {
            slotUI.SetItem(itemType, count, this);
        }
    }

    void ClearSlots()
    {
        for (int i = 0; i < spawnedSlots.Count; i++)
        {
            if (spawnedSlots[i] != null)
            {
                Destroy(spawnedSlots[i]);
            }
        }

        spawnedSlots.Clear();
    }

    void UpdateSlotSelectionVisuals()
    {
        for (int i = 0; i < spawnedSlots.Count; i++)
        {
            if (spawnedSlots[i] == null) continue;

            InventoryItemSlotUI slotUI = spawnedSlots[i].GetComponent<InventoryItemSlotUI>();

            if (slotUI != null)
            {
                slotUI.SetSelected(selectedItemType.HasValue && slotUI.GetItemType() == selectedItemType.Value);
            }
        }
    }

    void UpdateSystemStatusText()
    {
        if (systemStatusText == null || playerCondition == null) return;

        systemStatusText.text =
            "Systems\n\n" +
            "Core\n" +
            playerCondition.currentCoreIntegrity.ToString("F0") + " / " + playerCondition.maxCoreIntegrity.ToString("F0") +
            "\n\nMobility\n" +
            playerCondition.currentMobilityIntegrity.ToString("F0") + " / " + playerCondition.maxMobilityIntegrity.ToString("F0") +
            "\n\nPerception\n" +
            playerCondition.currentPerceptionIntegrity.ToString("F0") + " / " + playerCondition.maxPerceptionIntegrity.ToString("F0");
    }

    void UpdateItemDetailsText()
    {
        if (itemDetailsText == null) return;

        if (!selectedItemType.HasValue)
        {
            itemDetailsText.text =
                "Item Details\n\n" +
                "Select an item to view details.";
            return;
        }

        ItemType itemType = selectedItemType.Value;

        itemDetailsText.text =
            ItemDatabase.GetDisplayName(itemType) +
            "\n\n" +
            ItemDatabase.GetDescription(itemType);

        if (!string.IsNullOrWhiteSpace(lastFeedbackMessage))
        {
            itemDetailsText.text += "\n\n" + lastFeedbackMessage;
        }
    }

    void UseSelectedItem()
    {
        if (!selectedItemType.HasValue)
        {
            lastFeedbackMessage = "No item selected.";
            UpdateItemDetailsText();
            return;
        }

        bool usedItem = ItemUseSystem.TryUseItem(
            selectedItemType.Value,
            playerInventory,
            playerCondition,
            out lastFeedbackMessage
        );

        UpdateSystemStatusText();
        UpdateItemDetailsText();
    }
}