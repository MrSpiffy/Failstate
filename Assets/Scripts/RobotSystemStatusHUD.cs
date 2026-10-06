using UnityEngine;

public class RobotSystemStatusHUD : MonoBehaviour
{
    public bool showHud = true;
    public Vector2 barSize = new Vector2(210f, 18f);
    public float bottomOffset = 24f;
    public float barSpacing = 18f;
    public float minScale = 0.68f;
    public float maxScale = 1f;

    public Color coreColor = new Color(0.95f, 0.25f, 0.18f, 1f);
    public Color mobilityColor = new Color(0.95f, 0.72f, 0.2f, 1f);
    public Color perceptionColor = new Color(0.35f, 0.65f, 1f, 1f);

    private PlayerCondition playerCondition;
    private GUIStyle panelStyle;
    private GUIStyle labelStyle;
    private GUIStyle valueStyle;
    private Texture2D backgroundTexture;
    private Texture2D fillTexture;

    void Start()
    {
        ResolvePlayerCondition();
    }

    void OnGUI()
    {
        if (!showHud)
        {
            return;
        }

        if (DevConsoleUI.IsConsoleOpen || UIStateManager.CurrentState == UIState.Pause || UIStateManager.CurrentState == UIState.GameOver)
        {
            return;
        }

        ResolvePlayerCondition();

        if (playerCondition == null)
        {
            return;
        }

        EnsureStyles();

        Rect safeArea = GetGuiSafeArea();
        float scale = GetHudScale(safeArea);
        Vector2 scaledBarSize = barSize * scale;
        float scaledSpacing = barSpacing * scale;
        float totalWidth = scaledBarSize.x * 3f + scaledSpacing * 2f;
        float startX = Mathf.Clamp(
            safeArea.center.x - totalWidth * 0.5f,
            safeArea.xMin + 12f * scale,
            Mathf.Max(safeArea.xMin + 12f * scale, safeArea.xMax - totalWidth - 12f * scale)
        );
        float y = safeArea.yMax - bottomOffset * scale - scaledBarSize.y - 18f * scale;

        DrawSystemBar(new Rect(startX, y, scaledBarSize.x, scaledBarSize.y), "CORE", PlayerSystemType.Core, coreColor, scale);
        DrawSystemBar(new Rect(startX + scaledBarSize.x + scaledSpacing, y, scaledBarSize.x, scaledBarSize.y), "MOBILITY", PlayerSystemType.Mobility, mobilityColor, scale);
        DrawSystemBar(new Rect(startX + (scaledBarSize.x + scaledSpacing) * 2f, y, scaledBarSize.x, scaledBarSize.y), "PERCEPTION", PlayerSystemType.Perception, perceptionColor, scale);
    }

    void DrawSystemBar(Rect barRect, string label, PlayerSystemType systemType, Color fillColor, float scale)
    {
        float percent = Mathf.Clamp01(playerCondition.GetSystemPercent(systemType));
        float current = GetCurrentValue(systemType);
        float max = GetMaxValue(systemType);

        labelStyle.fontSize = Mathf.RoundToInt(12f * scale);
        valueStyle.fontSize = labelStyle.fontSize;

        GUI.Box(new Rect(barRect.x - 8f * scale, barRect.y - 18f * scale, barRect.width + 16f * scale, barRect.height + 30f * scale), GUIContent.none, panelStyle);

        GUI.Label(new Rect(barRect.x, barRect.y - 17f * scale, barRect.width * 0.55f, 16f * scale), label, labelStyle);
        GUI.Label(new Rect(barRect.x + barRect.width * 0.45f, barRect.y - 17f * scale, barRect.width * 0.55f, 16f * scale), Mathf.CeilToInt(current) + "/" + Mathf.CeilToInt(max), valueStyle);

        Color oldColor = GUI.color;
        GUI.color = new Color(0.04f, 0.055f, 0.065f, 0.88f);
        GUI.DrawTexture(barRect, backgroundTexture);

        GUI.color = fillColor;
        GUI.DrawTexture(new Rect(barRect.x, barRect.y, barRect.width * percent, barRect.height), fillTexture);

        GUI.color = new Color(1f, 1f, 1f, 0.22f);
        GUI.DrawTexture(new Rect(barRect.x, barRect.y, barRect.width, Mathf.Max(1f, 2f * scale)), fillTexture);
        GUI.color = oldColor;
    }

    float GetCurrentValue(PlayerSystemType systemType)
    {
        switch (systemType)
        {
            case PlayerSystemType.Mobility:
                return playerCondition.currentMobilityIntegrity;

            case PlayerSystemType.Perception:
                return playerCondition.currentPerceptionIntegrity;

            default:
                return playerCondition.currentCoreIntegrity;
        }
    }

    float GetMaxValue(PlayerSystemType systemType)
    {
        switch (systemType)
        {
            case PlayerSystemType.Mobility:
                return playerCondition.maxMobilityIntegrity;

            case PlayerSystemType.Perception:
                return playerCondition.maxPerceptionIntegrity;

            default:
                return playerCondition.maxCoreIntegrity;
        }
    }

    void ResolvePlayerCondition()
    {
        if (playerCondition != null)
        {
            return;
        }

        GameReferences refs = GameReferences.Instance;
        playerCondition = refs != null ? refs.playerCondition : null;

        if (playerCondition == null)
        {
            playerCondition = FindFirstObjectByType<PlayerCondition>();
        }
    }

    void EnsureStyles()
    {
        if (backgroundTexture == null)
        {
            backgroundTexture = new Texture2D(1, 1);
            backgroundTexture.SetPixel(0, 0, Color.white);
            backgroundTexture.Apply();
        }

        if (fillTexture == null)
        {
            fillTexture = new Texture2D(1, 1);
            fillTexture.SetPixel(0, 0, Color.white);
            fillTexture.Apply();
        }

        if (panelStyle == null)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, new Color(0.01f, 0.015f, 0.018f, 0.52f));
            texture.Apply();

            panelStyle = new GUIStyle(GUI.skin.box);
            panelStyle.normal.background = texture;
        }

        if (labelStyle == null)
        {
            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 12;
            labelStyle.normal.textColor = new Color(0.78f, 0.9f, 0.88f, 1f);
            labelStyle.alignment = TextAnchor.MiddleLeft;
        }

        if (valueStyle == null)
        {
            valueStyle = new GUIStyle(labelStyle);
            valueStyle.alignment = TextAnchor.MiddleRight;
        }
    }

    float GetHudScale(Rect safeArea)
    {
        float widthScale = safeArea.width / 1280f;
        float heightScale = safeArea.height / 720f;
        float fitScale = (safeArea.width - 32f) / (barSize.x * 3f + barSpacing * 2f);
        return Mathf.Clamp(Mathf.Min(widthScale, heightScale, fitScale), minScale, maxScale);
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
