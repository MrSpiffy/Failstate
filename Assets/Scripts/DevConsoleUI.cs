using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DevConsoleUI : MonoBehaviour
{
    public static bool IsConsoleOpen { get; private set; } = false;

    public GameObject consolePanel;
    public TMP_InputField commandInputField;
    public TextMeshProUGUI historyText;
    public GameObject suggestionPanel;
    public TextMeshProUGUI suggestionText;
    public ScrollRect historyScrollRect;

    public PlayerMovement playerMovement;
    public CameraFollow cameraFollow;
    public ScrapInventory scrapInventory;
    public PlayerCondition playerCondition;

    public static DevConsoleUI Instance;

    public static int LastClosedFrame { get; private set; } = -1;

    private readonly List<string> historyLines = new List<string>();
    private const int maxHistoryLines = 100;

    public static bool ConsumeEscape()
{
    if (IsConsoleOpen)
    {
        if (Instance != null)
        {
            Instance.CloseConsole();
        }

        return true;
    }

    if (LastClosedFrame == Time.frameCount)
    {
        return true;
    }

    return false;
}

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Slash) && !IsConsoleOpen)
        {
            OpenConsole();
            return;
        }

        if (!IsConsoleOpen)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseConsole();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            string input = commandInputField.text.Trim();

            if (!string.IsNullOrWhiteSpace(input))
            {
                ExecuteCommand(input);
            }

            commandInputField.text = "";
            commandInputField.ActivateInputField();
            UpdateSuggestionUI();
            return;
        }

        UpdateSuggestionUI();
    }

    void OpenConsole()
    {
        IsConsoleOpen = true;

        if (consolePanel != null)
        {
            consolePanel.SetActive(true);
        }

        if (playerMovement != null)
        {
            playerMovement.SetCanMove(false);
        }

        if (cameraFollow != null)
        {
            cameraFollow.SetCanLook(false);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (commandInputField != null)
        {
            commandInputField.text = "";
            commandInputField.ActivateInputField();
            commandInputField.Select();
        }

        UpdateHistoryText();
        UpdateSuggestionUI();
        StartCoroutine(ScrollToBottomNextFrame());
    }

    public void CloseConsole()
    {
        IsConsoleOpen = false;
        LastClosedFrame = Time.frameCount;

        if (consolePanel != null)
        {
            consolePanel.SetActive(false);
        }

        if (playerMovement != null)
        {
            playerMovement.SetCanMove(true);
        }

        if (cameraFollow != null)
        {
            cameraFollow.SetCanLook(true);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void UpdateHistoryText()
    {
        if (historyText != null)
        {
            historyText.text = string.Join("\n", historyLines);
        }
    }

    IEnumerator ScrollToBottomNextFrame()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();

        if (historyScrollRect != null)
        {
            historyScrollRect.verticalNormalizedPosition = 0f;
        }
    }

    void AddHistoryLine(string line)
    {
        historyLines.Add(line);

        while (historyLines.Count > maxHistoryLines)
        {
            historyLines.RemoveAt(0);
        }

        UpdateHistoryText();
        StartCoroutine(ScrollToBottomNextFrame());
    }

    void UpdateSuggestionUI()
    {
        if (commandInputField == null || suggestionPanel == null || suggestionText == null)
        {
            return;
        }

        string suggestion = GetSuggestion(commandInputField.text.TrimStart());

        bool hasSuggestion = !string.IsNullOrWhiteSpace(suggestion);
        suggestionPanel.SetActive(hasSuggestion);

        if (hasSuggestion)
        {
            suggestionText.text = suggestion;
        }
    }

    string GetSuggestion(string input)
    {
        string[] rawParts = input.ToLower().Split(' ');
        List<string> partsList = new List<string>(rawParts);

        if (partsList.Count > 0 && partsList[partsList.Count - 1] == "")
        {
            partsList.RemoveAt(partsList.Count - 1);
        }

        string[] parts = partsList.ToArray();
        bool endsWithSpace = input.EndsWith(" ");

        if (string.IsNullOrWhiteSpace(input))
        {
            return "<color=#A0A0A0>help</color>, <color=#A0A0A0>add</color>, <color=#A0A0A0>set</color>, <color=#A0A0A0>restore</color>";
        }

        if (parts.Length == 1 && !endsWithSpace)
        {
            return BuildStyledSuggestions(parts[0], new string[] { "help", "add", "set", "restore" });
        }

        if (parts.Length >= 1 && parts[0] == "add")
        {
            if (parts.Length == 1 && endsWithSpace)
            {
                return BuildStyledSuggestions("", new string[] { "metal", "wiring", "corefragments" });
            }

            if (parts.Length == 2 && !endsWithSpace)
            {
                return BuildStyledSuggestions(parts[1], new string[] { "metal", "wiring", "corefragments" });
            }

            if ((parts.Length == 2 && endsWithSpace) || (parts.Length == 3 && !endsWithSpace))
            {
                return "<color=#A0A0A0>amount</color>";
            }
        }

        if (parts.Length >= 1 && parts[0] == "set")
        {
            if (parts.Length == 1 && endsWithSpace)
            {
                return BuildStyledSuggestions("", new string[] { "core", "mobility", "perception" });
            }

            if (parts.Length == 2 && !endsWithSpace)
            {
                return BuildStyledSuggestions(parts[1], new string[] { "core", "mobility", "perception" });
            }

            if ((parts.Length == 2 && endsWithSpace) || (parts.Length == 3 && !endsWithSpace))
            {
                return "<color=#A0A0A0>value</color>";
            }
        }

        return "";
    }

    string BuildStyledSuggestions(string partial, string[] options)
    {
        List<string> matches = new List<string>();

        foreach (string option in options)
        {
            if (option.StartsWith(partial))
            {
                string typedPart = option.Substring(0, partial.Length);
                string remainingPart = option.Substring(partial.Length);

                matches.Add(
                    "<color=#FFFFFF>" + typedPart + "</color><color=#A0A0A0>" + remainingPart + "</color>"
                );
            }
        }

        return string.Join("\n", matches);
    }

    void ExecuteCommand(string input)
    {
        AddHistoryLine("> /" + input);

        string[] parts = input.ToLower().Split(' ');

        if (parts.Length == 0)
        {
            return;
        }

        switch (parts[0])
        {
            case "help":
                AddHistoryLine("Commands: help, add <metal|wiring|corefragments> <amount>, set <core|mobility|perception> <value>, restore");
                break;

            case "add":
                HandleAddCommand(parts);
                break;

            case "set":
                HandleSetCommand(parts);
                break;

            case "restore":
                if (playerCondition != null)
                {
                    playerCondition.FullyRestoreAllSystems();

                    if (scrapInventory != null)
                    {
                        scrapInventory.UpdateInventoryText();
                    }

                    AddHistoryLine("All systems fully restored.");
                }
                else
                {
                    AddHistoryLine("PlayerCondition not found.");
                }
                break;

            default:
                AddHistoryLine("Unknown command: " + input);
                break;
        }
    }

    void HandleAddCommand(string[] parts)
    {
        if (scrapInventory == null || parts.Length < 3)
        {
            AddHistoryLine("Usage: add <metal|wiring|corefragments> <amount>");
            return;
        }

        if (!int.TryParse(parts[2], out int amount))
        {
            AddHistoryLine("Invalid amount.");
            return;
        }

        switch (parts[1])
        {
            case "metal":
                scrapInventory.AddResource(ResourceType.MetalScrap, amount);
                break;
            case "wiring":
                scrapInventory.AddResource(ResourceType.Wiring, amount);
                break;
            case "corefragments":
                scrapInventory.AddResource(ResourceType.CoreFragment, amount);
                break;
            default:
                AddHistoryLine("Unknown resource type.");
                return;
        }

        AddHistoryLine("Added " + amount + " " + parts[1] + ".");
    }

    void HandleSetCommand(string[] parts)
    {
        if (playerCondition == null || parts.Length < 3)
        {
            AddHistoryLine("Usage: set <core|mobility|perception> <value>");
            return;
        }

        if (!float.TryParse(parts[2], out float value))
        {
            AddHistoryLine("Invalid value.");
            return;
        }

        switch (parts[1])
        {
            case "core":
                playerCondition.currentCoreIntegrity = Mathf.Clamp(value, 0f, playerCondition.maxCoreIntegrity);
                break;
            case "mobility":
                playerCondition.currentMobilityIntegrity = Mathf.Clamp(value, 0f, playerCondition.maxMobilityIntegrity);
                break;
            case "perception":
                playerCondition.currentPerceptionIntegrity = Mathf.Clamp(value, 0f, playerCondition.maxPerceptionIntegrity);
                break;
            default:
                AddHistoryLine("Unknown system type.");
                return;
        }

        if (scrapInventory != null)
        {
            scrapInventory.UpdateInventoryText();
        }

        AddHistoryLine("Set " + parts[1] + " to " + value + ".");
    }
}