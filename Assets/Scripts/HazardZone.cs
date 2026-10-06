using UnityEngine;

public class HazardZone : MonoBehaviour
{
    public PlayerSystemType affectedSystem = PlayerSystemType.Core;
    public float damagePerSecond = 10f;
    public HazardZoneType hazardType = HazardZoneType.CorruptedSignal;

    [Header("Presentation")]
    public bool tintByAffectedSystem = true;
    public bool pulseWhenPlayerNearby = true;
    public float warningDistance = 7f;
    public float pulseSpeed = 4f;
    public float pulseScaleAmount = 0.12f;
    public Color coreColor = new Color(1f, 0.12f, 0.08f, 0.75f);
    public Color mobilityColor = new Color(1f, 0.68f, 0.12f, 0.75f);
    public Color perceptionColor = new Color(0.35f, 0.55f, 1f, 0.75f);

    private Renderer zoneRenderer;
    private MaterialPropertyBlock propertyBlock;
    private Vector3 baseScale;
    private InfrastructureNetworkManager networkManager;
    private bool playerInside = false;
    private float currentDamageMultiplier = 1f;
    private float nextContactMessageTime = -999f;

    void Awake()
    {
        zoneRenderer = GetComponent<Renderer>();
        networkManager = FindFirstObjectByType<InfrastructureNetworkManager>();
        baseScale = transform.localScale;
        ApplySystemTint();
    }

    void Update()
    {
        GameReferences refs = GameReferences.Instance;

        if (refs == null || refs.playerTransform == null)
        {
            transform.localScale = baseScale;
            return;
        }

        float distance = Vector3.Distance(transform.position, refs.playerTransform.position);
        float damageMultiplier = GetDamageMultiplier();

        if (!Mathf.Approximately(currentDamageMultiplier, damageMultiplier))
        {
            currentDamageMultiplier = damageMultiplier;
            ApplySystemTint();
        }

        if (distance <= warningDistance && refs.interactionPromptUI != null && UIStateManager.IsGameplay())
        {
            refs.interactionPromptUI.ShowPrompt(
                "WARNING: " + GetHazardDisplayName() + " - " + GetSystemWarningText() + GetSuppressionText(),
                gameObject,
                playerInside ? 6 : -10
            );
        }
        else if (refs.interactionPromptUI != null)
        {
            refs.interactionPromptUI.HidePrompt(gameObject);
        }

        if (!pulseWhenPlayerNearby || distance > warningDistance)
        {
            transform.localScale = baseScale;
            return;
        }

        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseScaleAmount;
        transform.localScale = new Vector3(baseScale.x * pulse, baseScale.y, baseScale.z * pulse);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        PlayerCondition playerCondition = other.GetComponent<PlayerCondition>();

        if (playerCondition != null)
        {
            if (!playerInside)
            {
                ShowHazardContactMessage();
            }

            playerInside = true;
            currentDamageMultiplier = GetDamageMultiplier();
            playerCondition.DamageSystem(affectedSystem, damagePerSecond * currentDamageMultiplier * Time.deltaTime);
            playerCondition.ApplyTemporaryHazardEffect(affectedSystem, Mathf.Clamp01(0.45f * currentDamageMultiplier));
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerInside = false;
        currentDamageMultiplier = GetDamageMultiplier();
    }

    void OnValidate()
    {
        if (Application.isPlaying)
        {
            ApplySystemTint();
        }
    }

    void ApplySystemTint()
    {
        if (zoneRenderer == null)
        {
            zoneRenderer = GetComponent<Renderer>();
        }

        if (!tintByAffectedSystem || zoneRenderer == null)
        {
            return;
        }

        ApplyRendererColor(zoneRenderer, GetSystemColor());
    }

    void ApplyRendererColor(Renderer targetRenderer, Color color)
    {
        if (propertyBlock == null)
        {
            propertyBlock = new MaterialPropertyBlock();
        }

        targetRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor("_BaseColor", color);
        propertyBlock.SetColor("_Color", color);
        targetRenderer.SetPropertyBlock(propertyBlock);
    }

    Color GetSystemColor()
    {
        Color baseColor;

        switch (affectedSystem)
        {
            case PlayerSystemType.Mobility:
                baseColor = mobilityColor;
                break;

            case PlayerSystemType.Perception:
                baseColor = perceptionColor;
                break;

            default:
                baseColor = coreColor;
                break;
        }

        if (currentDamageMultiplier >= 0.95f)
        {
            return baseColor;
        }

        float suppressedAmount = 1f - currentDamageMultiplier;
        Color stabilizedColor = new Color(0.18f, 0.65f, 0.5f, 0.38f);
        return Color.Lerp(baseColor, stabilizedColor, Mathf.Clamp01(suppressedAmount * 0.9f));
    }

    string GetHazardDisplayName()
    {
        switch (hazardType)
        {
            case HazardZoneType.CoolantLeak:
                return "coolant leak";

            case HazardZoneType.CorruptedSignal:
                return "corrupted signal";

            case HazardZoneType.UnstablePower:
                return "unstable power";

            default:
                return "electromagnetic interference";
        }
    }

    string GetSystemWarningText()
    {
        switch (affectedSystem)
        {
            case PlayerSystemType.Mobility:
                return "Mobility servos degrading";

            case PlayerSystemType.Perception:
                return "Perception sensors desynchronizing";

            default:
                return "Core integrity destabilizing";
        }
    }

    string GetSystemShortName()
    {
        switch (affectedSystem)
        {
            case PlayerSystemType.Mobility:
                return "Mobility";

            case PlayerSystemType.Perception:
                return "Perception";

            default:
                return "Core";
        }
    }

    float GetDamageMultiplier()
    {
        if (networkManager == null)
        {
            networkManager = FindFirstObjectByType<InfrastructureNetworkManager>();
        }

        if (networkManager == null)
        {
            return 1f;
        }

        return networkManager.GetHazardDamageMultiplier(transform.position);
    }

    string GetSuppressionText()
    {
        currentDamageMultiplier = GetDamageMultiplier();

        if (currentDamageMultiplier >= 0.95f)
        {
            return "";
        }

        return " - grid suppression " + Mathf.RoundToInt((1f - currentDamageMultiplier) * 100f) + "%";
    }

    void ShowHazardContactMessage()
    {
        if (Time.time < nextContactMessageTime)
        {
            return;
        }

        nextContactMessageTime = Time.time + 3.5f;

        if (SystemMessageUI.Instance == null)
        {
            return;
        }

        currentDamageMultiplier = GetDamageMultiplier();
        string severity = currentDamageMultiplier < 0.95f ? "SUPPRESSED " : "";
        SystemMessageUI.Instance.ShowMessage(
            severity + "HAZARD CONTACT\n" +
            GetHazardDisplayName().ToUpperInvariant() + " damaging " + GetSystemShortName() + " integrity.",
            3f
        );
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
