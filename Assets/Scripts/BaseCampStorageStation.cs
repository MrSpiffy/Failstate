using UnityEngine;

public class BaseCampStorageStation : MonoBehaviour
{
    public BaseCampZone baseCampZone;
    public float interactionDistance = 3f;

    void Update()
    {
        GameReferences refs = GameReferences.Instance;

        if (baseCampZone == null || refs == null || refs.playerTransform == null || refs.playerInventory == null || refs.inputSettings == null)
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

        bool carriesSalvage = HasBasicSalvage(refs.playerInventory);
        string prompt = carriesSalvage
            ? "Press " + refs.inputSettings.interactKey + " to store carried salvage"
            : baseCampZone.HasStoredSalvage()
                ? "Press " + refs.inputSettings.interactKey + " to withdraw stored salvage"
                : "Storage empty - recover salvage in the local grid";

        if (refs.interactionPromptUI != null)
        {
            refs.interactionPromptUI.ShowPrompt(prompt, gameObject, 18);
        }

        if (!Input.GetKeyDown(refs.inputSettings.interactKey))
        {
            return;
        }

        int transferred = carriesSalvage
            ? baseCampZone.StoreBasicSalvage(refs.playerInventory)
            : baseCampZone.WithdrawBasicSalvage(refs.playerInventory);

        if (transferred > 0 && SystemMessageUI.Instance != null)
        {
            string action = carriesSalvage ? "SALVAGE STORED" : "SALVAGE WITHDRAWN";
            SystemMessageUI.Instance.ShowMessage(action + "\n" + transferred + " raw units transferred at base camp.", 3.5f);
        }
    }

    bool HasBasicSalvage(PlayerInventory inventory)
    {
        return inventory.GetItemCount(ItemType.MetalScrap) > 0 ||
            inventory.GetItemCount(ItemType.Wiring) > 0 ||
            inventory.GetItemCount(ItemType.CoreFragment) > 0;
    }
}
