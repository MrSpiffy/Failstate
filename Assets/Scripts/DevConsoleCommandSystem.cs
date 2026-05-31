using System.Collections.Generic;
using UnityEngine;

public static class DevConsoleCommandSystem
{
    public static string[] GetCommandNames()
    {
        return new string[]
        {
        "add",
        "creative",
        "help",
        "mapdebug",
        "netstatus",
        "relay",
        "restore",
        "revealmap",
        "scan",
        "set",
        "speed",
        "trace"
        };
    }

    public static string[] GetSuggestions(string[] rawParts, bool endsWithSpace)
    {
        List<string> partsList = new List<string>();

        for (int i = 0; i < rawParts.Length; i++)
        {
            if (!string.IsNullOrWhiteSpace(rawParts[i]))
            {
                partsList.Add(rawParts[i]);
            }
        }

        if (partsList.Count == 0)
        {
            return GetCommandNames();
        }

        string command = partsList[0];

        if (partsList.Count == 1 && !endsWithSpace)
        {
            return FilterSuggestions(command, GetCommandNames());
        }

        if (command == "add")
        {
            if (partsList.Count == 1 && endsWithSpace)
            {
                return ItemDatabase.GetAllCommandNames();
            }

            if (partsList.Count == 2 && !endsWithSpace)
            {
                return FilterSuggestions(partsList[1], ItemDatabase.GetAllCommandNames());
            }

            if (partsList.Count == 2 && endsWithSpace)
            {
                return new string[] { "amount" };
            }

            if (partsList.Count == 3 && !endsWithSpace)
            {
                return FilterSuggestions(partsList[2], new string[] { "amount" });
            }

            return new string[0];
        }

        if (command == "set")
        {
            if (partsList.Count == 1 && endsWithSpace)
            {
                return new string[] { "core", "mobility", "perception" };
            }

            if (partsList.Count == 2 && !endsWithSpace)
            {
                return FilterSuggestions(partsList[1], new string[] { "core", "mobility", "perception" });
            }

            if (partsList.Count == 2 && endsWithSpace)
            {
                return new string[] { "value" };
            }

            if (partsList.Count == 3 && !endsWithSpace)
            {
                return FilterSuggestions(partsList[2], new string[] { "value" });
            }

            return new string[0];
        }

        if (command == "creative")
        {
            if (partsList.Count == 1 && endsWithSpace)
            {
                return new string[] { "off", "on" };
            }

            if (partsList.Count == 2 && !endsWithSpace)
            {
                return FilterSuggestions(partsList[1], new string[] { "off", "on" });
            }

            return new string[0];
        }

        if (command == "mapdebug")
        {
            if (partsList.Count == 1 && endsWithSpace)
            {
                return new string[] { "off", "on" };
            }

            if (partsList.Count == 2 && !endsWithSpace)
            {
                return FilterSuggestions(partsList[1], new string[] { "off", "on" });
            }

            return new string[0];
        }

        if (command == "speed")
        {
            if (partsList.Count == 1 && endsWithSpace)
            {
                return new string[] { "value" };
            }

            if (partsList.Count == 2 && !endsWithSpace)
            {
                return FilterSuggestions(partsList[1], new string[] { "value" });
            }

            return new string[0];
        }

        if (command == "relay")
        {
            if (partsList.Count == 1 && endsWithSpace)
            {
                return new string[] { "root", "signal", "power", "transit", "chain", "all" };
            }

            if (partsList.Count == 2 && !endsWithSpace)
            {
                return FilterSuggestions(partsList[1], new string[] { "root", "signal", "power", "transit", "chain", "all" });
            }

            return new string[0];
        }

        if (command == "trace")
        {
            if (partsList.Count == 1 && endsWithSpace)
            {
                return new string[] { "1", "2", "3", "all" };
            }

            if (partsList.Count == 2 && !endsWithSpace)
            {
                return FilterSuggestions(partsList[1], new string[] { "1", "2", "3", "all" });
            }

            return new string[0];
        }

        return new string[0];
    }

