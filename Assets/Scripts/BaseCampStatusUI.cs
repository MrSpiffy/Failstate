using TMPro;
using UnityEngine;

public class BaseCampStatusUI : MonoBehaviour
{
    public GameObject statusObject;
    public TextMeshProUGUI statusText;

    private string cachedStatusText = "";
    private bool visible = false;
    private GUIStyle labelStyle;

    void Awake()
    {
        ApplyStatusLayout();
    }

    void OnGUI()
    {
        if (!visible || string.IsNullOrEmpty(cachedStatusText))
        {
            return;
        }

        if (UIStateManager.CurrentState == UIState.Pause || UIStateManager.CurrentState == UIState.GameOver)
        {
            return;
        }

        EnsureStyle();

        Rect objectiveRect = GetObjectivePanelRect();
        float scale = GetHudScale();
        Rect safeArea = GetGuiSafeArea();
        float margin = Mathf.Clamp(18f * scale, 14f, 24f);
        float width = Mathf.Min(430f * scale, safeArea.width * 0.56f);
        float availableHeight = Mathf.Max(48f, safeArea.yMax - objectiveRect.yMax - margin);
        Rect statusRect = new Rect(
            objectiveRect.x,
            objectiveRect.yMax + margin,
            width,
            Mathf.Min(132f * scale, availableHeight)
        );

        GUI.Label(statusRect, cachedStatusText, labelStyle);
    }

    public void ShowBaseCampStatus()
    {
        ShowBaseCampStatus(null);
    }

    public void ShowBaseCampStatus(BaseCampZone baseCamp)
    {
        visible = true;
        string networkStatus = GetCompactNetworkStatusText();

        if (baseCamp == null)
        {
            cachedStatusText =
                "BASE CAMP\n" +
                "Passive decay paused\n" +
                "Services: recharge / workbench / storage\n" +
                networkStatus;
        }
        else
        {
            cachedStatusText =
                "BASE CAMP\n" +
                "Passive decay paused\n" +
                "Services: recharge / workbench / storage\n" +
                baseCamp.GetRootRelayStatusText() + "\n" +
                baseCamp.GetDepositSummary() +
                networkStatus;
        }

        if (statusText != null)
        {
            statusText.text = "";
        }

        if (statusObject != null)
        {
            statusObject.SetActive(false);
        }
    }

    public void HideBaseCampStatus()
    {
        visible = false;
        cachedStatusText = "";

        if (statusObject != null)
        {
            statusObject.SetActive(false);
        }
    }

    string GetCompactNetworkStatusText()
    {
        GameReferences refs = GameReferences.Instance;
        InfrastructureNetworkManager network = refs != null ? refs.infrastructureNetworkManager : null;

        if (network == null)
        {
            network = FindFirstObjectByType<InfrastructureNetworkManager>();
        }

        if (network == null || network.GetTotalNodeCount() == 0)
        {
            return "";
        }

        return "\nGrid nodes: " + network.GetRestoredNodeCount() + "/" + network.GetTotalNodeCount();
    }

    void ApplyStatusLayout()
    {
        if (statusText == null)
        {
            return;
        }

        float scale = GetHudScale();
        statusText.fontSize = Mathf.RoundToInt(16f * scale);
        statusText.color = new Color(0.48f, 0.95f, 0.58f, 0.9f);
        statusText.alignment = TextAlignmentOptions.TopLeft;
        statusText.textWrappingMode = TextWrappingModes.Normal;
        statusText.overflowMode = TextOverflowModes.Overflow;

        RectTransform rectTransform = statusText.rectTransform;

        if (rectTransform != null)
        {
            Rect objectiveRect = GetObjectivePanelRect();
            float margin = Mathf.Clamp(16f * scale, 10f, 22f);
            float y = -(objectiveRect.yMax + margin);
            float width = Mathf.Min(430f * scale, Screen.width * 0.56f);

            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(0f, 1f);
            rectTransform.pivot = new Vector2(0f, 1f);
            rectTransform.anchoredPosition = new Vector2(objectiveRect.x, y);
            rectTransform.sizeDelta = new Vector2(width, 96f * scale);
        }
    }

    float GetHudScale()
    {
        return Mathf.Clamp(GetGuiSafeArea().height / 720f, 0.9f, 1.08f);
    }

    Rect GetObjectivePanelRect()
    {
        NetworkObjectiveUI objectiveUI = FindFirstObjectByType<NetworkObjectiveUI>();

        if (objectiveUI != null)
        {
            return objectiveUI.GetCurrentPanelRect();
        }

        float scale = GetHudScale();
        Rect safeArea = GetGuiSafeArea();
        float margin = Mathf.Clamp(18f * scale, 10f, 24f);
        return new Rect(safeArea.xMin + margin, safeArea.yMin + margin, Mathf.Min(430f * scale, safeArea.width * 0.58f), 108f * scale);
    }

    void EnsureStyle()
    {
        if (labelStyle == null)
        {
            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.normal.textColor = new Color(0.45f, 1f, 0.58f, 0.92f);
            labelStyle.wordWrap = true;
            labelStyle.richText = false;
            labelStyle.alignment = TextAnchor.UpperLeft;
        }

        labelStyle.fontSize = Mathf.RoundToInt(13f * GetHudScale());
    }

    Rect GetGuiSafeArea()
    {
        Rect safeArea = Screen.safeArea;
        return new Rect(
            safeArea.xMin,
            Screen.height - safeArea.yMax,
            safeArea.width,
            safeArea.height
        );
    }
}
