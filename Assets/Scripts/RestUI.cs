using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RestUI : MonoBehaviour
{
    public GameObject fadePanelObject;
    public Image fadePanelImage;
    public GameObject messageObject;
    public TextMeshProUGUI messageText;

    public float fadeDuration = 0.75f;
    public float holdDuration = 1.25f;

    public IEnumerator PlayRestSequence(PlayerCondition playerCondition)
    {
        if (fadePanelObject != null)
        {
            fadePanelObject.SetActive(true);
        }

        if (messageObject != null)
        {
            messageObject.SetActive(false);
        }

        SetFadeAlpha(0f);

        yield return FadeToAlpha(1f);

        if (playerCondition != null)
        {
            playerCondition.FullyRestoreAllSystems();
        }

        if (messageText != null)
        {
            messageText.text = "Systems restored.\nData archived.\nPower reserves stabilized.";
        }

        if (messageObject != null)
        {
            messageObject.SetActive(true);
        }

        yield return new WaitForSecondsRealtime(holdDuration);

        if (messageObject != null)
        {
            messageObject.SetActive(false);
        }

        yield return FadeToAlpha(0f);

        if (fadePanelObject != null)
        {
            fadePanelObject.SetActive(false);
        }
    }

    IEnumerator FadeToAlpha(float targetAlpha)
    {
        if (fadePanelImage == null)
        {
            yield break;
        }

        float startAlpha = fadePanelImage.color.a;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / fadeDuration;

            SetFadeAlpha(Mathf.Lerp(startAlpha, targetAlpha, t));

            yield return null;
        }

        SetFadeAlpha(targetAlpha);
    }

    void SetFadeAlpha(float alpha)
    {
        if (fadePanelImage == null) return;

        Color color = fadePanelImage.color;
        color.a = alpha;
        fadePanelImage.color = color;
    }
}