    public static DevConsoleCommandResult Execute(
        string input,
        PlayerInventory playerInventory,
        PlayerCondition playerCondition
    )
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return new DevConsoleCommandResult(false, "");
        }

        string[] parts = input.ToLower().Split(' ');

        switch (parts[0])
        {
            case "help":
                return new DevConsoleCommandResult(
                    true,
                    "Commands: help, add <item> <amount>, set <core|mobility|perception> <value>, restore, relay <root|signal|power|transit|chain|all>, trace <1|2|3|all>, creative <on|off>, speed <value>, revealmap, mapdebug <on|off>, scan, netstatus"
                );

            case "add":
                return ExecuteAdd(parts, playerInventory);

            case "set":
                return ExecuteSet(parts, playerCondition);

            case "restore":
                return ExecuteRestore(playerCondition);

            case "creative":
                return ExecuteCreative(parts);

            case "speed":
                return ExecuteSpeed(parts);

            case "revealmap":
                return ExecuteRevealMap();

            case "mapdebug":
                return ExecuteMapDebug(parts);

            case "scan":
                return ExecuteScan();

            case "netstatus":
                return ExecuteNetworkStatus();

            case "relay":
                return ExecuteRelay(parts);

            case "trace":
                return ExecuteTrace(parts);

            default:
                return new DevConsoleCommandResult(false, "Unknown command: " + input);
        }
    }

    static DevConsoleCommandResult ExecuteMapDebug(string[] parts)
    {
        GameReferences refs = GameReferences.Instance;

        if (refs == null || refs.minimapUI == null)
        {
            return new DevConsoleCommandResult(false, "MinimapUI reference not found.");
        }

        if (parts.Length < 2)
        {
            return new DevConsoleCommandResult(false, "Usage: mapdebug <on|off>");
        }

        if (parts[1] == "on")
        {
            refs.minimapUI.SetDebugMapColors(true);
            return new DevConsoleCommandResult(true, "Debug map colors enabled.");
        }

        if (parts[1] == "off")
        {
            refs.minimapUI.SetDebugMapColors(false);
            return new DevConsoleCommandResult(true, "Debug map colors disabled.");
        }

        return new DevConsoleCommandResult(false, "Usage: mapdebug <on|off>");
    }

    static DevConsoleCommandResult ExecuteRevealMap()
    {
        GameReferences refs = GameReferences.Instance;

        if (refs == null || refs.minimapUI == null)
        {
            return new DevConsoleCommandResult(false, "MinimapUI reference not found.");
        }

        refs.minimapUI.RevealEntireMap();

        return new DevConsoleCommandResult(true, "Map fully revealed.");
    }

    static DevConsoleCommandResult ExecuteScan()
    {
        GameReferences refs = GameReferences.Instance;

        if (refs == null || refs.minimapUI == null)
        {
            return new DevConsoleCommandResult(false, "MinimapUI reference not found.");
        }

        if (!refs.minimapUI.TryScanFromPlayer())
        {
            return new DevConsoleCommandResult(false, "Scan unavailable or cooling down.");
        }

        return new DevConsoleCommandResult(true, "Local signal scan emitted.");
    }

    static DevConsoleCommandResult ExecuteNetworkStatus()
    {
        GameReferences refs = GameReferences.Instance;

        if (refs == null || refs.infrastructureNetworkManager == null)
        {
            return new DevConsoleCommandResult(false, "InfrastructureNetworkManager reference not found.");
        }

        InfrastructureNetworkManager network = refs.infrastructureNetworkManager;
        string rootState = network.IsBaseCampRootOnline() ? "online" : "offline";

        return new DevConsoleCommandResult(
            true,
            "Root relay: " + rootState +
            " | Nodes restored: " + network.GetRestoredNodeCount() + "/" + network.GetTotalNodeCount() +
            " | Signal x" + network.GetSignalRangeMultiplier().ToString("0.00") +
            " | Reveal x" + network.GetRevealRangeMultiplier().ToString("0.00") +
            " | Decay x" + network.GetPassiveDecayMultiplier().ToString("0.00")
        );
    }

    static DevConsoleCommandResult ExecuteRelay(string[] parts)
    {
        GameReferences refs = GameReferences.Instance;

        if (refs == null || refs.infrastructureNetworkManager == null || refs.baseCampZone == null)
        {
            return new DevConsoleCommandResult(false, "Base camp or infrastructure network reference not found.");
        }

        if (parts.Length < 2)
        {
            return new DevConsoleCommandResult(false, "Usage: relay <root|signal|power|transit|chain|all>");
        }

        InfrastructureNetworkManager network = refs.infrastructureNetworkManager;
        string target = parts[1];

        if (target == "root")
        {
            bool restoredRoot = refs.baseCampZone.DebugRestoreRootRelay(refs);
            return new DevConsoleCommandResult(true, restoredRoot ? "Base Camp Root Relay restored." : "Base Camp Root Relay is already online.");
        }

        if (target == "chain" || target == "all")
        {
            int restoredCount = refs.baseCampZone.DebugRestoreRootRelay(refs) ? 1 : 0;

            restoredCount += DebugRestoreFirstNodeOfType(network, refs, InfrastructureNodeType.SignalRelay);
            restoredCount += DebugRestoreFirstNodeOfType(network, refs, InfrastructureNodeType.PowerJunction);
            restoredCount += DebugRestoreFirstNodeOfType(network, refs, InfrastructureNodeType.TransitLift);

            if (target == "all")
            {
                for (int i = 0; i < network.GetNodeCount(); i++)
                {
                    InfrastructureNode node = network.GetNode(i);

                    if (node != null && node.DebugRestore(refs))
                    {
                        restoredCount++;
                    }
                }
            }

            return new DevConsoleCommandResult(
                true,
                restoredCount > 0
                    ? "Debug restored " + restoredCount + " relay component(s). Network effects applied."
                    : "Requested relay components are already online."
            );
        }

        if (!TryParseNodeType(target, out InfrastructureNodeType nodeType))
        {
            return new DevConsoleCommandResult(false, "Usage: relay <root|signal|power|transit|chain|all>");
        }

        InfrastructureNode selectedNode = FindPreferredNode(network, nodeType);

        if (selectedNode == null)
        {
            return new DevConsoleCommandResult(false, "No " + target + " relay found in this generated district.");
        }

        if (!selectedNode.DebugRestore(refs))
        {
            return new DevConsoleCommandResult(true, selectedNode.GetDisplayName() + " is already online.");
        }

        return new DevConsoleCommandResult(true, selectedNode.GetDisplayName() + " restored. Network effects applied.");
    }

    static int DebugRestoreFirstNodeOfType(InfrastructureNetworkManager network, GameReferences refs, InfrastructureNodeType nodeType)
    {
        InfrastructureNode node = FindPreferredNode(network, nodeType);
        return node != null && node.DebugRestore(refs) ? 1 : 0;
    }

    static InfrastructureNode FindPreferredNode(InfrastructureNetworkManager network, InfrastructureNodeType nodeType)
    {
        InfrastructureNode fallback = null;

        for (int i = 0; i < network.GetNodeCount(); i++)
        {
            InfrastructureNode node = network.GetNode(i);

            if (node == null || node.nodeType != nodeType)
            {
                continue;
            }

            if (node.requiredChainLandmark)
            {
                return node;
            }

            if (fallback == null)
            {
                fallback = node;
            }
        }

        return fallback;
    }

    static bool TryParseNodeType(string target, out InfrastructureNodeType nodeType)
    {
        switch (target)
        {
            case "signal":
            case "sig":
                nodeType = InfrastructureNodeType.SignalRelay;
                return true;

            case "power":
            case "pwr":
                nodeType = InfrastructureNodeType.PowerJunction;
                return true;

            case "transit":
            case "lift":
                nodeType = InfrastructureNodeType.TransitLift;
                return true;

            default:
                nodeType = InfrastructureNodeType.SignalRelay;
                return false;
        }
    }

    static DevConsoleCommandResult ExecuteTrace(string[] parts)
    {
        if (parts.Length < 2)
        {
            return new DevConsoleCommandResult(false, "Usage: trace <1|2|3|all>");
        }

        EnvironmentalFragment[] fragments = Object.FindObjectsByType<EnvironmentalFragment>(FindObjectsSortMode.None);
        GameReferences refs = GameReferences.Instance;

        if (parts[1] == "all")
        {
            int recoveredCount = 0;

            for (int i = 0; i < fragments.Length; i++)
            {
                if (fragments[i] != null && fragments[i].Recover(refs))
                {
                    recoveredCount++;
                }
            }

            return new DevConsoleCommandResult(
                true,
                recoveredCount > 0
                    ? "Recovered " + recoveredCount + " data trace(s). Archive updated."
                    : "All generated data traces are already recovered."
            );
        }

        if (!int.TryParse(parts[1], out int fragmentIndex))
        {
            return new DevConsoleCommandResult(false, "Usage: trace <1|2|3|all>");
        }

        for (int i = 0; i < fragments.Length; i++)
        {
            if (fragments[i] == null || fragments[i].fragmentIndex != fragmentIndex)
            {
                continue;
            }

            bool recovered = fragments[i].Recover(refs);
            return new DevConsoleCommandResult(
                true,
                recovered
                    ? "Recovered data trace " + fragmentIndex + ". Archive updated."
                    : "Data trace " + fragmentIndex + " is already recovered."
            );
        }

        return new DevConsoleCommandResult(false, "Data trace " + fragmentIndex + " was not found in this generated district.");
    }

    static DevConsoleCommandResult ExecuteAdd(string[] parts, PlayerInventory playerInventory)
    {
        if (playerInventory == null || parts.Length < 3)
        {
            return new DevConsoleCommandResult(false, "Usage: add <item> <amount>");
        }

        if (!ItemDatabase.TryGetItemType(parts[1], out ItemType itemType))
        {
            return new DevConsoleCommandResult(false, "Unknown item type.");
        }

        if (!int.TryParse(parts[2], out int amount))
        {
            return new DevConsoleCommandResult(false, "Invalid amount.");
        }

        playerInventory.AddItem(itemType, amount);

        return new DevConsoleCommandResult(
            true,
            "Added " + amount + " " + ItemDatabase.GetDisplayName(itemType) + "."
        );
    }

    static DevConsoleCommandResult ExecuteSet(string[] parts, PlayerCondition playerCondition)
    {
        if (playerCondition == null || parts.Length < 3)
        {
            return new DevConsoleCommandResult(false, "Usage: set <core|mobility|perception> <value>");
        }

        if (!float.TryParse(parts[2], out float value))
        {
            return new DevConsoleCommandResult(false, "Invalid value.");
        }

        switch (parts[1])
        {
            case "core":
                playerCondition.currentCoreIntegrity =
                    UnityEngine.Mathf.Clamp(value, 0f, playerCondition.maxCoreIntegrity);
                return new DevConsoleCommandResult(true, "Set core to " + value + ".");

            case "mobility":
                playerCondition.currentMobilityIntegrity =
                    UnityEngine.Mathf.Clamp(value, 0f, playerCondition.maxMobilityIntegrity);
                return new DevConsoleCommandResult(true, "Set mobility to " + value + ".");

            case "perception":
                playerCondition.currentPerceptionIntegrity =
                    UnityEngine.Mathf.Clamp(value, 0f, playerCondition.maxPerceptionIntegrity);
                return new DevConsoleCommandResult(true, "Set perception to " + value + ".");

            default:
                return new DevConsoleCommandResult(false, "Unknown system type.");
        }
    }

    static DevConsoleCommandResult ExecuteRestore(PlayerCondition playerCondition)
    {
        if (playerCondition == null)
        {
            return new DevConsoleCommandResult(false, "PlayerCondition not found.");
        }

        playerCondition.FullyRestoreAllSystems();
        return new DevConsoleCommandResult(true, "All systems fully restored.");
    }

    static string[] FilterSuggestions(string partial, string[] options)
    {
        List<string> matches = new List<string>();

        for (int i = 0; i < options.Length; i++)
        {
            if (options[i].StartsWith(partial))
            {
                matches.Add(options[i]);
            }
        }

        matches.Sort();
        return matches.ToArray();
    }

    static DevConsoleCommandResult ExecuteCreative(string[] parts)
    {
        GameReferences refs = GameReferences.Instance;

        if (refs == null || refs.playerObject == null)
        {
            return new DevConsoleCommandResult(false, "Player reference not found.");
        }

        PlayerMovement movement = refs.playerObject.GetComponent<PlayerMovement>();

        if (movement == null)
        {
            return new DevConsoleCommandResult(false, "PlayerMovement not found.");
        }

        if (parts.Length < 2)
        {
            return new DevConsoleCommandResult(false, "Usage: creative <on|off>");
        }

        if (parts[1] == "on")
        {
            movement.SetCreativeMode(true);
            return new DevConsoleCommandResult(true, "Creative mode enabled.");
        }

        if (parts[1] == "off")
        {
            movement.SetCreativeMode(false);
            return new DevConsoleCommandResult(true, "Creative mode disabled.");
        }

        return new DevConsoleCommandResult(false, "Usage: creative <on|off>");
    }

    static DevConsoleCommandResult ExecuteSpeed(string[] parts)
    {
        GameReferences refs = GameReferences.Instance;

        if (refs == null || refs.playerObject == null)
        {
            return new DevConsoleCommandResult(false, "Player reference not found.");
        }

        PlayerMovement movement = refs.playerObject.GetComponent<PlayerMovement>();

        if (movement == null)
        {
            return new DevConsoleCommandResult(false, "PlayerMovement not found.");
        }

        if (parts.Length < 2)
        {
            return new DevConsoleCommandResult(false, "Usage: speed <value>");
        }

        if (!float.TryParse(parts[1], out float speed))
        {
            return new DevConsoleCommandResult(false, "Invalid speed.");
        }

        movement.SetCreativeSpeed(speed);

        return new DevConsoleCommandResult(true, "Creative speed set to " + speed + ".");
    }
}
