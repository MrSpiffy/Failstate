using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class WorkbenchUI : MonoBehaviour
{
    public static bool IsWorkbenchOpen { get; private set; } = false;

    public UIStateManager uiStateManager;

    [Header("UI")]
    public GameObject workbenchPanel;
    public TMP_InputField searchInputField;
    public TextMeshProUGUI recipeText;
    public TextMeshProUGUI infoText;
    public ScrollRect recipeScrollRect;

    [Header("References")]
    public ScrapInventory scrapInventory;

    private readonly List<CraftingRecipe> allRecipes = new List<CraftingRecipe>();
    private readonly List<CraftingRecipe> visibleRecipes = new List<CraftingRecipe>();

    private string lastSearchText = "";

    void Awake()
    {
        BuildRecipeList();
    }

    void Update()
    {
        if (!IsWorkbenchOpen) return;

        if (searchInputField != null && searchInputField.text != lastSearchText)
        {
            UpdateRecipeDisplay();
        }

        HandleRecipeHotkeys();
    }

    void BuildRecipeList()
    {
        allRecipes.Clear();

        allRecipes.Add(new CraftingRecipe(
            "Repair Kit",
            "A compact emergency repair item. Later this can restore Core integrity in the field.",
            2, 1, 0,
            CraftRepairKit
        ));

        allRecipes.Add(new CraftingRecipe(
            "Mobility Patch",
            "A temporary repair component for movement systems.",
            0, 2, 0,
            CraftMobilityPatch
        ));

        allRecipes.Add(new CraftingRecipe(
            "Sensor Patch",
            "A temporary repair component for damaged perception systems.",
            0, 0, 2,
            CraftSensorPatch
        ));
    }

    public void ToggleWorkbench()
    {
        if (IsWorkbenchOpen)
        {
            CloseWorkbench();
        }
        else
        {
            OpenWorkbench();
        }
    }

    public void OpenWorkbench()
    {
        IsWorkbenchOpen = true;

        if (workbenchPanel != null)
        {
            workbenchPanel.SetActive(true);
        }

        if (uiStateManager != null)
        {
            uiStateManager.SetState(UIState.Workbench);
        }

        if (searchInputField != null)
        {
            searchInputField.text = "";
        }

        UpdateRecipeDisplay();
    }

    public void CloseWorkbench()
    {
        if (!IsWorkbenchOpen) return;

        IsWorkbenchOpen = false;

        if (workbenchPanel != null)
        {
            workbenchPanel.SetActive(false);
        }

        if (uiStateManager != null)
        {
            uiStateManager.ReturnToGameplay();
        }
    }

    void UpdateRecipeDisplay()
    {
        if (recipeText == null || scrapInventory == null) return;

        visibleRecipes.Clear();

        string search = "";

        if (searchInputField != null)
        {
            search = searchInputField.text.ToLower().Trim();
            lastSearchText = searchInputField.text;
        }

        foreach (CraftingRecipe recipe in allRecipes)
        {
            if (string.IsNullOrWhiteSpace(search) || recipe.name.ToLower().Contains(search))
            {
                visibleRecipes.Add(recipe);
            }
        }

        string output = "";

        for (int i = 0; i < visibleRecipes.Count; i++)
        {
            CraftingRecipe recipe = visibleRecipes[i];
            bool canCraft = recipe.CanCraft(scrapInventory);

            string color = canCraft ? "#FFFFFF" : "#777777";
            string status = canCraft ? "READY" : "MISSING RESOURCES";

            output +=
                "<color=" + color + ">" +
                (i + 1) + ". " + recipe.name + " [" + status + "]\n" +
                "   Cost: " + recipe.GetCostText() + "\n" +
                "   " + recipe.description + "\n\n" +
                "</color>";
        }

        if (string.IsNullOrWhiteSpace(output))
        {
            output = "<color=#999999>No recipes found.</color>";
        }

        recipeText.text = output;

        if (infoText != null)
        {
            infoText.text =
                "Press 1-9 to craft a visible recipe.\n" +
                "Resources — Metal: " + scrapInventory.metalScrapCount +
                " | Wiring: " + scrapInventory.wiringCount +
                " | Core Fragments: " + scrapInventory.coreFragmentCount;
        }

        StartCoroutine(ScrollRecipesToTopNextFrame());
    }

    IEnumerator ScrollRecipesToTopNextFrame()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();

        if (recipeScrollRect != null)
        {
            recipeScrollRect.verticalNormalizedPosition = 1f;
        }
    }

    void HandleRecipeHotkeys()
    {
        for (int i = 0; i < visibleRecipes.Count && i < 9; i++)
        {
            KeyCode key = KeyCode.Alpha1 + i;

            if (Input.GetKeyDown(key))
            {
                TryCraftVisibleRecipe(i);
                return;
            }
        }
    }

    void TryCraftVisibleRecipe(int index)
    {
        if (index < 0 || index >= visibleRecipes.Count || scrapInventory == null) return;

        CraftingRecipe recipe = visibleRecipes[index];

        if (!recipe.CanCraft(scrapInventory))
        {
            Debug.Log("Cannot craft " + recipe.name + ": missing resources.");
            UpdateRecipeDisplay();
            return;
        }

        recipe.CraftAction.Invoke();
        UpdateRecipeDisplay();
    }

    void CraftRepairKit()
    {
        if (scrapInventory.SpendResources(2, 1, 0))
        {
            scrapInventory.repairKitCount += 1;
            scrapInventory.UpdateInventoryText();
            Debug.Log("Crafted Repair Kit.");
        }
    }

    void CraftMobilityPatch()
    {
        if (scrapInventory.SpendResources(0, 2, 0))
        {
            scrapInventory.mobilityPatchCount += 1;
            scrapInventory.UpdateInventoryText();
            Debug.Log("Crafted Mobility Patch.");
        }
    }

    void CraftSensorPatch()
    {
        if (scrapInventory.SpendResources(0, 0, 2))
        {
            scrapInventory.sensorPatchCount += 1;
            scrapInventory.UpdateInventoryText();
            Debug.Log("Crafted Sensor Patch.");
        }
    }

    private class CraftingRecipe
    {
        public string name;
        public string description;
        public int metalCost;
        public int wiringCost;
        public int coreFragmentCost;
        public System.Action CraftAction;

        public CraftingRecipe(string name, string description, int metalCost, int wiringCost, int coreFragmentCost, System.Action craftAction)
        {
            this.name = name;
            this.description = description;
            this.metalCost = metalCost;
            this.wiringCost = wiringCost;
            this.coreFragmentCost = coreFragmentCost;
            CraftAction = craftAction;
        }

        public bool CanCraft(ScrapInventory inventory)
        {
            return inventory.CanAfford(metalCost, wiringCost, coreFragmentCost);
        }

        public string GetCostText()
        {
            List<string> costs = new List<string>();

            if (metalCost > 0) costs.Add(metalCost + " Metal Scrap");
            if (wiringCost > 0) costs.Add(wiringCost + " Wiring");
            if (coreFragmentCost > 0) costs.Add(coreFragmentCost + " Core Fragments");

            return costs.Count == 0 ? "Free" : string.Join(" + ", costs);
        }
    }
}