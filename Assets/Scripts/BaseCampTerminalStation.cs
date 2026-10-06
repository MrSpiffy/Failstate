using UnityEngine;

public class BaseCampTerminalStation : MonoBehaviour
{
    public BaseCampZone baseCampZone;
    public float interactionDistance = 3f;
    private int nextArchivePage = 0;

    void Update()
    {
        GameReferences refs = GameReferences.Instance;

        if (baseCampZone == null || !baseCampZone.rootRelayOnline || refs == null || refs.playerTransform == null || refs.inputSettings == null)
        {
            return;
        }

        bool canInteract = Vector3.Distance(transform.position, refs.playerTransform.position) <= interactionDistance &&
            UIStateManager.CanInteract();

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
                "Press " + refs.inputSettings.interactKey + " to read root archive terminal",
                gameObject,
                18
            );
        }

        if (!Input.GetKeyDown(refs.inputSettings.interactKey))
        {
            return;
        }

        FirstRunObjectiveManager objective = FindFirstObjectByType<FirstRunObjectiveManager>();
        string archivePage = objective != null
            ? objective.GetArchivePage(nextArchivePage)
            : "ROOT ARCHIVE TERMINAL\nArchive unavailable.";
        int pageCount = objective != null ? objective.GetArchivePageCount() : 1;
        int shownPage = Mathf.Clamp(nextArchivePage, 0, pageCount - 1) + 1;
        nextArchivePage = (nextArchivePage + 1) % Mathf.Max(1, pageCount);

        if (SystemMessageUI.Instance != null)
        {
            SystemMessageUI.Instance.ShowMessage(
                archivePage + "\nRECORD " + shownPage + "/" + pageCount,
                7f
            );
        }
    }
}
