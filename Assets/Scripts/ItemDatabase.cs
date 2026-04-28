using System.Collections.Generic;

public static class ItemDatabase
{
    public static ItemUseEffect GetUseEffect(ItemType itemType)
{
    switch (itemType)
    {
        case ItemType.RepairKit:
            return new ItemUseEffect(ItemUseEffectType.RestorePlayerSystem, PlayerSystemType.Core, 25f);

        case ItemType.MobilityPatch:
            return new ItemUseEffect(ItemUseEffectType.RestorePlayerSystem, PlayerSystemType.Mobility, 25f);

        case ItemType.SensorPatch:
            return new ItemUseEffect(ItemUseEffectType.RestorePlayerSystem, PlayerSystemType.Perception, 25f);

        default:
            return new ItemUseEffect(ItemUseEffectType.None, PlayerSystemType.Core, 0f);
    }
}
    
    public static ItemType[] GetAllItemTypes()
    {
        return new ItemType[]
        {
            ItemType.MetalScrap,
            ItemType.Wiring,
            ItemType.CoreFragment,
            ItemType.RepairKit,
            ItemType.MobilityPatch,
            ItemType.SensorPatch
        };
    }

    public static string[] GetAllCommandNames()
    {
        List<string> names = new List<string>();

        foreach (ItemType itemType in GetAllItemTypes())
        {
            names.Add(GetCommandName(itemType));
        }

        names.Sort();
        return names.ToArray();
    }

    public static string GetCommandName(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.MetalScrap: return "metal";
            case ItemType.Wiring: return "wiring";
            case ItemType.CoreFragment: return "corefragment";
            case ItemType.RepairKit: return "repairkit";
            case ItemType.MobilityPatch: return "mobilitypatch";
            case ItemType.SensorPatch: return "sensorpatch";
            default: return "unknown";
        }
    }

    public static bool TryGetItemType(string input, out ItemType itemType)
    {
        input = input.ToLower().Replace(" ", "").Replace("_", "");

        switch (input)
        {
            case "metal":
            case "metalscrap":
                itemType = ItemType.MetalScrap;
                return true;

            case "wiring":
                itemType = ItemType.Wiring;
                return true;

            case "corefragment":
            case "corefragments":
                itemType = ItemType.CoreFragment;
                return true;

            case "repairkit":
            case "repairkits":
                itemType = ItemType.RepairKit;
                return true;

            case "mobilitypatch":
            case "mobilitypatches":
                itemType = ItemType.MobilityPatch;
                return true;

            case "sensorpatch":
            case "sensorpatches":
                itemType = ItemType.SensorPatch;
                return true;
        }

        itemType = ItemType.MetalScrap;
        return false;
    }

    public static string GetDisplayName(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.MetalScrap: return "Metal Scrap";
            case ItemType.Wiring: return "Wiring";
            case ItemType.CoreFragment: return "Core Fragment";
            case ItemType.RepairKit: return "Repair Kit";
            case ItemType.MobilityPatch: return "Mobility Patch";
            case ItemType.SensorPatch: return "Sensor Patch";
            default: return "Unknown Item";
        }
    }

    public static string GetDescription(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.MetalScrap:
                return "Basic structural material.\n\nUsed for crafting and core repairs.\n\nFound while scavenging.";
            case ItemType.Wiring:
                return "Useful for mobility and electrical repair.\n\nUsed in mobility-related crafting.\n\nFound in ruined machines.";
            case ItemType.CoreFragment:
                return "Rare sensor material.\n\nUsed for perception-related crafting.\n\nFound in advanced wreckage.";
            case ItemType.RepairKit:
                return "Restores Core integrity by 25.\n\nPress E to use.";
            case ItemType.MobilityPatch:
                return "Restores Mobility integrity by 25.\n\nPress E to use.";
            case ItemType.SensorPatch:
                return "Restores Perception integrity by 25.\n\nPress E to use.";
            default:
                return "No description available.";
        }
    }
}