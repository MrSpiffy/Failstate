using UnityEngine;

public class RelaySalvageCache : MonoBehaviour
{
    public string cacheName = "relay salvage cache";
    public ItemCost[] contents;
    public float interactionDistance = 3f;
    public Color availableColor = new Color(0.35f, 0.95f, 1f, 1f);
    public Color lootedColor = new Color(0.18f, 0.22f, 0.24f, 1f);

    private bool looted = false;
    private Renderer cacheRenderer;
    private MaterialPropertyBlock propertyBlock;

    void Awake()
    {
        cacheRenderer = GetComponent<Renderer>();
        ApplyColor(availableColor);
    }

    void Update()
    {
        if (looted)
        {
            return;
        }

        GameReferences refs = GameReferences.Instance;

        if (refs == null || refs.playerTransform == null || refs.playerInventory == null || refs.inputSettings == null)
        {
            return;
        }

        bool canInteract = Vector3.Distance(transform.position, refs.playerTransform.position) <= interactionDistance &&
                           UIStateManager.CanInteract();

        if (canInteract)
        {
            if (refs.interactionPromptUI != null)
            {
                refs.interactionPromptUI.ShowPrompt(
                    "Press " + refs.inputSettings.interactKey + " to search " + cacheName,
                    gameObject,
                    18
                );
            }

            if (Input.GetKeyDown(refs.inputSettings.interactKey))
            {
                Loot(refs);
            }
        }
        else if (refs.interactionPromptUI != null)
        {
            refs.interactionPromptUI.HidePrompt(gameObject);
        }
    }

    public void Configure(string displayName, ItemCost[] cacheContents)
    {
        cacheName = displayName;
        contents = cacheContents;
        gameObject.name = "RelayCache_" + displayName.Replace(" ", "_");
    }

    void Loot(GameReferences refs)
    {
        looted = true;

        string foundText = "";

        if (contents != null)
        {
            for (int i = 0; i < contents.Length; i++)
            {
                refs.playerInventory.AddItem(contents[i].itemType, contents[i].amount);

                if (i > 0)
                {
                    foundText += ", ";
                }

                foundText += contents[i].amount + " " + ItemDatabase.GetDisplayName(contents[i].itemType);
            }
        }

        FirstRunObjectiveManager objectiveManager = refs.firstRunObjectiveManager != null
            ? refs.firstRunObjectiveManager
            : FindFirstObjectByType<FirstRunObjectiveManager>();

        if (objectiveManager != null)
        {
            objectiveManager.NotifyRelaySalvageLooted(contents);
        }

        if (refs.interactionPromptUI != null)
        {
            refs.interactionPromptUI.HidePrompt(gameObject);
        }

        if (SystemMessageUI.Instance != null)
        {
            SystemMessageUI.Instance.ShowMessage("SALVAGE RECOVERED\n" + foundText, 4f);
        }

        ApplyColor(lootedColor);
        Destroy(gameObject, 0.2f);
    }

    void ApplyColor(Color color)
    {
        if (cacheRenderer == null)
        {
            cacheRenderer = GetComponent<Renderer>();
        }

        if (cacheRenderer == null)
        {
            return;
        }

        if (propertyBlock == null)
        {
            propertyBlock = new MaterialPropertyBlock();
        }

        cacheRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor("_BaseColor", color);
        propertyBlock.SetColor("_Color", color);
        cacheRenderer.SetPropertyBlock(propertyBlock);
    }
}
