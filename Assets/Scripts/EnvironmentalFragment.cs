using UnityEngine;

public class EnvironmentalFragment : MonoBehaviour
{
    public int fragmentIndex;
    public string fragmentTitle = "CORRUPTED TRACE";
    [TextArea] public string fragmentText = "Data unreadable.";
    public float interactionDistance = 3.2f;
    public bool recovered = false;

    public void Configure(int index, string title, string text)
    {
        fragmentIndex = index;
        fragmentTitle = title;
        fragmentText = text;
    }

    void Update()
    {
        GameReferences refs = GameReferences.Instance;

        if (recovered || refs == null || refs.playerTransform == null || refs.inputSettings == null)
        {
            return;
        }

        bool inRange = Vector3.Distance(transform.position, refs.playerTransform.position) <= interactionDistance;

        if (!inRange || !UIStateManager.CanInteract())
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
                "Press " + refs.inputSettings.interactKey + " to recover data trace: " + fragmentTitle,
                gameObject,
                16
            );
        }

        if (Input.GetKeyDown(refs.inputSettings.interactKey))
        {
            Recover(refs);
        }
    }

    public bool Recover(GameReferences refs)
    {
        if (recovered)
        {
            return false;
        }

        recovered = true;

        if (refs != null && refs.interactionPromptUI != null)
        {
            refs.interactionPromptUI.HidePrompt(gameObject);
        }

        FirstRunObjectiveManager objectiveManager = FindFirstObjectByType<FirstRunObjectiveManager>();

        if (objectiveManager != null)
        {
            objectiveManager.RegisterEnvironmentalFragment(fragmentIndex);
        }

        if (SystemMessageUI.Instance != null)
        {
            SystemMessageUI.Instance.ShowMessage(
                "ARCHIVE TRACE " + fragmentIndex + "/3 // " + fragmentTitle + "\n" +
                fragmentText + "\n" +
                "Root terminal can attempt correlation.",
                8f
            );
        }

        Renderer renderer = GetComponent<Renderer>();

        if (renderer != null)
        {
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            Color archivedColor = new Color(0.1f, 0.28f, 0.3f, 1f);
            block.SetColor("_BaseColor", archivedColor);
            block.SetColor("_Color", archivedColor);
            renderer.SetPropertyBlock(block);
        }

        return true;
    }
}
