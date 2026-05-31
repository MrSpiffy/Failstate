using UnityEngine;

public class NetworkObjectiveUI : MonoBehaviour
{
    public bool showObjectivePanel = true;
    public Vector2 screenPosition = new Vector2(24f, 28f);
    public Vector2 panelSize = new Vector2(430f, 108f);
    public float updateInterval = 0.25f;
    public float minScale = 0.72f;
    public float maxScale = 1f;

    private InfrastructureNetworkManager networkManager;
    private FirstRunObjectiveManager firstRunObjectiveManager;
    private string cachedText = "";
    private float nextUpdateTime = -999f;
    private GUIStyle panelStyle;
    private GUIStyle labelStyle;

    void Start()
    {
        ResolveNetworkManager();
        RefreshText();
    }

    void Update()
    {
        if (Time.unscaledTime < nextUpdateTime)
        {
            return;
        }

        RefreshText();
    }

    void OnGUI()
    {
        if (!showObjectivePanel || string.IsNullOrEmpty(cachedText))
        {
            return;
        }

        if (UIStateManager.CurrentState == UIState.Pause || UIStateManager.CurrentState == UIState.GameOver)
        {
            return;
        }

        EnsureStyles();

        float scale = GetHudScale();
        Rect safeArea = GetGuiSafeArea();
        Vector2 position = GetResponsivePosition(scale, safeArea);
        float width = Mathf.Min(panelSize.x * scale, safeArea.width * 0.58f);
        Rect panelRect = new Rect(position.x, position.y, width, GetPanelHeight(scale));
        GUI.Box(panelRect, GUIContent.none, panelStyle);
        GUI.Label(new Rect(panelRect.x + 14f * scale, panelRect.y + 10f * scale, panelRect.width - 28f * scale, panelRect.height - 20f * scale), cachedText, labelStyle);
    }

    void RefreshText()
    {
        nextUpdateTime = Time.unscaledTime + updateInterval;
        ResolveNetworkManager();

        GameReferences refs = GameReferences.Instance;

        if (refs == null || refs.playerTransform == null)
        {
            cachedText = "";
            return;
        }

        InfrastructureNetworkManager network = networkManager != null ? networkManager : refs.infrastructureNetworkManager;
        FirstRunObjectiveManager objectiveManager = GetFirstRunObjectiveManager();

        if (network == null)
        {
            cachedText = "NETWORK OBJECTIVE\nNo city network signal detected";
            return;
        }

        string objectiveText = objectiveManager != null
            ? objectiveManager.GetObjectiveText(refs.playerTransform.position, refs.playerInventory)
            : network.GetObjectiveStatusText(refs.playerTransform.position, refs.playerInventory);

        string chainStatus = network.IsBaseCampRootOnline()
            ? "CHAIN: " + network.GetRequiredChainText()
            : "CHAIN: [dark] Root -> Signal -> Power -> Transit";

        cachedText =
            "LOCAL RESTORATION\n" +
            objectiveText +
            "\n" +
            chainStatus;
    }

    void ResolveNetworkManager()
    {
        if (networkManager != null)
        {
            return;
        }

        GameReferences refs = GameReferences.Instance;
        networkManager = refs != null ? refs.infrastructureNetworkManager : null;

        if (networkManager == null)
        {
            networkManager = FindFirstObjectByType<InfrastructureNetworkManager>();
        }
    }

    FirstRunObjectiveManager GetFirstRunObjectiveManager()
    {
        if (firstRunObjectiveManager == null)
        {
            firstRunObjectiveManager = FindFirstObjectByType<FirstRunObjectiveManager>();
        }

        return firstRunObjectiveManager;
    }

    void EnsureStyles()
    {
        if (panelStyle == null)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, new Color(0.015f, 0.025f, 0.03f, 0.72f));
            texture.Apply();

            panelStyle = new GUIStyle(GUI.skin.box);
            panelStyle.normal.background = texture;
        }

        float scale = GetHudScale();

        if (labelStyle == null)
        {
            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.normal.textColor = new Color(0.72f, 0.95f, 0.9f, 1f);
            labelStyle.wordWrap = true;
            labelStyle.richText = false;
        }

        labelStyle.fontSize = Mathf.RoundToInt(14f * scale);
    }

    float GetPanelHeight(float scale)
    {
        int lineCount = 1;

        for (int i = 0; i < cachedText.Length; i++)
        {
            if (cachedText[i] == '\n')
            {
                lineCount++;
            }
        }

        return Mathf.Clamp((34f + lineCount * 19f) * scale, 78f * scale, 176f * scale);
    }

    public Rect GetCurrentPanelRect()
    {
        float scale = GetHudScale();
        Rect safeArea = GetGuiSafeArea();
        Vector2 position = GetResponsivePosition(scale, safeArea);
        float width = Mathf.Min(panelSize.x * scale, safeArea.width * 0.58f);
        return new Rect(position.x, position.y, width, GetPanelHeight(scale));
    }

    float GetHudScale()
    {
        return Mathf.Clamp(GetGuiSafeArea().height / 840f, minScale, maxScale);
    }

    Vector2 GetResponsivePosition(float scale, Rect safeArea)
    {
        float margin = Mathf.Clamp(18f * scale, 10f, 24f);
        return new Vector2(safeArea.xMin + margin, safeArea.yMin + margin);
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
