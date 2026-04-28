using System.Collections.Generic;

public static class DevConsoleCommandSystem
{
    public static string[] GetCommandNames()
    {
        return new string[]
        {
            "add",
            "help",
            "restore",
            "set"
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
                    "Commands: help, add <item> <amount>, set <core|mobility|perception> <value>, restore"
                );

            case "add":
                return ExecuteAdd(parts, playerInventory);

            case "set":
                return ExecuteSet(parts, playerCondition);

            case "restore":
                return ExecuteRestore(playerCondition);

            default:
                return new DevConsoleCommandResult(false, "Unknown command: " + input);
        }
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
}