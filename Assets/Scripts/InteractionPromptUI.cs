using UnityEngine;
using TMPro;

public class InteractionPromptUI : MonoBehaviour
{
    public GameObject promptObject;
    public TextMeshProUGUI promptText;

    private GameObject currentOwner;

    public void ShowPrompt(string message, GameObject owner)
    {
        currentOwner = owner;

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

        if (promptObject != null)
        {
            promptObject.SetActive(false);
        }
    }

    public void HideAllPrompts()
    {
        currentOwner = null;

        if (promptObject != null)
        {
            promptObject.SetActive(false);
        }
    }
}