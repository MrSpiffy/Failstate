using System.Collections.Generic;
using UnityEngine;

public class InfrastructureNetworkManager : MonoBehaviour
{
    [Header("Prototype Deep-City Signal")]
    public float deepSignalDistance = 18f;
    public float deepSignalBeaconHeight = 6f;
    public float deepSignalMinimumSeparation = 7f;
    public float deepSignalPromptDistance = 5f;
    public int deepSignalEdgeBandCells = 2;
    public Color deepSignalColor = new Color(0.7f, 0.42f, 1f, 1f);

    private readonly List<InfrastructureNode> nodes = new List<InfrastructureNode>();
    private readonly InfrastructureNodeType[] requiredChain =
    {
        InfrastructureNodeType.SignalRelay,
        InfrastructureNodeType.PowerJunction,
        InfrastructureNodeType.TransitLift
    };

    private bool baseCampRootOnline = false;
    private InfrastructureNode lastRestoredNode;
    private bool localChainLockedAtBase = false;
    private bool deepCitySignalDetected = false;
    private Vector3 deepCitySignalPosition;
    private Vector3 deepCitySignalApproachDirection = Vector3.back;
    private GameObject deepCitySignalBeacon;
    private GeneratedWorldSpawner generatedWorldSpawner;
    private CityBlockoutGenerator cityGenerator;

    void Update()
    {
        UpdateDeepSignalPrompt();
    }

    public void RegisterNode(InfrastructureNode node)
    {
        if (node != null && !nodes.Contains(node))
        {
            nodes.Add(node);
        }
    }

    public void UnregisterNode(InfrastructureNode node)
    {
        if (node != null)
        {
            nodes.Remove(node);
        }
    }

    public void HandleNodeRestored(InfrastructureNode node)
    {
        if (node == null)
        {
            return;
        }

        lastRestoredNode = node;

        if (node.nodeType == InfrastructureNodeType.TransitLift)
        {
            ActivateDeepCitySignal(node.transform.position);
        }

        Debug.Log(node.GetDisplayName() + " restored. " + node.GetSystemMessage());
    }

    public void SetBaseCampRootOnline(bool online)
    {
        baseCampRootOnline = online;
        Debug.Log(online ? "Base Camp root relay online." : "Base Camp root relay offline.");
    }

    public bool IsBaseCampRootOnline()
    {
        return baseCampRootOnline;
    }

    public void LockLocalChainAtBase()
    {
        if (baseCampRootOnline && IsRequiredChainComplete())
        {
            localChainLockedAtBase = true;
        }
    }

    public bool IsLocalChainLockedAtBase()
    {
        return localChainLockedAtBase;
    }

    public bool HasDeepCitySignal()
    {
        return deepCitySignalDetected;
    }

    public Vector3 GetDeepCitySignalPosition()
    {
        return deepCitySignalPosition;
    }

