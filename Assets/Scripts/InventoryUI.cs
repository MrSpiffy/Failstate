using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public static bool IsInventoryOpen { get; private set; } = false;

    public GameObject inventoryPanel;
    public ScrapInventory scrapInventory;
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
    private string selectedItemName = "";

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

        selectedItemName = "";
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

    public void SelectItem(string itemName)
    {
        selectedItemName = itemName;
        UpdateItemDetailsText();
    }

    public void RefreshInventoryGrid()
    {
        if (scrapInventory == null || itemSlotParent == null || itemSlotPrefab == null)
        {
            return;
        }

        ClearSlots();

        AddSlot("Metal Scrap", scrapInventory.metalScrapCount);
        AddSlot("Wiring", scrapInventory.wiringCount);
        AddSlot("Core Fragments", scrapInventory.coreFragmentCount);
        AddSlot("Repair Kits", scrapInventory.repairKitCount);
        AddSlot("Mobility Patches", scrapInventory.mobilityPatchCount);
        AddSlot("Sensor Patches", scrapInventory.sensorPatchCount);
    }

    void AddSlot(string itemName, int count)
    {
        if (count <= 0) return;

        GameObject slotObject = Instantiate(itemSlotPrefab, itemSlotParent);
        spawnedSlots.Add(slotObject);

        InventoryItemSlotUI slotUI = slotObject.GetComponent<InventoryItemSlotUI>();

        if (slotUI != null)
        {
            slotUI.SetItem(itemName, count, this);
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

        if (string.IsNullOrWhiteSpace(selectedItemName))
        {
            itemDetailsText.text =
                "Item Details\n\n" +
                "Select an item to view details.";
            return;
        }

        switch (selectedItemName)
        {
            case "Metal Scrap":
                itemDetailsText.text =
                    "Metal Scrap\n\n" +
                    "Basic structural material.\n\n" +
                    "Used for crafting and core repairs.\n\n" +
                    "Found while scavenging.";
                break;

            case "Wiring":
                itemDetailsText.text =
                    "Wiring\n\n" +
                    "Useful for mobility and electrical repair.\n\n" +
                    "Used in mobility-related crafting.\n\n" +
                    "Found in ruined machines.";
                break;

            case "Core Fragments":
                itemDetailsText.text =
                    "Core Fragments\n\n" +
                    "Rare sensor material.\n\n" +
                    "Used for perception-related crafting.\n\n" +
                    "Found in advanced wreckage.";
                break;

            case "Repair Kits":
                itemDetailsText.text =
                    "Repair Kit\n\n" +
                    "Restores Core integrity by 25.\n\n" +
                    "Press E to use.";
                break;

            case "Mobility Patches":
                itemDetailsText.text =
                    "Mobility Patch\n\n" +
                    "Restores Mobility integrity by 25.\n\n" +
                    "Press E to use.";
                break;

            case "Sensor Patches":
                itemDetailsText.text =
                    "Sensor Patch\n\n" +
                    "Restores Perception integrity by 25.\n\n" +
                    "Press E to use.";
                break;

            default:
                itemDetailsText.text = selectedItemName;
                break;
        }
    }

    void UseSelectedItem()
    {
        if (scrapInventory == null || playerCondition == null) return;

        bool usedItem = false;

        switch (selectedItemName)
        {
            case "Repair Kits":
                if (scrapInventory.repairKitCount > 0 && playerCondition.currentCoreIntegrity < playerCondition.maxCoreIntegrity)
                {
                    scrapInventory.repairKitCount--;
                    playerCondition.currentCoreIntegrity = Mathf.Min(playerCondition.maxCoreIntegrity, playerCondition.currentCoreIntegrity + 25f);
                    usedItem = true;
                }
                break;

            case "Mobility Patches":
                if (scrapInventory.mobilityPatchCount > 0 && playerCondition.currentMobilityIntegrity < playerCondition.maxMobilityIntegrity)
                {
                    scrapInventory.mobilityPatchCount--;
                    playerCondition.currentMobilityIntegrity = Mathf.Min(playerCondition.maxMobilityIntegrity, playerCondition.currentMobilityIntegrity + 25f);
                    usedItem = true;
                }
                break;

            case "Sensor Patches":
                if (scrapInventory.sensorPatchCount > 0 && playerCondition.currentPerceptionIntegrity < playerCondition.maxPerceptionIntegrity)
                {
                    scrapInventory.sensorPatchCount--;
                    playerCondition.currentPerceptionIntegrity = Mathf.Min(playerCondition.maxPerceptionIntegrity, playerCondition.currentPerceptionIntegrity + 25f);
                    usedItem = true;
                }
                break;
        }

        if (usedItem)
        {
            RefreshInventoryGrid();
            UpdateSystemStatusText();
            UpdateItemDetailsText();
        }
    }
}