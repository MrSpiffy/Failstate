using UnityEngine;
using TMPro;

public class InteractionPromptUI : MonoBehaviour
{
    public GameObject promptObject;
    public TextMeshProUGUI promptText;

    private GameObject currentOwner;
    private int currentPriority = int.MinValue;
    private float currentDistance = float.MaxValue;

    void Awake()
    {
        ApplyPromptPresentation();
    }

    public void ShowPrompt(string message, GameObject owner, int priority = 0)
    {
        ApplyPromptPresentation();
        float distance = GetDistanceFromPlayer(owner);

        if (currentOwner != null && currentOwner != owner)
        {
            bool lowerPriority = priority < currentPriority;
            bool samePriorityFarther = priority == currentPriority && distance > currentDistance;

            if (lowerPriority || samePriorityFarther)
            {
                return;
            }
        }

        currentOwner = owner;
        currentPriority = priority;
        currentDistance = distance;

        if (promptText != null)
        {
            promptText.text = message;
        }

        if (promptObject != null)
        {
            promptObject.SetActive(true);
        }
    }

    public void HidePrompt(GameObject owner)
    {
        if (currentOwner != owner)
        {
            return;
        }

        currentOwner = null;
        currentPriority = int.MinValue;
        currentDistance = float.MaxValue;

        if (promptObject != null)
        {
            promptObject.SetActive(false);
        }
    }

    public void HideAllPrompts()
    {
        currentOwner = null;
        currentPriority = int.MinValue;
        currentDistance = float.MaxValue;

        if (promptObject != null)
        {
            promptObject.SetActive(false);
        }
    }

    float GetDistanceFromPlayer(GameObject owner)
    {
        GameReferences refs = GameReferences.Instance;

        if (owner == null || refs == null || refs.playerTransform == null)
        {
            return float.MaxValue;
        }

        return Vector3.Distance(owner.transform.position, refs.playerTransform.position);
    }

    void ApplyPromptPresentation()
    {
        if (promptText == null)
        {
            return;
        }

        promptText.color = new Color(0.86f, 0.95f, 0.92f, 0.95f);
        promptText.fontSize = Mathf.Max(promptText.fontSize, 20f);
        promptText.alignment = TextAlignmentOptions.Center;
        promptText.textWrappingMode = TextWrappingModes.Normal;
        promptText.outlineColor = new Color(0f, 0f, 0f, 0.82f);
        promptText.outlineWidth = 0.18f;
    }
}
