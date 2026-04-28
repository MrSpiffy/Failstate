using UnityEngine;

public class ScrapPickup : MonoBehaviour
{
    public ItemType itemType = ItemType.MetalScrap;
    public int itemAmount = 1;
    public float interactionDistance = 3f;

    public Material normalMaterial;
    public Material highlightMaterial;

    private Renderer objectRenderer;

    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        SetNormalMaterial();
    }

    void Update()
    {
        GameReferences refs = GameReferences.Instance;

        if (refs == null || refs.playerTransform == null || refs.playerInventory == null || refs.inputSettings == null)
        {
            return;
        }

        float distance = Vector3.Distance(transform.position, refs.playerTransform.position);
        bool isInRange = distance <= interactionDistance;
        bool canInteract = isInRange && UIStateManager.CanInteract();

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
            if (refs.interactionPromptUI != null)
            {
                refs.interactionPromptUI.ShowPrompt(
                    "Press " + refs.inputSettings.interactKey + " to collect " + ItemDatabase.GetDisplayName(itemType),
                    gameObject
                );
            }

            if (Input.GetKeyDown(refs.inputSettings.interactKey))
            {
                refs.playerInventory.AddItem(itemType, itemAmount);

                if (refs.interactionPromptUI != null)
                {
                    refs.interactionPromptUI.HidePrompt(gameObject);
                }

                Destroy(gameObject);
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