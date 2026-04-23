using UnityEngine;

public class RechargeStation : MonoBehaviour
{
    public float interactionDistance = 3f;

    private Transform player;
    private PlayerCondition playerCondition;
    private InputSettings inputSettings;
    private ScrapInventory scrapInventory;
    private InteractionPromptUI promptUI;

    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
            playerCondition = playerObject.GetComponent<PlayerCondition>();
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
    }

    void Update()
    {
        if (player == null || playerCondition == null || inputSettings == null)
        {
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);
        bool isInRange = distance <= interactionDistance;
        bool canInteract = isInRange && !InventoryUI.IsInventoryOpen && !PauseMenuUI.IsPauseMenuOpen && !DevConsoleUI.IsConsoleOpen;

        if (canInteract)
        {
            if (promptUI != null)
            {
                promptUI.ShowPrompt("Press " + inputSettings.interactKey + " to recharge", gameObject);
            }

            if (Input.GetKeyDown(inputSettings.interactKey))
            {
                playerCondition.FullyRestoreAllSystems();

                if (scrapInventory != null)
                {
                    scrapInventory.UpdateInventoryText();
                }

                Debug.Log("Player fully recharged.");
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
}