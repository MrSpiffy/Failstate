using UnityEngine;

public class ScrapPickup : MonoBehaviour
{
    public ResourceType resourceType = ResourceType.MetalScrap;
    public int resourceAmount = 1;
    public float interactionDistance = 3f;

    public Material normalMaterial;
    public Material highlightMaterial;

    private Transform player;
    private ScrapInventory scrapInventory;
    private InputSettings inputSettings;
    private Renderer objectRenderer;
    private InteractionPromptUI promptUI;

    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
            inputSettings = playerObject.GetComponent<InputSettings>();
        }

        GameObject gameManager = GameObject.Find("GameManager");

        if (gameManager != null)
        {
            scrapInventory = gameManager.GetComponent<ScrapInventory>();
        }

        GameObject uiManager = GameObject.Find("UIManager");

        if (uiManager != null)
        {
            promptUI = uiManager.GetComponent<InteractionPromptUI>();
        }

        objectRenderer = GetComponent<Renderer>();
        SetNormalMaterial();
    }

    void Update()
    {
        if (player == null || scrapInventory == null || inputSettings == null)
        {
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);
        bool isInRange = distance <= interactionDistance;
        bool canInteract = isInRange && !InventoryUI.IsInventoryOpen && !PauseMenuUI.IsPauseMenuOpen && !DevConsoleUI.IsConsoleOpen;

        if (isInRange)
        {
            SetHighlightMaterial();
        }
        else
        {
            SetNormalMaterial();
        }

        if (canInteract)
        {
            if (promptUI != null)
            {
                promptUI.ShowPrompt("Press " + inputSettings.interactKey + " to collect " + resourceType, gameObject);
            }

            if (Input.GetKeyDown(inputSettings.interactKey))
            {
                scrapInventory.AddResource(resourceType, resourceAmount);

                if (promptUI != null)
                {
                    promptUI.HidePrompt(gameObject);
                }

                Destroy(gameObject);
            }
        }
        else
        {
            if (promptUI != null)
            {
                promptUI.HidePrompt(gameObject);
            }
        }
    }

    void SetNormalMaterial()
    {
        if (objectRenderer != null && normalMaterial != null)
        {
            objectRenderer.material = normalMaterial;
        }
    }

    void SetHighlightMaterial()
    {
        if (objectRenderer != null && highlightMaterial != null)
        {
            objectRenderer.material = highlightMaterial;
        }
    }
}