using UnityEngine;

public class WorkbenchStation : MonoBehaviour
{
    public float interactionDistance = 3f;

    void Update()
    {
        GameReferences refs = GameReferences.Instance;

        if (refs == null || refs.playerTransform == null || refs.inputSettings == null || refs.workbenchUI == null)
        {
            return;
        }

        float distance = Vector3.Distance(transform.position, refs.playerTransform.position);
        bool isInRange = distance <= interactionDistance;

        if (WorkbenchUI.IsWorkbenchOpen)
        {
            if (refs.interactionPromptUI != null)
            {
                refs.interactionPromptUI.HidePrompt(gameObject);
            }

            return;
        }

        bool canInteract = isInRange && UIStateManager.CanInteract();

        if (canInteract)
        {
            if (refs.interactionPromptUI != null)
            {
                refs.interactionPromptUI.ShowPrompt("Press " + refs.inputSettings.interactKey + " to use workbench", gameObject);
            }

            if (Input.GetKeyDown(refs.inputSettings.interactKey))
            {
                refs.workbenchUI.ToggleWorkbench();

                if (refs.interactionPromptUI != null)
                {
                    refs.interactionPromptUI.HidePrompt(gameObject);
                }
            }
        }
        else
        {
            if (refs.interactionPromptUI != null)
            {
                refs.interactionPromptUI.HidePrompt(gameObject);
            }
        }
    }
}