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
            ItemType.SensorPatch,
            ItemType.CircuitScrap,
            ItemType.EnergyCell,
            ItemType.SignalProcessor,
            ItemType.ConduitComponents,
            ItemType.StabilizerModule,
            ItemType.PowerRegulator,
            ItemType.PowerCoupler,
            ItemType.TransitActuator,
            ItemType.TransitCore,
            ItemType.TransitControlModule
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
            case ItemType.CircuitScrap: return "circuit";
            case ItemType.EnergyCell: return "energycell";
            case ItemType.SignalProcessor: return "signalprocessor";
            case ItemType.ConduitComponents: return "conduitcomponents";
            case ItemType.StabilizerModule: return "stabilizermodule";
            case ItemType.PowerRegulator: return "powerregulator";
            case ItemType.PowerCoupler: return "powercoupler";
            case ItemType.TransitActuator: return "transitactuator";
            case ItemType.TransitCore: return "transitcore";
            case ItemType.TransitControlModule: return "transitcontrolmodule";
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

            case "circuit":
            case "circuits":
            case "circuitscrap":
                itemType = ItemType.CircuitScrap;
                return true;

            case "energy":
            case "energycell":
            case "energycells":
                itemType = ItemType.EnergyCell;
                return true;

            case "signalprocessor":
            case "processor":
                itemType = ItemType.SignalProcessor;
                return true;

            case "conduit":
            case "conduitcomponent":
            case "conduitcomponents":
                itemType = ItemType.ConduitComponents;
                return true;

            case "stabilizer":
            case "stabilizermodule":
                itemType = ItemType.StabilizerModule;
                return true;

            case "powerregulator":
            case "regulator":
                itemType = ItemType.PowerRegulator;
                return true;

            case "powercoupler":
            case "coupler":
                itemType = ItemType.PowerCoupler;
                return true;

            case "transitactuator":
            case "actuator":
                itemType = ItemType.TransitActuator;
                return true;

            case "transitcore":
                itemType = ItemType.TransitCore;
                return true;

            case "transitcontrolmodule":
            case "controlmodule":
                itemType = ItemType.TransitControlModule;
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
            case ItemType.CircuitScrap: return "Circuit Scrap";
            case ItemType.EnergyCell: return "Energy Cell";
            case ItemType.SignalProcessor: return "Signal Processor";
            case ItemType.ConduitComponents: return "Conduit Components";
            case ItemType.StabilizerModule: return "Stabilizer Module";
            case ItemType.PowerRegulator: return "Power Regulator";
            case ItemType.PowerCoupler: return "Power Coupler";
            case ItemType.TransitActuator: return "Transit Actuator";
            case ItemType.TransitCore: return "Transit Core";
            case ItemType.TransitControlModule: return "Transit Control Module";
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
            case ItemType.CircuitScrap:
                return "Reusable control-board fragments.\n\nUsed for relay restoration crafting.";
            case ItemType.EnergyCell:
                return "A charged cell pulled from city infrastructure.\n\nUsed for relay stabilization.";
            case ItemType.SignalProcessor:
                return "A relay-specific processor core.\n\nRequired to restore the Signal Relay.";
            case ItemType.ConduitComponents:
                return "Replacement couplers and conduit hardware.\n\nRequired to reconnect relay output lines.";
            case ItemType.StabilizerModule:
                return "Workbench-built relay stabilizer.\n\nRequired to bring a damaged relay safely online.";
            case ItemType.PowerRegulator:
                return "Replacement regulator for a damaged power relay.\n\nRequired for Power Relay restoration.";
            case ItemType.PowerCoupler:
                return "Workbench-built coupler for routing restored energy flow.\n\nRequired for Power Relay restoration.";
            case ItemType.TransitActuator:
                return "Heavy actuator assembly for a failed transit relay frame.\n\nRequired for Transit Relay restoration.";
            case ItemType.TransitCore:
                return "Core control unit for a transit spine relay.\n\nRequired for Transit Relay restoration.";
            case ItemType.TransitControlModule:
                return "Workbench-built module that synchronizes transit control stations.\n\nRequired for Transit Relay restoration.";
            default:
                return "No description available.";
        }
    }
}
