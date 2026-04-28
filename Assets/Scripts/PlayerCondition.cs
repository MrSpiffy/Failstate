using UnityEngine;

public class PlayerCondition : MonoBehaviour
{
    [Header("Core")]
    public float maxCoreIntegrity = 100f;
    public float currentCoreIntegrity = 100f;
    public float coreDecayRate = 1f;

    [Header("Mobility")]
    public float maxMobilityIntegrity = 100f;
    public float currentMobilityIntegrity = 100f;
    public float mobilityDecayRate = 1.5f;

    [Header("Perception")]
    public float maxPerceptionIntegrity = 100f;
    public float currentPerceptionIntegrity = 100f;
    public float perceptionDecayRate = 0.75f;

    void Start()
    {
        FullyRestoreAllSystems();
    }

    void Update()
    {
        DamageSystem(PlayerSystemType.Core, coreDecayRate * Time.deltaTime);
        DamageSystem(PlayerSystemType.Mobility, mobilityDecayRate * Time.deltaTime);
        DamageSystem(PlayerSystemType.Perception, perceptionDecayRate * Time.deltaTime);

        if (currentCoreIntegrity <= 0f)
        {
            Debug.Log("Core integrity failed.");
        }
    }

    public void DamageSystem(PlayerSystemType systemType, float amount)
    {
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

    public void FullyRestoreAllSystems()
    {
        currentCoreIntegrity = maxCoreIntegrity;
        currentMobilityIntegrity = maxMobilityIntegrity;
        currentPerceptionIntegrity = maxPerceptionIntegrity;
    }
}