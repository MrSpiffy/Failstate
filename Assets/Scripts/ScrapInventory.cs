using UnityEngine;
using TMPro;

public class ScrapInventory : MonoBehaviour
{
    public int metalScrapCount = 0;
    public int wiringCount = 0;
    public int coreFragmentCount = 0;

    public TextMeshProUGUI inventoryText;
    public PlayerCondition playerCondition;
    public InputSettings inputSettings;

    void Start()
    {
        UpdateInventoryText();
    }

    void Update()
    {
        if (!InventoryUI.IsInventoryOpen || inputSettings == null || playerCondition == null)
        {
            return;
        }

        if (Input.GetKeyDown(inputSettings.repairCoreKey))
        {
            RepairCore();
        }

        if (Input.GetKeyDown(inputSettings.repairMobilityKey))
        {
            RepairMobility();
        }

        if (Input.GetKeyDown(inputSettings.repairPerceptionKey))
        {
            RepairPerception();
        }
    }

    public void AddResource(ResourceType resourceType, int amount)
    {
        switch (resourceType)
        {
            case ResourceType.MetalScrap:
                metalScrapCount += amount;
                break;
            case ResourceType.Wiring:
                wiringCount += amount;
                break;
            case ResourceType.CoreFragment:
                coreFragmentCount += amount;
                break;
        }

        UpdateInventoryText();
        Debug.Log("Collected " + amount + " " + resourceType);
    }

    void RepairCore()
    {
        if (metalScrapCount <= 0) return;
        if (playerCondition.currentCoreIntegrity >= playerCondition.maxCoreIntegrity) return;

        metalScrapCount -= 1;
        playerCondition.RepairCore();
        UpdateInventoryText();
        Debug.Log("Used 1 Metal Scrap to repair Core.");
    }

    void RepairMobility()
    {
        if (wiringCount <= 0) return;
        if (playerCondition.currentMobilityIntegrity >= playerCondition.maxMobilityIntegrity) return;

        wiringCount -= 1;
        playerCondition.RepairMobility();
        UpdateInventoryText();
        Debug.Log("Used 1 Wiring to repair Mobility.");
    }

    void RepairPerception()
    {
        if (coreFragmentCount <= 0) return;
        if (playerCondition.currentPerceptionIntegrity >= playerCondition.maxPerceptionIntegrity) return;

        coreFragmentCount -= 1;
        playerCondition.RepairPerception();
        UpdateInventoryText();
        Debug.Log("Used 1 Core Fragment to repair Perception.");
    }

    public void UpdateInventoryText()
    {
        if (inventoryText == null) return;

        string coreText = "Unknown";
        string mobilityText = "Unknown";
        string perceptionText = "Unknown";
        string controlsText = "";

        if (playerCondition != null)
        {
            coreText = playerCondition.currentCoreIntegrity.ToString("F0") + " / " + playerCondition.maxCoreIntegrity.ToString("F0");
            mobilityText = playerCondition.currentMobilityIntegrity.ToString("F0") + " / " + playerCondition.maxMobilityIntegrity.ToString("F0");
            perceptionText = playerCondition.currentPerceptionIntegrity.ToString("F0") + " / " + playerCondition.maxPerceptionIntegrity.ToString("F0");
        }

        if (inputSettings != null)
        {
            controlsText =
                "\n\nRepair Controls:" +
                "\n" + inputSettings.repairCoreKey + " - Repair Core (Metal Scrap)" +
                "\n" + inputSettings.repairMobilityKey + " - Repair Mobility (Wiring)" +
                "\n" + inputSettings.repairPerceptionKey + " - Repair Perception (Core Fragments)";
        }

        inventoryText.text =
            "Inventory" +
            "\nMetal Scrap: " + metalScrapCount +
            "\nWiring: " + wiringCount +
            "\nCore Fragments: " + coreFragmentCount +
            "\n\nSystems" +
            "\nCore: " + coreText +
            "\nMobility: " + mobilityText +
            "\nPerception: " + perceptionText +
            controlsText;
    }
}