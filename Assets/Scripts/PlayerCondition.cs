using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerCondition : MonoBehaviour
{
    [Header("Core")]
    public float maxCoreIntegrity = 100f;
    public float currentCoreIntegrity = 100f;
    public float coreDecayRate = 0.45f;

    [Header("Mobility")]
    public float maxMobilityIntegrity = 100f;
    public float currentMobilityIntegrity = 100f;
    public float mobilityDecayRate = 0.65f;

    [Header("Perception")]
    public float maxPerceptionIntegrity = 100f;
    public float currentPerceptionIntegrity = 100f;
    public float perceptionDecayRate = 0.5f;

    [Header("Failure")]
    public bool showDefaultFailureOverlay = true;
    public KeyCode restartKey = KeyCode.R;

    public bool passiveDecayEnabled = true;

    public event Action OnCoreFailed;

    private bool hasFailed = false;
    private UIStateManager uiStateManager;
    private InfrastructureNetworkManager infrastructureNetworkManager;
    private float temporaryMobilityMultiplier = 1f;
    private float temporaryPerceptionMultiplier = 1f;
    private float temporaryMobilityEffectUntil = -999f;
    private float temporaryPerceptionEffectUntil = -999f;

    void Start()
    {
        uiStateManager = FindFirstObjectByType<UIStateManager>();
        infrastructureNetworkManager = FindFirstObjectByType<InfrastructureNetworkManager>();
        FullyRestoreAllSystems();
    }

    void Update()
    {
        if (hasFailed)
        {
            HandleFailureInput();
            return;
        }

        if (passiveDecayEnabled)
        {
            float decayMultiplier = GetNetworkPassiveDecayMultiplier();
            DamageSystem(PlayerSystemType.Core, coreDecayRate * decayMultiplier * Time.deltaTime);
            DamageSystem(PlayerSystemType.Mobility, mobilityDecayRate * decayMultiplier * Time.deltaTime);
            DamageSystem(PlayerSystemType.Perception, perceptionDecayRate * decayMultiplier * Time.deltaTime);
        }

        if (currentCoreIntegrity <= 0f)
        {
            TriggerCoreFailure();
        }

    }

    void TriggerCoreFailure()
    {
        if (hasFailed)
        {
            return;
        }

        hasFailed = true;
        passiveDecayEnabled = false;
        currentCoreIntegrity = 0f;

        if (uiStateManager == null)
        {
            uiStateManager = FindFirstObjectByType<UIStateManager>();
        }

        if (uiStateManager != null)
        {
            uiStateManager.SetState(UIState.GameOver);
        }

        OnCoreFailed?.Invoke();
        Debug.Log("Core integrity failed.");
    }

    void HandleFailureInput()
    {
        if (Input.GetKeyDown(restartKey))
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    void OnGUI()
    {
        if (!showDefaultFailureOverlay || !hasFailed)
        {
            return;
        }

        float width = 420f;
        float height = 180f;
        Rect boxRect = new Rect(
            (Screen.width - width) * 0.5f,
            (Screen.height - height) * 0.5f,
            width,
            height
        );

        GUI.Box(boxRect, "");
        GUI.Label(new Rect(boxRect.x + 24f, boxRect.y + 24f, width - 48f, 36f), "CORE FAILURE");
        GUI.Label(
            new Rect(boxRect.x + 24f, boxRect.y + 66f, width - 48f, 64f),
            "Your chassis has gone dark before you could stabilize the signal."
        );
        GUI.Label(
            new Rect(boxRect.x + 24f, boxRect.y + 130f, width - 48f, 32f),
            "Press " + restartKey + " to restart the run."
        );
    }

    public void DamageSystem(PlayerSystemType systemType, float amount)
    {
        if (hasFailed)
        {
            return;
        }

        switch (systemType)
        {
            case PlayerSystemType.Core:
                currentCoreIntegrity = Mathf.Clamp(currentCoreIntegrity - amount, 0f, maxCoreIntegrity);
                break;

            case PlayerSystemType.Mobility:
                currentMobilityIntegrity = Mathf.Clamp(currentMobilityIntegrity - amount, 0f, maxMobilityIntegrity);
                break;

            case PlayerSystemType.Perception:
                currentPerceptionIntegrity = Mathf.Clamp(currentPerceptionIntegrity - amount, 0f, maxPerceptionIntegrity);
                break;
        }
    }

    public void RestoreSystem(PlayerSystemType systemType, float amount)
    {
        switch (systemType)
        {
            case PlayerSystemType.Core:
                currentCoreIntegrity = Mathf.Clamp(currentCoreIntegrity + amount, 0f, maxCoreIntegrity);
                break;

            case PlayerSystemType.Mobility:
                currentMobilityIntegrity = Mathf.Clamp(currentMobilityIntegrity + amount, 0f, maxMobilityIntegrity);
                break;

            case PlayerSystemType.Perception:
                currentPerceptionIntegrity = Mathf.Clamp(currentPerceptionIntegrity + amount, 0f, maxPerceptionIntegrity);
                break;
        }
    }

    public bool IsSystemFull(PlayerSystemType systemType)
    {
        switch (systemType)
        {
            case PlayerSystemType.Core:
                return currentCoreIntegrity >= maxCoreIntegrity;

            case PlayerSystemType.Mobility:
                return currentMobilityIntegrity >= maxMobilityIntegrity;

            case PlayerSystemType.Perception:
                return currentPerceptionIntegrity >= maxPerceptionIntegrity;

            default:
                return true;
        }
    }

    public float GetSystemPercent(PlayerSystemType systemType)
    {
        switch (systemType)
        {
            case PlayerSystemType.Core:
                return maxCoreIntegrity <= 0f ? 1f : currentCoreIntegrity / maxCoreIntegrity;

            case PlayerSystemType.Mobility:
                return maxMobilityIntegrity <= 0f ? 1f : currentMobilityIntegrity / maxMobilityIntegrity;

            case PlayerSystemType.Perception:
                return maxPerceptionIntegrity <= 0f ? 1f : currentPerceptionIntegrity / maxPerceptionIntegrity;

            default:
                return 1f;
        }
    }

    public float GetMobilityPercent()
    {
        return GetSystemPercent(PlayerSystemType.Mobility);
    }

    public float GetEffectiveMobilityPercent()
    {
        float multiplier = Time.time <= temporaryMobilityEffectUntil ? temporaryMobilityMultiplier : 1f;
        return Mathf.Clamp01(GetSystemPercent(PlayerSystemType.Mobility) * multiplier);
    }

    public float GetEffectivePerceptionPercent()
    {
        float multiplier = Time.time <= temporaryPerceptionEffectUntil ? temporaryPerceptionMultiplier : 1f;
        return Mathf.Clamp01(GetSystemPercent(PlayerSystemType.Perception) * multiplier);
    }

    public float GetMovementSpeedMultiplier(float minimumMultiplier)
    {
        return Mathf.Lerp(minimumMultiplier, 1f, GetEffectiveMobilityPercent());
    }

    public void ApplyTemporaryHazardEffect(PlayerSystemType systemType, float strength)
    {
        float multiplier = Mathf.Clamp01(1f - strength);

        if (systemType == PlayerSystemType.Mobility)
        {
            if (Time.time > temporaryMobilityEffectUntil)
            {
                temporaryMobilityMultiplier = 1f;
            }

            temporaryMobilityMultiplier = Mathf.Min(temporaryMobilityMultiplier, Mathf.Lerp(0.45f, 1f, multiplier));
            temporaryMobilityEffectUntil = Time.time + 0.2f;
        }
        else if (systemType == PlayerSystemType.Perception)
        {
            if (Time.time > temporaryPerceptionEffectUntil)
            {
                temporaryPerceptionMultiplier = 1f;
            }

            temporaryPerceptionMultiplier = Mathf.Min(temporaryPerceptionMultiplier, Mathf.Lerp(0.55f, 1f, multiplier));
            temporaryPerceptionEffectUntil = Time.time + 0.2f;
        }
    }

    public void FullyRestoreAllSystems()
    {
        hasFailed = false;
        currentCoreIntegrity = maxCoreIntegrity;
        currentMobilityIntegrity = maxMobilityIntegrity;
        currentPerceptionIntegrity = maxPerceptionIntegrity;
    }

    public bool HasFailed()
    {
        return hasFailed;
    }

    float GetNetworkPassiveDecayMultiplier()
    {
        if (infrastructureNetworkManager == null)
        {
            infrastructureNetworkManager = FindFirstObjectByType<InfrastructureNetworkManager>();
        }

        if (infrastructureNetworkManager == null)
        {
            return 1f;
        }

        return infrastructureNetworkManager.GetPassiveDecayMultiplier(transform.position);
    }
}
