public static class ItemUseSystem
{
    public static bool TryUseItem(
        ItemType itemType,
        PlayerInventory inventory,
        PlayerCondition playerCondition,
        out string feedbackMessage
    )
    {
        feedbackMessage = "";

        if (inventory == null || playerCondition == null)
        {
            feedbackMessage = "Missing inventory or player condition reference.";
            return false;
        }

        ItemUseEffect effect = ItemDatabase.GetUseEffect(itemType);

        switch (effect.effectType)
        {
            case ItemUseEffectType.RestorePlayerSystem:
                return TryRestorePlayerSystem(itemType, effect, inventory, playerCondition, out feedbackMessage);

            default:
                feedbackMessage = "This item cannot be used directly.";
                return false;
        }
    }

    private static bool TryRestorePlayerSystem(
        ItemType itemType,
        ItemUseEffect effect,
        PlayerInventory inventory,
        PlayerCondition playerCondition,
        out string feedbackMessage
    )
    {
        if (inventory.GetItemCount(itemType) <= 0)
        {
            feedbackMessage = "No " + ItemDatabase.GetDisplayName(itemType) + " available.";
            return false;
        }

        if (playerCondition.IsSystemFull(effect.targetSystem))
        {
            feedbackMessage = effect.targetSystem + " is already fully repaired.";
            return false;
        }

        if (!inventory.TryRemoveItem(itemType, 1))
        {
            feedbackMessage = "Could not use item.";
            return false;
        }

        playerCondition.RestoreSystem(effect.targetSystem, effect.amount);

        feedbackMessage =
            "Used " + ItemDatabase.GetDisplayName(itemType) +
            ". " + effect.targetSystem +
            " restored by " + effect.amount + ".";

        return true;
    }
}