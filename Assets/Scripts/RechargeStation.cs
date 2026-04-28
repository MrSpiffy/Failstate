using UnityEngine;

public class RechargeStation : MonoBehaviour
{
    public float interactionDistance = 3f;

    void Update()
    {
        GameReferences refs = GameReferences.Instance;

        if (refs == null || refs.playerTransform == null || refs.playerCondition == null || refs.inputSettings == null)
        {
            return;
        }

        float distance = Vector3.Distance(transform.position, refs.playerTransform.position);
        bool isInRange = distance <= interactionDistance;
        bool canInteract = isInRange && UIStateManager.CurrentState == UIState.Gameplay;

        if (!canInteract)
        {
            if (refs.interactionPromptUI != null)
            {
                refs.interactionPromptUI.HidePrompt(gameObject);
            }

            return;
        }

        if (refs.interactionPromptUI != null)
        {
            refs.interactionPromptUI.ShowPrompt("Press " + refs.inputSettings.interactKey + " to recharge", gameObject);
        }

        if (Input.GetKeyDown(refs.inputSettings.interactKey))
        {
            refs.playerCondition.FullyRestoreAllSystems();
            Debug.Log("Player fully recharged.");
        }
    }
}