using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class WorkbenchStation : MonoBehaviour
{
    public float interactionDistance = 3f;

    private Transform player;
    private InputSettings inputSettings;
    private InteractionPromptUI promptUI;
    private WorkbenchUI workbenchUI;

    bool IsTypingInInputField()
{
    return UIStateManager.IsTypingInInputField();
}
    
    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
            inputSettings = playerObject.GetComponent<InputSettings>();
        }

        GameObject uiManager = GameObject.Find("UIManager");

        if (uiManager != null)
        {
            promptUI = uiManager.GetComponent<InteractionPromptUI>();
            workbenchUI = uiManager.GetComponent<WorkbenchUI>();
        }
    }

    void Update()
    {
        if (player == null || inputSettings == null || workbenchUI == null)
        {
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);
        bool isInRange = distance <= interactionDistance;

        // If workbench is open, only handle closing
        if (WorkbenchUI.IsWorkbenchOpen)
        {
            if (promptUI != null)
            {
                promptUI.HidePrompt(gameObject);
            }

            if (Input.GetKeyDown(inputSettings.interactKey) && !IsTypingInInputField())
{
    workbenchUI.CloseWorkbench();
}

            return;
        }

        bool canInteract = isInRange && UIStateManager.CanInteract();

        if (canInteract)
        {
            if (promptUI != null)
            {
                promptUI.ShowPrompt("Press " + inputSettings.interactKey + " to use workbench", gameObject);
            }

            if (Input.GetKeyDown(inputSettings.interactKey))
            {
                workbenchUI.ToggleWorkbench();

                if (promptUI != null)
                {
                    promptUI.HidePrompt(gameObject);
                }
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