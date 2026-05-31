using System.Collections;
using UnityEngine;

public class RechargeStation : MonoBehaviour
{
    public float interactionDistance = 3f;

    private bool isRecharging = false;

    void Update()
    {
        if (isRecharging)
        {
            return;
        }

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
            refs.interactionPromptUI.ShowPrompt(
                "Press " + refs.inputSettings.interactKey + " to rest and recharge",
                gameObject,
                20
            );
        }

        if (Input.GetKeyDown(refs.inputSettings.interactKey))
        {
            StartCoroutine(RestAtStation(refs));
        }
    }

    IEnumerator RestAtStation(GameReferences refs)
    {
        isRecharging = true;

        if (refs.interactionPromptUI != null)
        {
            refs.interactionPromptUI.HidePrompt(gameObject);
        }

        UIStateManager uiStateManager = FindFirstObjectByType<UIStateManager>();

        if (uiStateManager != null)
        {
            uiStateManager.SetState(UIState.Pause);
        }

        if (refs.restUI != null)
        {
            yield return refs.restUI.PlayRestSequence(refs.playerCondition);
        }
        else
        {
            refs.playerCondition.FullyRestoreAllSystems();
        }

        if (uiStateManager != null)
        {
            uiStateManager.ReturnToGameplay();
        }

        isRecharging = false;
    }
}
