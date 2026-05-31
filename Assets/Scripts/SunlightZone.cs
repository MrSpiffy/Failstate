using UnityEngine;

public class SunlightZone : MonoBehaviour
{
    public Vector2Int cell;
    public string displayName = "Sunlight Breach";
    public float zoneRadius = 5f;
    public float coreRestorePerSecond = 0.75f;
    public float perceptionRestorePerSecond = 0.35f;
    public float revealRadius = 16f;

    private bool playerInside = false;
    private bool hasRevealedLandmark = false;

    void Start()
    {
        Collider collider = GetComponent<Collider>();

        if (collider != null)
        {
            collider.isTrigger = true;
        }
    }

    void Update()
    {
        GameReferences refs = GameReferences.Instance;

        if (refs == null || refs.playerTransform == null)
        {
            return;
        }

        float distance = Vector3.Distance(transform.position, refs.playerTransform.position);
        bool inRange = distance <= zoneRadius;

        if (!inRange)
        {
            if (playerInside && refs.interactionPromptUI != null)
            {
                refs.interactionPromptUI.HidePrompt(gameObject);
            }

            playerInside = false;
            return;
        }

        playerInside = true;

        if (!hasRevealedLandmark && refs.minimapUI != null)
        {
            hasRevealedLandmark = true;
            refs.minimapUI.RevealAroundWorldPosition(transform.position, revealRadius);
        }

        if (refs.playerCondition != null && !refs.playerCondition.HasFailed())
        {
            refs.playerCondition.RestoreSystem(PlayerSystemType.Core, coreRestorePerSecond * Time.deltaTime);
            refs.playerCondition.RestoreSystem(PlayerSystemType.Perception, perceptionRestorePerSecond * Time.deltaTime);
        }

        if (refs.interactionPromptUI != null && UIStateManager.IsGameplay())
        {
            refs.interactionPromptUI.ShowPrompt(
                displayName + " // Core + Perception stabilized",
                gameObject,
                -5
            );
        }
    }

    void OnDisable()
    {
        GameReferences refs = GameReferences.Instance;

        if (refs != null && refs.interactionPromptUI != null)
        {
            refs.interactionPromptUI.HidePrompt(gameObject);
        }
    }
}
