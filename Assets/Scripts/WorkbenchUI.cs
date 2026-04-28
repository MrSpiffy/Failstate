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
    public PlayerInventory playerInventory;

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

        CraftingRecipe[] recipes = CraftingRecipeDatabase.GetAllRecipes();

        for (int i = 0; i < recipes.Length; i++)
        {
            allRecipes.Add(recipes[i]);
        }
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
        if (recipeText == null || playerInventory == null) return;

        visibleRecipes.Clear();

        string search = "";

        if (searchInputField != null)
        {
            search = searchInputField.text.ToLower().Trim();
            lastSearchText = searchInputField.text;
        }

        foreach (CraftingRecipe recipe in allRecipes)
        {
            string recipeName = ItemDatabase.GetDisplayName(recipe.outputItem).ToLower();

            if (string.IsNullOrWhiteSpace(search) || recipeName.Contains(search))
            {
                visibleRecipes.Add(recipe);
            }
        }

        string output = "";

        for (int i = 0; i < visibleRecipes.Count; i++)
        {
            CraftingRecipe recipe = visibleRecipes[i];
            bool canCraft = playerInventory.CanAfford(recipe.costs);

            string color = canCraft ? "#FFFFFF" : "#777777";
            string status = canCraft ? "READY" : "MISSING RESOURCES";

            output +=
                "<color=" + color + ">" +
                (i + 1) + ". " + ItemDatabase.GetDisplayName(recipe.outputItem) + " [" + status + "]\n" +
                "   Cost: " + recipe.GetCostText() + "\n" +
                "   " + ItemDatabase.GetDescription(recipe.outputItem) + "\n\n" +
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
                "Resources — Metal: " + playerInventory.GetItemCount(ItemType.MetalScrap) +
                " | Wiring: " + playerInventory.GetItemCount(ItemType.Wiring) +
                " | Core Fragments: " + playerInventory.GetItemCount(ItemType.CoreFragment);
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
        if (index < 0 || index >= visibleRecipes.Count || playerInventory == null) return;

        CraftingRecipe recipe = visibleRecipes[index];

        if (!playerInventory.CanAfford(recipe.costs))
        {
            Debug.Log("Cannot craft " + ItemDatabase.GetDisplayName(recipe.outputItem) + ": missing resources.");
            UpdateRecipeDisplay();
            return;
        }

        if (playerInventory.SpendItems(recipe.costs))
        {
            playerInventory.AddItem(recipe.outputItem, recipe.outputAmount);
            Debug.Log("Crafted " + recipe.outputAmount + " " + ItemDatabase.GetDisplayName(recipe.outputItem));
        }

        UpdateRecipeDisplay();
    }
}