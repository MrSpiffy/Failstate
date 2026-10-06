using UnityEngine;
using System.Collections;

public class SystemMessageUI : MonoBehaviour
{
    public static SystemMessageUI Instance { get; private set; }

    public Vector2 panelSize = new Vector2(520f, 92f);
    public float maxPanelHeight = 170f;
    public float defaultDuration = 5f;
    public float fadeDuration = 0.55f;

    private string currentMessage = "";
    private float messageEndTime = -999f;
    private float messageStartTime = -999f;
    private GUIStyle panelStyle;
    private GUIStyle labelStyle;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(PlayBootIntro());
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void ShowMessage(string message)
    {
        ShowMessage(message, defaultDuration);
    }

    public void ShowMessage(string message, float duration)
    {
        currentMessage = message;
        messageStartTime = Time.unscaledTime;
        messageEndTime = Time.unscaledTime + Mathf.Max(0.5f, duration);
    }

    void OnGUI()
    {
        if (string.IsNullOrEmpty(currentMessage) || Time.unscaledTime > messageEndTime + fadeDuration)
        {
            return;
        }

        if (UIStateManager.CurrentState == UIState.Pause)
        {
            return;
        }

        EnsureStyles();

        float alpha = GetCurrentAlpha();
        Color oldColor = GUI.color;
        GUI.color = new Color(1f, 1f, 1f, alpha);

        Rect safeArea = GetGuiSafeArea();
        float width = Mathf.Min(panelSize.x, safeArea.width - 32f);
        float contentHeight = labelStyle.CalcHeight(new GUIContent(currentMessage), width - 32f) + 24f;
        float height = Mathf.Clamp(contentHeight, panelSize.y, Mathf.Min(maxPanelHeight, safeArea.height * 0.34f));
        Rect panelRect = new Rect(
            safeArea.center.x - width * 0.5f,
            safeArea.yMin + 26f,
            width,
            height
        );

        GUI.Box(panelRect, GUIContent.none, panelStyle);
        GUI.Label(new Rect(panelRect.x + 16f, panelRect.y + 12f, panelRect.width - 32f, panelRect.height - 24f), currentMessage, labelStyle);

        GUI.color = oldColor;
    }

    float GetCurrentAlpha()
    {
        float fadeIn = Mathf.Clamp01((Time.unscaledTime - messageStartTime) / fadeDuration);
        float fadeOut = Mathf.Clamp01((messageEndTime - Time.unscaledTime) / fadeDuration);
        return Mathf.Min(fadeIn, fadeOut);
    }

    void EnsureStyles()
    {
        if (panelStyle == null)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, new Color(0.01f, 0.018f, 0.022f, 0.78f));
            texture.Apply();

            panelStyle = new GUIStyle(GUI.skin.box);
            panelStyle.normal.background = texture;
        }

        if (labelStyle == null)
        {
            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 15;
            labelStyle.normal.textColor = new Color(0.78f, 0.96f, 0.9f, 1f);
            labelStyle.wordWrap = true;
            labelStyle.alignment = TextAnchor.MiddleLeft;
        }
    }

    IEnumerator PlayBootIntro()
    {
        ShowMessage("BOOT SEQUENCE PARTIAL\nMotor stack responsive. Memory lattice damaged. Local network signal detected.", 5.5f);
        yield return new WaitForSecondsRealtime(5.8f);
        ShowMessage("ENVIRONMENTAL POWER TRICKLE DETECTED\nSolar breach and base camp relay are sustaining minimum function.", 5f);
        yield return new WaitForSecondsRealtime(5.2f);
        ShowMessage("PRIMARY DIRECTIVE CORRUPTED\nRestore local infrastructure chain to recover identity fragment.", 5f);
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
