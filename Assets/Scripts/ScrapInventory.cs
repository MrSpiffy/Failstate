using UnityEngine;
using TMPro;

public class ScrapInventory : MonoBehaviour
{
    public int metalScrapCount = 0;
    public int wiringCount = 0;
    public int coreFragmentCount = 0;
    public int repairKitCount = 0;
    public int mobilityPatchCount = 0;
    public int sensorPatchCount = 0;

    public TextMeshProUGUI inventoryText;
    public PlayerCondition playerCondition;
    public InputSettings inputSettings;

    public bool CanAfford(int metalCost, int wiringCost, int coreFragmentCost)
{
    return metalScrapCount >= metalCost &&
           wiringCount >= wiringCost &&
           coreFragmentCount >= coreFragmentCost;
}

public bool SpendResources(int metalCost, int wiringCost, int coreFragmentCost)
{
    if (!CanAfford(metalCost, wiringCost, coreFragmentCost))
    {
        return false;
    }

    metalScrapCount -= metalCost;
    wiringCount -= wiringCost;
    coreFragmentCount -= coreFragmentCost;
    UpdateInventoryText();

    return true;
}

    public bool TryCraftRepairKit()
{
    if (metalScrapCount < 2 || wiringCount < 1)
    {
        Debug.Log("Not enough resources to craft Repair Kit.");
        return false;
    }

    metalScrapCount -= 2;
    wiringCount -= 1;
    repairKitCount += 1;

    UpdateInventoryText();
    Debug.Log("Crafted 1 Repair Kit.");

    return true;
}

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
            "\nRepair Kits: " + repairKitCount +
            "\nMobility Patches: " + mobilityPatchCount +
            "\nSensor Patches: " + sensorPatchCount +
            "\n\nSystems" +
            "\nCore: " + coreText +
            "\nMobility: " + mobilityText +
            "\nPerception: " + perceptionText +
            controlsText;
    }
}