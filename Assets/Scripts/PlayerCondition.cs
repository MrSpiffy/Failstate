using UnityEngine;

public class PlayerCondition : MonoBehaviour
{
    [Header("Core")]
    public float maxCoreIntegrity = 100f;
    public float currentCoreIntegrity = 100f;
    public float coreDecayRate = 1f;
    public float coreRepairAmount = 20f;

    [Header("Mobility")]
    public float maxMobilityIntegrity = 100f;
    public float currentMobilityIntegrity = 100f;
    public float mobilityDecayRate = 1.5f;
    public float mobilityRepairAmount = 20f;

    [Header("Perception")]
    public float maxPerceptionIntegrity = 100f;
    public float currentPerceptionIntegrity = 100f;
    public float perceptionDecayRate = 0.75f;
    public float perceptionRepairAmount = 20f;

    void Start()
    {
        currentCoreIntegrity = maxCoreIntegrity;
        currentMobilityIntegrity = maxMobilityIntegrity;
        currentPerceptionIntegrity = maxPerceptionIntegrity;
    }

    void Update()
{
    currentCoreIntegrity -= coreDecayRate * Time.deltaTime;
    currentMobilityIntegrity -= mobilityDecayRate * Time.deltaTime;
    currentPerceptionIntegrity -= perceptionDecayRate * Time.deltaTime;

    currentCoreIntegrity = Mathf.Clamp(currentCoreIntegrity, 0f, maxCoreIntegrity);
    currentMobilityIntegrity = Mathf.Clamp(currentMobilityIntegrity, 0f, maxMobilityIntegrity);
    currentPerceptionIntegrity = Mathf.Clamp(currentPerceptionIntegrity, 0f, maxPerceptionIntegrity);

    if (currentCoreIntegrity <= 0f)
    {
        Debug.Log("Core integrity failed.");
    }
}

    public void RepairCore()
    {
        currentCoreIntegrity += coreRepairAmount;
        currentCoreIntegrity = Mathf.Clamp(currentCoreIntegrity, 0f, maxCoreIntegrity);
    }

    public void RepairMobility()
    {
        currentMobilityIntegrity += mobilityRepairAmount;
        currentMobilityIntegrity = Mathf.Clamp(currentMobilityIntegrity, 0f, maxMobilityIntegrity);
    }

    public void RepairPerception()
    {
        currentPerceptionIntegrity += perceptionRepairAmount;
        currentPerceptionIntegrity = Mathf.Clamp(currentPerceptionIntegrity, 0f, maxPerceptionIntegrity);
    }

    public void DamageSystem(HazardSystemType systemType, float amount)
    {
        switch (systemType)
        {
            case HazardSystemType.Core:
                currentCoreIntegrity -= amount;
                currentCoreIntegrity = Mathf.Clamp(currentCoreIntegrity, 0f, maxCoreIntegrity);
                break;

            case HazardSystemType.Mobility:
                currentMobilityIntegrity -= amount;
                currentMobilityIntegrity = Mathf.Clamp(currentMobilityIntegrity, 0f, maxMobilityIntegrity);
                break;

            case HazardSystemType.Perception:
                currentPerceptionIntegrity -= amount;
                currentPerceptionIntegrity = Mathf.Clamp(currentPerceptionIntegrity, 0f, maxPerceptionIntegrity);
                break;
        }
    }

    public float GetMobilityPercent()
    {
        if (maxMobilityIntegrity <= 0f) return 1f;
        return currentMobilityIntegrity / maxMobilityIntegrity;
    }

    public void FullyRestoreAllSystems()
{
    currentCoreIntegrity = maxCoreIntegrity;
    currentMobilityIntegrity = maxMobilityIntegrity;
    currentPerceptionIntegrity = maxPerceptionIntegrity;
}
}