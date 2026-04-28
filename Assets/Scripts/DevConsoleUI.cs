using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DevConsoleUI : MonoBehaviour
{
    public static bool IsConsoleOpen { get; private set; } = false;
    public static DevConsoleUI Instance;
    public static int LastClosedFrame { get; private set; } = -1;

    public UIStateManager uiStateManager;

    public GameObject consolePanel;
    public TMP_InputField commandInputField;
    public TextMeshProUGUI historyText;
    public GameObject suggestionPanel;
    public TextMeshProUGUI suggestionText;
    public ScrollRect historyScrollRect;

    public PlayerInventory playerInventory;
    public PlayerCondition playerCondition;

    private readonly List<string> historyLines = new List<string>();
    private readonly List<string> currentSuggestions = new List<string>();

    private const int maxHistoryLines = 100;
    private const float suggestionMinWidth = 140f;
    private const float suggestionMaxWidth = 900f;
    private const float suggestionHeight = 35f;

    private int selectedSuggestionIndex = 0;
    private string currentPartial = "";

    private const float suggestionPanelY = 43f;

    void Awake()
    {
        Instance = this;
    }

    public static bool ConsumeEscape()
    {
        if (IsConsoleOpen)
        {
            if (Instance != null) Instance.CloseConsole();
            return true;
        }

        return LastClosedFrame == Time.frameCount;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Slash) && UIStateManager.CurrentState == UIState.Gameplay)
        {
            OpenConsole();
            return;
        }

        if (!IsConsoleOpen) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseConsole();
            return;
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            MoveSuggestionSelection(-1);
            return;
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            MoveSuggestionSelection(1);
            return;
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ApplySelectedSuggestion();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            string input = commandInputField != null ? commandInputField.text.Trim() : "";

            if (!string.IsNullOrWhiteSpace(input))
            {
                ExecuteCommand(input);
            }

            if (commandInputField != null)
            {
                commandInputField.text = "";
                commandInputField.ActivateInputField();
            }

            selectedSuggestionIndex = 0;
            UpdateSuggestionUI();
            return;
        }

        UpdateSuggestionUI();
    }

    void OpenConsole()
    {
        IsConsoleOpen = true;

        if (consolePanel != null) consolePanel.SetActive(true);

        if (uiStateManager != null)
        {
            uiStateManager.SetState(UIState.DevConsole);
        }

        if (commandInputField != null)
        {
            commandInputField.text = "";
            commandInputField.ActivateInputField();
            commandInputField.Select();
        }

        selectedSuggestionIndex = 0;
        UpdateHistoryText();
        UpdateSuggestionUI();
        StartCoroutine(ScrollToBottomNextFrame());
    }

    public void CloseConsole()
    {
        IsConsoleOpen = false;
        LastClosedFrame = Time.frameCount;

        if (consolePanel != null) consolePanel.SetActive(false);

        if (uiStateManager != null)
        {
            uiStateManager.ReturnToGameplay();
        }
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
        if (commandInputField == null || suggestionPanel == null || suggestionText == null) return;

        BuildSuggestions(commandInputField.text);

        bool hasSuggestions = currentSuggestions.Count > 0;
        suggestionPanel.SetActive(hasSuggestions);

        if (!hasSuggestions) return;

        selectedSuggestionIndex = Mathf.Clamp(selectedSuggestionIndex, 0, currentSuggestions.Count - 1);

        List<string> visibleSuggestionStrings = new List<string>();
        float estimatedWidth = 20f;

        for (int i = 0; i < currentSuggestions.Count; i++)
        {
            string rawSuggestion = currentSuggestions[i];
            string styledSuggestion = StyleSuggestion(rawSuggestion, currentPartial);

            if (i == selectedSuggestionIndex)
            {
                styledSuggestion = "<mark=#555555AA>" + styledSuggestion + "</mark>";
            }

            string separator = visibleSuggestionStrings.Count > 0 ? " <color=#666666>|</color> " : "";
            string candidate = separator + styledSuggestion;

            float candidateWidth = suggestionText.GetPreferredValues(
                StripRichTextForWidth(separator + rawSuggestion)
            ).x;

            if (estimatedWidth + candidateWidth > suggestionMaxWidth && visibleSuggestionStrings.Count > 0)
            {
                break;
            }

            visibleSuggestionStrings.Add(candidate);
            estimatedWidth += candidateWidth;
        }

        suggestionText.text = string.Join("", visibleSuggestionStrings);
        ResizeSuggestionPanel(estimatedWidth);
    }

    string StripRichTextForWidth(string text)
    {
        return text
            .Replace("<color=#666666>", "")
            .Replace("<color=#FFFFFF>", "")
            .Replace("<color=#A0A0A0>", "")
            .Replace("</color>", "")
            .Replace("<mark=#555555AA>", "")
            .Replace("</mark>", "");
    }

    void BuildSuggestions(string input)
    {
        string previousSelection =
            currentSuggestions.Count > 0 && selectedSuggestionIndex < currentSuggestions.Count
                ? currentSuggestions[selectedSuggestionIndex]
                : "";

        currentSuggestions.Clear();
        currentPartial = "";

        bool endsWithSpace = input.EndsWith(" ");
        string[] parts = input.ToLower().Split(' ');

        if (!string.IsNullOrWhiteSpace(input) && !endsWithSpace)
        {
            currentPartial = parts[parts.Length - 1];
        }

        string[] suggestions = DevConsoleCommandSystem.GetSuggestions(parts, endsWithSpace);

        for (int i = 0; i < suggestions.Length; i++)
        {
            currentSuggestions.Add(suggestions[i]);
        }

        int restoredIndex = currentSuggestions.IndexOf(previousSelection);
        selectedSuggestionIndex = restoredIndex >= 0 ? restoredIndex : 0;
    }

    void AddMatchingSuggestions(string partial, string[] options)
    {
        foreach (string option in options)
        {
            if (option.StartsWith(partial))
            {
                currentSuggestions.Add(option);
            }
        }
    }

    string StyleSuggestion(string suggestion, string partial)
    {
        partial = partial.ToLower();

        if (string.IsNullOrEmpty(partial))
        {
            return "<color=#A0A0A0>" + suggestion + "</color>";
        }

        int typedLength = Mathf.Min(partial.Length, suggestion.Length);

        string typedPart = suggestion.Substring(0, typedLength);
        string remainingPart = suggestion.Substring(typedLength);

        return "<color=#FFFFFF>" + typedPart + "</color><color=#A0A0A0>" + remainingPart + "</color>";
    }

    void ResizeSuggestionPanel(float estimatedWidth)
    {
        RectTransform panelRect = suggestionPanel.GetComponent<RectTransform>();

        if (panelRect == null) return;

        RectTransform parentRect = suggestionPanel.transform.parent.GetComponent<RectTransform>();

        float maxAllowedWidth = suggestionMaxWidth;

        if (parentRect != null)
        {
            float leftPadding = 15f;
            float rightPadding = 15f;

            maxAllowedWidth = parentRect.rect.width - leftPadding - rightPadding;

            panelRect.anchoredPosition = new Vector2(leftPadding, suggestionPanelY);
        }

        float width = Mathf.Clamp(estimatedWidth + 20f, suggestionMinWidth, maxAllowedWidth);

        panelRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        panelRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, suggestionHeight);
    }

    void MoveSuggestionSelection(int direction)
    {
        if (currentSuggestions.Count == 0) return;

        selectedSuggestionIndex += direction;

        if (selectedSuggestionIndex < 0)
        {
            selectedSuggestionIndex = currentSuggestions.Count - 1;
        }
        else if (selectedSuggestionIndex >= currentSuggestions.Count)
        {
            selectedSuggestionIndex = 0;
        }

        UpdateSuggestionUI();
    }

    void ApplySelectedSuggestion()
    {
        if (commandInputField == null || currentSuggestions.Count == 0) return;

        string selectedSuggestion = currentSuggestions[selectedSuggestionIndex];

        if (selectedSuggestion == "amount" || selectedSuggestion == "value")
        {
            return;
        }

        string input = commandInputField.text;

        if (string.IsNullOrWhiteSpace(input))
        {
            commandInputField.text = selectedSuggestion + " ";
        }
        else
        {
            bool endsWithSpace = input.EndsWith(" ");
            string[] parts = input.Split(' ');

            if (endsWithSpace)
            {
                commandInputField.text = input + selectedSuggestion + " ";
            }
            else
            {
                parts[parts.Length - 1] = selectedSuggestion;
                commandInputField.text = string.Join(" ", parts) + " ";
            }
        }

        commandInputField.caretPosition = commandInputField.text.Length;
        commandInputField.ActivateInputField();

        selectedSuggestionIndex = 0;
        UpdateSuggestionUI();
    }

    void ExecuteCommand(string input)
    {
        AddHistoryLine("> /" + input);

        DevConsoleCommandResult result = DevConsoleCommandSystem.Execute(
            input,
            playerInventory,
            playerCondition
        );

        if (!string.IsNullOrWhiteSpace(result.message))
        {
            AddHistoryLine(result.message);
        }
    }
}