    public float GetSignalRangeMultiplier()
    {
        float multiplier = baseCampRootOnline ? 1.25f : 1f;

        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i] != null && nodes[i].restored && nodes[i].nodeType == InfrastructureNodeType.SignalRelay)
            {
                multiplier += 0.35f;
            }
        }

        return Mathf.Clamp(multiplier, 0.65f, 2.75f);
    }

    public float GetRevealRangeMultiplier()
    {
        float multiplier = baseCampRootOnline ? 1.15f : 1f;

        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i] != null && nodes[i].restored && nodes[i].nodeType == InfrastructureNodeType.SignalRelay)
            {
                multiplier += 0.18f;
            }
        }

        return Mathf.Clamp(multiplier, 0.75f, 2.25f);
    }

    public float GetHazardDamageMultiplier(Vector3 hazardPosition)
    {
        float multiplier = 1f;

        for (int i = 0; i < nodes.Count; i++)
        {
            InfrastructureNode node = nodes[i];

            if (node == null || !node.restored || node.nodeType != InfrastructureNodeType.PowerJunction)
            {
                continue;
            }

            float distance = Vector3.Distance(hazardPosition, node.transform.position);

            if (distance <= node.localStabilityRadius)
            {
                multiplier *= 0.22f;
            }
        }

        return Mathf.Clamp(multiplier, 0.2f, 1f);
    }

    public float GetPassiveDecayMultiplier()
    {
        return GetPassiveDecayMultiplier(null);
    }

    public float GetPassiveDecayMultiplier(Vector3? playerPosition)
    {
        float multiplier = baseCampRootOnline ? 0.94f : 1f;

        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i] != null && nodes[i].restored && nodes[i].nodeType == InfrastructureNodeType.TransitLift)
            {
                multiplier *= 0.86f;
            }

            if (playerPosition.HasValue && nodes[i] != null && nodes[i].restored && nodes[i].nodeType == InfrastructureNodeType.PowerJunction)
            {
                float distance = Vector3.Distance(playerPosition.Value, nodes[i].transform.position);

                if (distance <= nodes[i].localStabilityRadius)
                {
                    multiplier *= 0.68f;
                }
            }
        }

        if (playerPosition.HasValue && IsPlayerInsideRestoredDistrict(playerPosition.Value))
        {
            multiplier *= 0.75f;
        }

        return Mathf.Clamp(multiplier, 0.42f, 1f);
    }

    bool IsPlayerInsideRestoredDistrict(Vector3 playerPosition)
    {
        if (generatedWorldSpawner == null)
        {
            generatedWorldSpawner = FindFirstObjectByType<GeneratedWorldSpawner>();
        }

        if (cityGenerator == null)
        {
            GameReferences refs = GameReferences.Instance;
            cityGenerator = refs != null ? refs.cityGenerator : null;
        }

        if (cityGenerator == null)
        {
            cityGenerator = FindFirstObjectByType<CityBlockoutGenerator>();
        }

        if (generatedWorldSpawner == null || cityGenerator == null)
        {
            return false;
        }

        if (!cityGenerator.TryWorldToCell(playerPosition, out Vector2Int cell))
        {
            return false;
        }

        return generatedWorldSpawner.IsRestoredDistrictCell(cell.x, cell.y);
    }

    public InfrastructureNode GetNearestUnrestoredNode(Vector3 position)
    {
        InfrastructureNode chainNode = GetCurrentRequiredNode(position);

        if (chainNode != null)
        {
            return chainNode;
        }

        InfrastructureNode nearest = null;
        float nearestDistance = float.MaxValue;

        for (int i = 0; i < nodes.Count; i++)
        {
            InfrastructureNode node = nodes[i];

            if (node == null || node.restored)
            {
                continue;
            }

            float distance = Vector3.Distance(position, node.transform.position);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = node;
            }
        }

        return nearest;
    }

    public int GetNodeCount()
    {
        return nodes.Count;
    }

    public InfrastructureNode GetNode(int index)
    {
        if (index < 0 || index >= nodes.Count)
        {
            return null;
        }

        return nodes[index];
    }

    public float GetNearestUnrestoredNodeDistance(Vector3 position)
    {
        InfrastructureNode nearest = GetNearestUnrestoredNode(position);

        if (nearest == null)
        {
            return -1f;
        }

        return Vector3.Distance(position, nearest.transform.position);
    }

    public InfrastructureNode GetLastRestoredNode()
    {
        return lastRestoredNode;
    }

    public bool CanRestoreNode(InfrastructureNode node)
    {
        if (node == null || node.restored)
        {
            return false;
        }

        if (!baseCampRootOnline)
        {
            return false;
        }

        InfrastructureNodeType? nextRequiredType = GetNextRequiredNodeType();

        if (!nextRequiredType.HasValue)
        {
            return true;
        }

        if (node.nodeType != nextRequiredType.Value)
        {
            return false;
        }

        InfrastructureNode requiredLandmark = GetUnrestoredRequiredLandmark(nextRequiredType.Value);
        return requiredLandmark == null || node == requiredLandmark;
    }

    public string GetNodeLockReason(InfrastructureNode node)
    {
        if (node == null)
        {
            return "No network handshake available";
        }

        if (node.restored)
        {
            return node.GetDisplayName() + " already restored";
        }

        if (!baseCampRootOnline)
        {
            return "Restore Base Camp root relay first";
        }

        InfrastructureNodeType? nextRequiredType = GetNextRequiredNodeType();

        if (nextRequiredType.HasValue && node.nodeType != nextRequiredType.Value)
        {
            return "Network sequence requires " + GetNodeTypeDisplayName(nextRequiredType.Value) + " first";
        }

        InfrastructureNode requiredLandmark = nextRequiredType.HasValue
            ? GetUnrestoredRequiredLandmark(nextRequiredType.Value)
            : null;

        if (requiredLandmark != null && node != requiredLandmark)
        {
            return "Primary local-grid " + GetNodeTypeDisplayName(nextRequiredType.Value) + " must be restored first";
        }

        return "";
    }

    public InfrastructureNode GetCurrentRequiredNode(Vector3 position)
    {
        InfrastructureNodeType? nextRequiredType = GetNextRequiredNodeType();

        if (!nextRequiredType.HasValue)
        {
            return null;
        }

        InfrastructureNode requiredLandmark = GetUnrestoredRequiredLandmark(nextRequiredType.Value);
        return requiredLandmark != null
            ? requiredLandmark
            : GetNearestUnrestoredNodeOfType(position, nextRequiredType.Value);
    }

    public InfrastructureNodeType? GetNextRequiredNodeType()
    {
        for (int i = 0; i < requiredChain.Length; i++)
        {
            if (!HasRestoredNodeType(requiredChain[i]))
            {
                return requiredChain[i];
            }
        }

        return null;
    }

    public int GetRequiredChainProgress()
    {
        int progress = 0;

        for (int i = 0; i < requiredChain.Length; i++)
        {
            if (HasRestoredNodeType(requiredChain[i]))
            {
                progress++;
            }
        }

        return progress;
    }

    public int GetRequiredChainLength()
    {
        return requiredChain.Length;
    }

    public bool IsRequiredChainComplete()
    {
        return GetNextRequiredNodeType() == null;
    }

    public string GetRequiredChainText()
    {
        string text = "";

        for (int i = 0; i < requiredChain.Length; i++)
        {
            if (i > 0)
            {
                text += " -> ";
            }

            text += HasRestoredNodeType(requiredChain[i]) ? "[online] " : "[dark] ";
            text += GetNodeTypeShortName(requiredChain[i]);
        }

        return text;
    }

    public bool HasRestoredNodeType(InfrastructureNodeType nodeType)
    {
        return GetRestoredNodeCount(nodeType) > 0;
    }

    public int GetRestoredNodeCount()
    {
        int count = 0;

        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i] != null && nodes[i].restored)
            {
                count++;
            }
        }

        return count;
    }

    public int GetRestoredNodeCount(InfrastructureNodeType nodeType)
    {
        int count = 0;

        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i] != null && nodes[i].restored && nodes[i].nodeType == nodeType)
            {
                count++;
            }
        }

        return count;
    }

    public int GetTotalNodeCount()
    {
        int count = 0;

        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i] != null)
            {
                count++;
            }
        }

        return count;
    }

    public int GetTotalNodeCount(InfrastructureNodeType nodeType)
    {
        int count = 0;

        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i] != null && nodes[i].nodeType == nodeType)
            {
                count++;
            }
        }

        return count;
    }

    public string GetNetworkStatusText()
    {
        return
            "Network nodes: " + GetRestoredNodeCount() + "/" + GetTotalNodeCount() + "\n" +
            "Signal relays: " + GetRestoredNodeCount(InfrastructureNodeType.SignalRelay) + "/" + GetTotalNodeCount(InfrastructureNodeType.SignalRelay) +
            "  Power: " + GetRestoredNodeCount(InfrastructureNodeType.PowerJunction) + "/" + GetTotalNodeCount(InfrastructureNodeType.PowerJunction) +
            "  Transit: " + GetRestoredNodeCount(InfrastructureNodeType.TransitLift) + "/" + GetTotalNodeCount(InfrastructureNodeType.TransitLift);
    }

    public string GetObjectiveStatusText(Vector3 playerPosition, PlayerInventory inventory)
    {
        if (GetTotalNodeCount() == 0)
        {
            return "OBJECTIVE: locate broken infrastructure signals";
        }

        if (!baseCampRootOnline)
        {
            return "OBJECTIVE: restore Base Camp root relay";
        }

        InfrastructureNode nearest = GetCurrentRequiredNode(playerPosition);

        if (nearest == null)
        {
            return "OBJECTIVE: local infrastructure chain online - push deeper into the city";
        }

        string affordability = inventory != null && inventory.CanAfford(nearest.repairCosts)
            ? "resources ready"
            : "needs " + nearest.GetCostText();

        return
            "OBJECTIVE: restore " + nearest.GetDisplayName() + "\n" +
            nearest.GetEffectSummary() + "\n" +
            Mathf.RoundToInt(Vector3.Distance(playerPosition, nearest.transform.position)) + "m away - " + affordability;
    }

    InfrastructureNode GetNearestUnrestoredNodeOfType(Vector3 position, InfrastructureNodeType nodeType)
    {
        InfrastructureNode nearest = null;
        float nearestDistance = float.MaxValue;

        for (int i = 0; i < nodes.Count; i++)
        {
            InfrastructureNode node = nodes[i];

            if (node == null || node.restored || node.nodeType != nodeType)
            {
                continue;
            }

            float distance = Vector3.Distance(position, node.transform.position);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = node;
            }
        }

        return nearest;
    }

    InfrastructureNode GetUnrestoredRequiredLandmark(InfrastructureNodeType nodeType)
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            InfrastructureNode node = nodes[i];

            if (node != null && !node.restored && node.requiredChainLandmark && node.nodeType == nodeType)
            {
                return node;
            }
        }

        return null;
    }

    string GetNodeTypeDisplayName(InfrastructureNodeType nodeType)
    {
        switch (nodeType)
        {
            case InfrastructureNodeType.PowerJunction:
                return "Power Junction";

            case InfrastructureNodeType.TransitLift:
                return "Transit/Deep Relay";

            case InfrastructureNodeType.BaseCampRoot:
                return "Base Camp Root Relay";

            default:
                return "Signal Relay";
        }
    }

    string GetNodeTypeShortName(InfrastructureNodeType nodeType)
    {
        switch (nodeType)
        {
            case InfrastructureNodeType.PowerJunction:
                return "Power";

            case InfrastructureNodeType.TransitLift:
                return "Transit";

            default:
                return "Signal";
        }
    }

    void ActivateDeepCitySignal(Vector3 transitPosition)
    {
        Vector3 basePosition = Vector3.zero;
        BaseCampZone baseCamp = FindFirstObjectByType<BaseCampZone>();

        if (baseCamp != null)
        {
            basePosition = baseCamp.transform.position;
        }

        Vector3 outwardDirection = transitPosition - basePosition;
        outwardDirection.y = 0f;

        if (outwardDirection.sqrMagnitude < 0.1f)
        {
            outwardDirection = Vector3.forward;
        }

        deepCitySignalPosition = FindReachableEdgeGatewayPosition(
            transitPosition,
            outwardDirection.normalized,
            out deepCitySignalApproachDirection
        );
        deepCitySignalDetected = true;
        BuildDeepCitySignalBeacon();

        GameReferences refs = GameReferences.Instance;

        if (refs != null && refs.minimapUI != null)
        {
            refs.minimapUI.RefreshNetworkMarkers();
        }
    }

    void BuildDeepCitySignalBeacon()
    {
        if (deepCitySignalBeacon != null)
        {
            GameReferences refs = GameReferences.Instance;

            if (refs != null && refs.interactionPromptUI != null)
            {
                refs.interactionPromptUI.HidePrompt(deepCitySignalBeacon);
            }

            Destroy(deepCitySignalBeacon);
        }

        deepCitySignalBeacon = new GameObject("DeepCitySealedAccess");
        deepCitySignalBeacon.transform.position = deepCitySignalPosition;

        if (deepCitySignalApproachDirection.sqrMagnitude > 0.1f)
        {
            deepCitySignalBeacon.transform.rotation = Quaternion.LookRotation(deepCitySignalApproachDirection.normalized, Vector3.up);
        }

        float gateHeight = Mathf.Max(3.8f, deepSignalBeaconHeight * 0.72f);
        Color structureColor = new Color(0.16f, 0.18f, 0.22f, 1f);
        Color lockedPanelColor = new Color(0.2f, 0.12f, 0.28f, 1f);

        GameObject threshold = CreateBeaconPrimitive(
            PrimitiveType.Cube,
            "DeepAccess_Threshold",
            new Vector3(0f, 0.03f, 0f),
            new Vector3(6.2f, 0.08f, 3.4f),
            structureColor
        );
        threshold.transform.SetParent(deepCitySignalBeacon.transform, false);

        GameObject leftSupport = CreateBeaconPrimitive(
            PrimitiveType.Cube,
            "DeepAccess_LeftSupport",
            new Vector3(-2.65f, gateHeight * 0.5f, 1.2f),
            new Vector3(0.5f, gateHeight, 0.55f),
            structureColor
        );
        leftSupport.transform.SetParent(deepCitySignalBeacon.transform, false);

        GameObject rightSupport = CreateBeaconPrimitive(
            PrimitiveType.Cube,
            "DeepAccess_RightSupport",
            new Vector3(2.65f, gateHeight * 0.5f, 1.2f),
            new Vector3(0.5f, gateHeight, 0.55f),
            structureColor
        );
        rightSupport.transform.SetParent(deepCitySignalBeacon.transform, false);

        GameObject header = CreateBeaconPrimitive(
            PrimitiveType.Cube,
            "DeepAccess_Header",
            new Vector3(0f, gateHeight, 1.2f),
            new Vector3(5.8f, 0.42f, 0.55f),
            structureColor
        );
        header.transform.SetParent(deepCitySignalBeacon.transform, false);

        GameObject sealedDoor = CreateBeaconPrimitive(
            PrimitiveType.Cube,
            "DeepAccess_SealedDoor",
            new Vector3(0f, gateHeight * 0.43f, 1.28f),
            new Vector3(4.65f, gateHeight * 0.78f, 0.18f),
            lockedPanelColor
        );
        sealedDoor.transform.SetParent(deepCitySignalBeacon.transform, false);

        GameObject lockStrip = CreateBeaconPrimitive(
            PrimitiveType.Cube,
            "DeepAccess_LockStrip",
            new Vector3(0f, gateHeight * 0.54f, 1.16f),
            new Vector3(2.2f, 0.13f, 0.05f),
            deepSignalColor
        );
        lockStrip.transform.SetParent(deepCitySignalBeacon.transform, false);

        BuildAccessLamp(new Vector3(-2.1f, gateHeight - 0.48f, 0.84f));
        BuildAccessLamp(new Vector3(2.1f, gateHeight - 0.48f, 0.84f));

        GameObject labelObject = new GameObject("DeepSignal_Label");
        labelObject.transform.SetParent(deepCitySignalBeacon.transform, false);
        labelObject.transform.localPosition = new Vector3(0f, gateHeight + 0.48f, 1.2f);
        TextMesh label = labelObject.AddComponent<TextMesh>();
        label.text = "PERIMETER LINK // LOCKED";
        label.fontSize = 38;
        label.characterSize = 0.05f;
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.color = deepSignalColor;
    }

    GameObject CreateBeaconPrimitive(PrimitiveType type, string objectName, Vector3 localPosition, Vector3 scale, Color color)
    {
        GameObject primitive = GameObject.CreatePrimitive(type);
        primitive.name = objectName;
        primitive.transform.localPosition = localPosition;
        primitive.transform.localScale = scale;

        Collider collider = primitive.GetComponent<Collider>();

        if (collider != null)
        {
            Destroy(collider);
        }

        Renderer renderer = primitive.GetComponent<Renderer>();

        if (renderer != null)
        {
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            block.SetColor("_BaseColor", color);
            block.SetColor("_Color", color);
            renderer.SetPropertyBlock(block);
        }

        return primitive;
    }

    void BuildAccessLamp(Vector3 localPosition)
    {
        GameObject lamp = CreateBeaconPrimitive(
            PrimitiveType.Cube,
            "DeepAccess_ActiveSignalLamp",
            localPosition,
            new Vector3(0.32f, 0.18f, 0.12f),
            deepSignalColor
        );
        lamp.transform.SetParent(deepCitySignalBeacon.transform, false);

        Light light = lamp.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = deepSignalColor;
        light.range = 8f;
        light.intensity = 2f;
    }

    Vector3 FindReachableEdgeGatewayPosition(Vector3 transitPosition, Vector3 outwardDirection, out Vector3 approachDirection)
    {
        CityBlockoutGenerator city = FindFirstObjectByType<CityBlockoutGenerator>();
        Vector3 desiredPosition = transitPosition + outwardDirection * deepSignalDistance;
        approachDirection = transitPosition - desiredPosition;
        approachDirection.y = 0f;

        if (city == null || !city.HasGeneratedMap())
        {
            return desiredPosition;
        }

        List<Vector2Int> walkableCells = city.GetWalkableCells();
        Vector3 bestPosition = desiredPosition;
        Vector3 bestBoundaryOutward = outwardDirection;
        float bestScore = float.MaxValue;
        bool foundEdgeCandidate = false;

        for (int i = 0; i < walkableCells.Count; i++)
        {
            Vector2Int cell = walkableCells[i];
            Vector3 boundaryOutward;
            int boundaryDistance = GetClosestBoundaryDistanceAndDirection(city, cell, out boundaryOutward);
            bool isEdgeCandidate = boundaryDistance <= Mathf.Max(1, deepSignalEdgeBandCells);

            if (foundEdgeCandidate && !isEdgeCandidate)
            {
                continue;
            }

            if (!foundEdgeCandidate && isEdgeCandidate)
            {
                foundEdgeCandidate = true;
                bestScore = float.MaxValue;
            }

            Vector3 candidatePosition = city.CellToWorldPosition(cell, 0.12f);
            Vector3 delta = candidatePosition - transitPosition;
            delta.y = 0f;
            float distanceFromTransit = delta.magnitude;
            float alignment = Vector3.Dot(boundaryOutward, outwardDirection);
            float outwardProgress = Vector3.Dot(delta, outwardDirection);
            float score = boundaryDistance * (isEdgeCandidate ? 12f : 80f);
            score -= alignment * 72f;
            score -= Mathf.Max(0f, outwardProgress) * 0.08f;
            score += distanceFromTransit * 0.04f;

            if (distanceFromTransit < deepSignalMinimumSeparation)
            {
                score += 80f + (deepSignalMinimumSeparation - distanceFromTransit) * 32f;
            }

            if (city.IsCellMainStreet(cell.x, cell.y))
            {
                score -= 14f;
            }
            else if (city.IsCellSideStreet(cell.x, cell.y))
            {
                score -= 6f;
            }
            else if (city.IsCellAlley(cell.x, cell.y))
            {
                score += 10f;
            }

            if (score < bestScore)
            {
                bestScore = score;
                bestPosition = candidatePosition;
                bestBoundaryOutward = boundaryOutward;
            }
        }

        if (bestScore < float.MaxValue)
        {
            approachDirection = -bestBoundaryOutward;
        }

        return bestPosition;
    }

    int GetClosestBoundaryDistanceAndDirection(CityBlockoutGenerator city, Vector2Int cell, out Vector3 boundaryOutward)
    {
        int left = cell.x;
        int right = city.GetGridWidth() - 1 - cell.x;
        int bottom = cell.y;
        int top = city.GetGridHeight() - 1 - cell.y;
        int distance = left;
        boundaryOutward = Vector3.left;

        if (right < distance)
        {
            distance = right;
            boundaryOutward = Vector3.right;
        }

        if (bottom < distance)
        {
            distance = bottom;
            boundaryOutward = Vector3.back;
        }

        if (top < distance)
        {
            distance = top;
            boundaryOutward = Vector3.forward;
        }

        return distance;
    }

    void UpdateDeepSignalPrompt()
    {
        if (!deepCitySignalDetected || deepCitySignalBeacon == null)
        {
            return;
        }

        GameReferences refs = GameReferences.Instance;

        if (refs == null || refs.playerTransform == null || refs.interactionPromptUI == null)
        {
            return;
        }

        float distance = Vector3.Distance(refs.playerTransform.position, deepCitySignalPosition);

        if (distance <= deepSignalPromptDistance && UIStateManager.IsGameplay())
        {
            refs.interactionPromptUI.ShowPrompt(
                "SEALED PERIMETER LINK - local chain recognized; next district route remains locked",
                deepCitySignalBeacon,
                2
            );
        }
        else
        {
            refs.interactionPromptUI.HidePrompt(deepCitySignalBeacon);
        }
    }
}
