using System.Collections.Generic;
using UnityEngine;

public class GeneratedWorldSpawner : MonoBehaviour
{
    [Header("References")]
    public CityBlockoutGenerator city;

    [Header("Prefabs")]
    public GameObject resourcePrefab;
    public GameObject hazardPrefab;
    public GameObject infrastructureNodePrefab;

    [Header("Auto Scaling")]
    public bool autoScaleSpawnCounts = true;
    public float resourceClusterDensity = 0.006f;
    public float hazardClusterDensity = 0.0025f;
    public float infrastructureNodeDensity = 0.0015f;
    public int maxLocalGridResourceClusters = 24;
    public int maxLocalGridHazardClusters = 10;
    public int maxLocalGridInfrastructureNodes = 5;

    [Header("Manual Counts")]
    public int resourceClusterCount = 20;
    public int hazardClusterCount = 8;
    public int infrastructureNodeCount = 3;
    public bool spawnInfrastructureNodesOnStartIfMissing = true;
    public bool spawnStarterRestorationCaches = true;

    [Header("Local Grid First Chapter")]
    public bool buildLocalGridChapterOnStart = true;
    public bool spawnLandmarkSupportSalvage = true;
    public bool spawnLandmarkDiscoveries = true;
    public bool spawnPowerTutorialHazard = true;
    public bool placePrimaryRelaysInCourtyards = true;
    public bool spawnSupplementalInfrastructureNodes = false;

    [Header("District Restoration")]
    public bool spawnDistrictRestorationVisuals = true;
    public int districtRestorationFixtureCount = 18;
    public int districtRestorationFixtureSpacing = 5;

    [Header("Opening Run Pacing")]
    public int rootSupplyMinCellDistance = 3;
    public int rootSupplyMaxCellDistance = 6;
    public int outboundSupplyMinCellDistance = 5;
    public int outboundSupplyMaxCellDistance = 11;
    public int signalRelayMinCellDistance = 8;
    public int signalRelayMaxCellDistance = 11;
    public int powerJunctionMinCellDistance = 16;
    public int powerJunctionMaxCellDistance = 20;
    public int transitLiftMinCellDistance = 24;
    public int transitLiftMaxCellDistance = 28;
    public int minimumRequiredRelayStageGap = 4;
    public int maximumRequiredRelayStageGap = 11;
    public float minimumOpeningRelayDirectionChange = 24f;
    public bool validateOpeningPacing = true;
    public bool logSuccessfulOpeningPacing = false;

    [Header("Cluster Sizes")]
    public int minResourceClusterSize = 2;
    public int maxResourceClusterSize = 5;
    public int minHazardClusterSize = 1;
    public int maxHazardClusterSize = 3;

    [Header("Placement Rules")]
    public float minimumDistanceFromBase = 8f;
    public int generalHazardMinimumCellDistance = 14;
    public int guaranteedFamiliarResourceClusters = 3;
    public int familiarResourceMaximumCellDistance = 15;
    public int minimumResourceSpacing = 3;
    public int minimumHazardSpacing = 5;
    public int minimumResourceHazardSpacing = 3;

    [Header("Spawn Heights")]
    public float resourceYOffset = 0.5f;
    public float hazardYOffset = 0.05f;

    [Header("Debug Data")]
    [SerializeField] private List<Vector2Int> spawnedResourceCells = new List<Vector2Int>();
    [SerializeField] private List<Vector2Int> spawnedHazardCells = new List<Vector2Int>();
    [SerializeField] private List<Vector2Int> spawnedInfrastructureCells = new List<Vector2Int>();
    [SerializeField] private List<Vector2Int> spawnedDiscoveryCells = new List<Vector2Int>();
    [SerializeField] private List<Vector2Int> spawnedRootSupplyCells = new List<Vector2Int>();
    [SerializeField] private Vector2Int requiredSignalCell = new Vector2Int(-1, -1);
    [SerializeField] private Vector2Int requiredPowerCell = new Vector2Int(-1, -1);
    [SerializeField] private Vector2Int requiredTransitCell = new Vector2Int(-1, -1);
    private readonly HashSet<Vector2Int> spawnedResourceLookup = new HashSet<Vector2Int>();
    private readonly HashSet<Vector2Int> spawnedHazardLookup = new HashSet<Vector2Int>();
    private readonly HashSet<Vector2Int> spawnedInfrastructureLookup = new HashSet<Vector2Int>();
    private readonly List<InfrastructureNode> requiredChainNodes = new List<InfrastructureNode>();
    private bool lookupCachesValid = false;

    void Start()
    {
        if (buildLocalGridChapterOnStart && city != null && city.HasGeneratedMap())
        {
            GenerateSpawnedObjects();
            return;
        }

        if (!spawnInfrastructureNodesOnStartIfMissing)
        {
            return;
        }

        if (city == null || !city.HasGeneratedMap() || spawnedInfrastructureCells.Count > 0)
        {
            return;
        }

        EnsureInfrastructureNetworkManager();

        if (autoScaleSpawnCounts)
        {
            ApplyAutoScaledSpawnCounts();
        }

        SpawnInfrastructureNodes();
        SpawnDistrictRestorationVisuals();
    }

    public List<Vector2Int> GetSpawnedResourceCells()
    {
        return new List<Vector2Int>(spawnedResourceCells);
    }

    public List<Vector2Int> GetSpawnedHazardCells()
    {
        return new List<Vector2Int>(spawnedHazardCells);
    }

    public List<Vector2Int> GetSpawnedInfrastructureCells()
    {
        return new List<Vector2Int>(spawnedInfrastructureCells);
    }

    public bool HasResourceAtCell(int x, int y)
    {
        EnsureLookupCaches();
        return spawnedResourceLookup.Contains(new Vector2Int(x, y));
    }

    public bool HasHazardAtCell(int x, int y)
    {
        EnsureLookupCaches();
        return spawnedHazardLookup.Contains(new Vector2Int(x, y));
    }

    public bool HasInfrastructureNodeAtCell(int x, int y)
    {
        EnsureLookupCaches();
        return spawnedInfrastructureLookup.Contains(new Vector2Int(x, y));
    }

    public bool IsRestoredDistrictCell(int x, int y)
    {
        if (city == null || !city.HasGeneratedMap())
        {
            return false;
        }

        for (int i = 0; i < requiredChainNodes.Count; i++)
        {
            InfrastructureNode node = requiredChainNodes[i];

            if (node != null &&
                node.restored &&
                city.IsCellInRestorationDistrict(x, y, node.nodeType))
            {
                return true;
            }
        }

        return false;
    }

    public void RemoveResourceAtCell(Vector2Int cell)
    {
        EnsureLookupCaches();

        if (spawnedResourceLookup.Contains(cell) || spawnedResourceCells.Contains(cell))
        {
            spawnedResourceCells.Remove(cell);
            spawnedResourceLookup.Remove(cell);
        }
    }

    [ContextMenu("Generate Spawned World Objects")]
    public void GenerateSpawnedObjects()
    {
        ClearSpawnedObjects();
        EnsureInfrastructureNetworkManager();

        if (city == null || !city.HasGeneratedMap())
        {
            Debug.LogWarning("GeneratedWorldSpawner: No generated city data found.");
            return;
        }

        if (autoScaleSpawnCounts)
        {
            ApplyAutoScaledSpawnCounts();
        }

        SpawnRootRepairResources();
        SpawnInfrastructureNodes();
        SpawnDistrictRestorationVisuals();
        SpawnOutboundSignalResources();
        SpawnResources();
        SpawnHazards();
        ValidateOpeningPacing();
    }

    void EnsureInfrastructureNetworkManager()
    {
        if (FindFirstObjectByType<InfrastructureNetworkManager>() != null)
        {
            return;
        }

        GameObject managerObject = new GameObject("InfrastructureNetwork");
        managerObject.AddComponent<InfrastructureNetworkManager>();
    }

    [ContextMenu("Clear Spawned World Objects")]
    public void ClearSpawnedObjects()
    {
        List<GameObject> childrenToDelete = new List<GameObject>();

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            childrenToDelete.Add(transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < childrenToDelete.Count; i++)
        {
            if (childrenToDelete[i] == null) continue;

            if (Application.isPlaying)
            {
                Destroy(childrenToDelete[i]);
            }
            else
            {
                DestroyImmediate(childrenToDelete[i]);
            }
        }

        spawnedResourceCells.Clear();
        spawnedHazardCells.Clear();
        spawnedInfrastructureCells.Clear();
        spawnedDiscoveryCells.Clear();
        spawnedRootSupplyCells.Clear();
        requiredSignalCell = new Vector2Int(-1, -1);
        requiredPowerCell = new Vector2Int(-1, -1);
        requiredTransitCell = new Vector2Int(-1, -1);
        requiredChainNodes.Clear();
        InvalidateLookupCaches();
    }

    void ApplyAutoScaledSpawnCounts()
    {
        int mapArea = city.GetGridWidth() * city.GetGridHeight();

        resourceClusterCount = Mathf.Clamp(Mathf.RoundToInt(mapArea * resourceClusterDensity), 6, maxLocalGridResourceClusters);
        hazardClusterCount = Mathf.Clamp(Mathf.RoundToInt(mapArea * hazardClusterDensity), 3, maxLocalGridHazardClusters);
        infrastructureNodeCount = spawnSupplementalInfrastructureNodes
            ? Mathf.Clamp(Mathf.RoundToInt(mapArea * infrastructureNodeDensity), 4, maxLocalGridInfrastructureNodes)
            : 3;
    }

    void SpawnRootRepairResources()
    {
        if (!spawnStarterRestorationCaches || resourcePrefab == null)
        {
            return;
        }

        SpawnStarterResource(ItemType.Wiring, 2, "reactivation wiring cache", rootSupplyMinCellDistance, rootSupplyMaxCellDistance, true, null);
        SpawnStarterResource(ItemType.CoreFragment, 1, "reactivation core housing", rootSupplyMinCellDistance, rootSupplyMaxCellDistance, true, null);
    }

    void SpawnOutboundSignalResources()
    {
        if (!spawnStarterRestorationCaches || resourcePrefab == null)
        {
            return;
        }

        Vector2? routeAnchor = GetOutboundSignalSupplyAnchor();
        SpawnStarterResource(ItemType.MetalScrap, 1, "signal-route service plate", outboundSupplyMinCellDistance, outboundSupplyMaxCellDistance, false, routeAnchor);
        SpawnStarterResource(ItemType.Wiring, 1, "signal-route utility trunk", outboundSupplyMinCellDistance, outboundSupplyMaxCellDistance, false, routeAnchor);
    }

    bool SpawnStarterResource(
        ItemType itemType,
        int amount,
        string sourceContext,
        int minimumDistance,
        int maximumDistance,
        bool rootRepairSupply,
        Vector2? preferredRoutePosition)
    {
        List<Vector2Int> candidates = BuildStarterResourceCandidates(minimumDistance, maximumDistance, true);

        if (candidates.Count == 0)
        {
            candidates = BuildStarterResourceCandidates(minimumDistance, maximumDistance, false);
        }

        if (candidates.Count == 0 && rootRepairSupply)
        {
            candidates = BuildStarterResourceCandidates(city.baseCampRadius + 1, maximumDistance + 3, false);
        }

        if (candidates.Count == 0)
        {
            Debug.LogWarning("GeneratedWorldSpawner: Could not place opening supply '" + sourceContext + "'.");
            return false;
        }

        Vector2Int cell = PickOpeningSupplyCandidate(candidates, (minimumDistance + maximumDistance) * 0.5f, preferredRoutePosition);
        SpawnResourceAtCell(cell, itemType, amount, sourceContext);

        if (rootRepairSupply)
        {
            spawnedRootSupplyCells.Add(cell);
        }

        return true;
    }

    List<Vector2Int> BuildStarterResourceCandidates(int minimumDistance, int maximumDistance, bool requireSceneryNeighbor)
    {
        List<Vector2Int> candidates = new List<Vector2Int>();
        List<Vector2Int> walkableCells = city.GetWalkableCells();
        float innerRadius = Mathf.Max(city.baseCampRadius + 1f, minimumDistance);
        float outerRadius = Mathf.Max(maximumDistance, innerRadius + 1f);

        for (int i = 0; i < walkableCells.Count; i++)
        {
            Vector2Int cell = walkableCells[i];
            float distance = city.GetDistanceFromBase(cell);

            if (distance < innerRadius || distance > outerRadius)
            {
                continue;
            }

            if (IsTooCloseToCells(cell, spawnedResourceCells, 2))
            {
                continue;
            }

            if (IsTooCloseToCells(cell, spawnedInfrastructureCells, 3))
            {
                continue;
            }

            if (requireSceneryNeighbor && !city.IsCellPlaza(cell.x, cell.y) && !HasNonWalkableCellNearby(cell, 1))
            {
                continue;
            }

            candidates.Add(cell);
        }

        return candidates;
    }

    Vector2? GetOutboundSignalSupplyAnchor()
    {
        if (!IsRecordedCell(requiredSignalCell))
        {
            return null;
        }

        Vector2 baseCell = new Vector2(city.GetBaseCell().x, city.GetBaseCell().y);
        Vector2 signalCell = new Vector2(requiredSignalCell.x, requiredSignalCell.y);
        return Vector2.Lerp(baseCell, signalCell, 0.65f);
    }

    Vector2Int PickOpeningSupplyCandidate(List<Vector2Int> candidates, float targetDistance, Vector2? preferredRoutePosition)
    {
        Vector2Int selected = candidates[0];
        float bestScore = float.MaxValue;

        for (int i = 0; i < candidates.Count; i++)
        {
            Vector2Int candidate = candidates[i];
            float score = Mathf.Abs(city.GetDistanceFromBase(candidate) - targetDistance);

            if (preferredRoutePosition.HasValue)
            {
                score += Vector2.Distance(new Vector2(candidate.x, candidate.y), preferredRoutePosition.Value) * 1.5f;
            }

            score += Random.Range(0f, 0.4f);

            if (score < bestScore)
            {
                bestScore = score;
                selected = candidate;
            }
        }

        return selected;
    }

    void SpawnResources()
    {
        if (resourcePrefab == null) return;

        List<Vector2Int> candidates = BuildResourceCandidates();
        List<Vector2Int> resourceOpportunities = city.GetResourceOpportunityCells();
        int opportunityClustersSpawned = SpawnResourceOpportunityClusters(resourceOpportunities);

        for (int i = opportunityClustersSpawned; i < resourceClusterCount; i++)
        {
            if (candidates.Count == 0) break;

            Vector2Int center;

            if (i < guaranteedFamiliarResourceClusters &&
                TryPickCandidateInDistanceBand(candidates, minimumDistanceFromBase + 1f, familiarResourceMaximumCellDistance, out Vector2Int familiarCenter))
            {
                center = familiarCenter;
            }
            else
            {
                center = PickWeightedCandidate(candidates, 1.25f);
            }

            int clusterSize = Random.Range(minResourceClusterSize, maxResourceClusterSize + 1);

            if (IsValidResourceCell(center))
            {
                SpawnResourceAtCell(center);
            }

            for (int j = 1; j < clusterSize; j++)
            {
                Vector2Int cell = center + new Vector2Int(
                    Random.Range(-1, 2),
                    Random.Range(-1, 2)
                );

                if (!IsValidResourceCell(cell))
                {
                    continue;
                }

                SpawnResourceAtCell(cell);
            }
        }

        EnsureFamiliarResourceBand();
    }

    void EnsureFamiliarResourceBand()
    {
        int existing = CountResourcesInDistanceBand(minimumDistanceFromBase + 1f, familiarResourceMaximumCellDistance);

        for (int i = existing; i < guaranteedFamiliarResourceClusters; i++)
        {
            if (!TrySpawnFamiliarResourceFallback())
            {
                break;
            }
        }
    }

    bool TrySpawnFamiliarResourceFallback()
    {
        List<Vector2Int> candidates = new List<Vector2Int>();
        List<Vector2Int> walkableCells = city.GetWalkableCells();

        for (int i = 0; i < walkableCells.Count; i++)
        {
            Vector2Int cell = walkableCells[i];
            float distance = city.GetDistanceFromBase(cell);

            if (distance < minimumDistanceFromBase + 1f || distance > familiarResourceMaximumCellDistance)
            {
                continue;
            }

            if (city.IsCellPlaza(cell.x, cell.y) || city.IsCellMainStreet(cell.x, cell.y))
            {
                continue;
            }

            if (!HasNonWalkableCellNearby(cell, 1))
            {
                continue;
            }

            if (IsTooCloseToCells(cell, spawnedResourceCells, Mathf.Max(2, minimumResourceSpacing - 1)) ||
                IsTooCloseToCells(cell, spawnedInfrastructureCells, 3))
            {
                continue;
            }

            candidates.Add(cell);
        }

        if (candidates.Count == 0)
        {
            return false;
        }

        Vector2Int selected = candidates[Random.Range(0, candidates.Count)];
        SpawnResourceAtCell(selected, Random.value < 0.75f ? ItemType.MetalScrap : ItemType.Wiring, 1, "starter salvage cache");
        return true;
    }

    void SpawnHazards()
    {
        if (hazardPrefab == null) return;

        List<Vector2Int> candidates = BuildHazardCandidates();

        for (int i = 0; i < hazardClusterCount; i++)
        {
            if (candidates.Count == 0) return;

            Vector2Int center = PickWeightedCandidate(candidates, 1.75f);
            int clusterSize = Random.Range(minHazardClusterSize, maxHazardClusterSize + 1);

            for (int j = 0; j < clusterSize; j++)
            {
                Vector2Int cell = center + new Vector2Int(
                    Random.Range(-1, 2),
                    Random.Range(-1, 2)
                );

                if (!IsValidHazardCell(cell))
                {
                    continue;
                }

                SpawnHazardAtCell(cell);
            }
        }

        EnsureSectorHazardPressure(candidates);
    }

    void EnsureSectorHazardPressure(List<Vector2Int> candidates)
    {
        if (city == null || candidates == null || candidates.Count == 0)
        {
            return;
        }

        int[] targetHazardsBySector =
        {
            1,
            2,
            3
        };

        for (int sector = 0; sector < targetHazardsBySector.Length; sector++)
        {
            int existing = CountHazardsInSector(sector);

            for (int i = existing; i < targetHazardsBySector[sector]; i++)
            {
                if (!TrySpawnHazardInSector(candidates, sector))
                {
                    break;
                }
            }
        }
    }

    int SpawnResourceOpportunityClusters(List<Vector2Int> resourceOpportunities)
    {
        if (resourceOpportunities == null || resourceOpportunities.Count == 0)
        {
            return 0;
        }

        int spawnedClusters = 0;

        for (int i = 0; i < resourceOpportunities.Count; i++)
        {
            Vector2Int center = resourceOpportunities[i];

            if (!IsValidResourceCell(center))
            {
                continue;
            }

            SpawnResourceAtCell(center, PickGeneratedResourceType(center), PickGeneratedResourceAmount(center), PickResourceOpportunityContext(center));
            spawnedClusters++;

            if (Random.value < 0.55f)
            {
                Vector2Int bonus = center + new Vector2Int(Random.Range(-1, 2), Random.Range(-1, 2));

                if (bonus != center && IsValidResourceCell(bonus))
                {
                    SpawnResourceAtCell(bonus, PickGeneratedResourceType(bonus), 1, PickResourceOpportunityContext(center));
                }
            }
        }

        return spawnedClusters;
    }

    int CountHazardsInSector(int sector)
    {
        int count = 0;

        for (int i = 0; i < spawnedHazardCells.Count; i++)
        {
            if (city.GetRestorationDistrictIndex(spawnedHazardCells[i]) == sector)
            {
                count++;
            }
        }

        return count;
    }

    bool TrySpawnHazardInSector(List<Vector2Int> candidates, int sector)
    {
        List<Vector2Int> sectorCandidates = new List<Vector2Int>();

        for (int i = 0; i < candidates.Count; i++)
        {
            Vector2Int candidate = candidates[i];

            if (city.GetRestorationDistrictIndex(candidate) == sector && IsValidHazardCell(candidate))
            {
                sectorCandidates.Add(candidate);
            }
        }

        if (sectorCandidates.Count == 0)
        {
            return false;
        }

        Vector2Int selected = PickWeightedCandidate(sectorCandidates, sector == 0 ? 0.6f : sector == 1 ? 1.2f : 1.8f);
        SpawnHazardAtCell(selected);
        return true;
    }

    bool TryPickCandidateInDistanceBand(
        List<Vector2Int> candidates,
        float minimumDistance,
        float maximumDistance,
        out Vector2Int selected)
    {
        selected = default;
        List<Vector2Int> bandCandidates = new List<Vector2Int>();

        for (int i = 0; i < candidates.Count; i++)
        {
            float distance = city.GetDistanceFromBase(candidates[i]);

            if (distance >= minimumDistance && distance <= maximumDistance &&
                IsValidResourceCell(candidates[i]))
            {
                bandCandidates.Add(candidates[i]);
            }
        }

        if (bandCandidates.Count == 0)
        {
            return false;
        }

        selected = bandCandidates[Random.Range(0, bandCandidates.Count)];
        return true;
    }

    void SpawnInfrastructureNodes()
    {
        List<Vector2Int> candidates = BuildInfrastructureNodeCandidates();

        int spawnedCount = 0;
        int attempts = 0;

        spawnedCount += TrySpawnRequiredChainNode(candidates, InfrastructureNodeType.SignalRelay, signalRelayMinCellDistance, signalRelayMaxCellDistance) ? 1 : 0;
        spawnedCount += TrySpawnRequiredChainNode(candidates, InfrastructureNodeType.PowerJunction, powerJunctionMinCellDistance, powerJunctionMaxCellDistance) ? 1 : 0;
        spawnedCount += TrySpawnRequiredChainNode(candidates, InfrastructureNodeType.TransitLift, transitLiftMinCellDistance, transitLiftMaxCellDistance) ? 1 : 0;

        while (spawnSupplementalInfrastructureNodes && spawnedCount < infrastructureNodeCount &&
               candidates.Count > 0 && attempts < infrastructureNodeCount * 12)
        {
            attempts++;

            Vector2Int cell = PickWeightedCandidate(candidates, 1.35f);

            if (!IsValidInfrastructureNodeCell(cell))
            {
                candidates.Remove(cell);
                continue;
            }

            SpawnInfrastructureNodeAtCell(cell, PickInfrastructureNodeType(spawnedCount));
            candidates.Remove(cell);
            spawnedCount++;
        }
    }

    bool TrySpawnRequiredChainNode(List<Vector2Int> candidates, InfrastructureNodeType nodeType, int minCellDistance, int maxCellDistance)
    {
        if (candidates == null || candidates.Count == 0)
        {
            return false;
        }

        Vector2Int openingCell;

        if (city.TryGetOpeningRelayCourtyardCell(nodeType, out openingCell))
        {
            if (IsValidInfrastructureNodeCell(openingCell))
            {
                SpawnInfrastructureNodeAtCell(openingCell, nodeType, true);
                candidates.Remove(openingCell);
                return true;
            }
        }

        List<Vector2Int> courtyardCandidates = BuildRequiredCourtyardCandidates(minCellDistance, maxCellDistance);
        List<Vector2Int> bandCandidates = new List<Vector2Int>();

        for (int i = 0; i < candidates.Count; i++)
        {
            Vector2Int cell = candidates[i];

            if (!IsValidInfrastructureNodeCell(cell))
            {
                continue;
            }

            float distance = city.GetDistanceFromBase(cell);

            if (distance >= minCellDistance && distance <= maxCellDistance)
            {
                bandCandidates.Add(cell);
            }
        }

        List<Vector2Int> source = courtyardCandidates.Count > 0
            ? courtyardCandidates
            : bandCandidates.Count > 0 ? bandCandidates : candidates;
        Vector2Int selected = PickProgressionCandidate(source, (minCellDistance + maxCellDistance) * 0.5f);

        if (!IsValidInfrastructureNodeCell(selected))
        {
            return false;
        }

        SpawnInfrastructureNodeAtCell(selected, nodeType, true);
        candidates.Remove(selected);
        return true;
    }

    List<Vector2Int> BuildRequiredCourtyardCandidates(int minCellDistance, int maxCellDistance)
    {
        List<Vector2Int> candidates = new List<Vector2Int>();

        if (!placePrimaryRelaysInCourtyards)
        {
            return candidates;
        }

        List<Vector2Int> anchors = city.GetCourtyardAnchorCells();

        for (int i = 0; i < anchors.Count; i++)
        {
            Vector2Int cell = anchors[i];

            if (!IsValidInfrastructureNodeCell(cell))
            {
                continue;
            }

            float distance = city.GetDistanceFromBase(cell);

            if (distance >= minCellDistance && distance <= maxCellDistance)
            {
                candidates.Add(cell);
            }
        }

        return candidates;
    }

    Vector2Int PickProgressionCandidate(List<Vector2Int> candidates, float targetDistance)
    {
        Vector2Int selected = candidates[0];
        float bestScore = float.MaxValue;

        for (int i = 0; i < candidates.Count; i++)
        {
            Vector2Int candidate = candidates[i];
            float score = Mathf.Abs(city.GetDistanceFromBase(candidate) - targetDistance);
            score += Random.Range(0f, 0.6f);

            if (score < bestScore)
            {
                bestScore = score;
                selected = candidate;
            }
        }

        return selected;
    }

    List<Vector2Int> BuildResourceCandidates()
    {
        List<Vector2Int> candidates = new List<Vector2Int>();
        List<Vector2Int> walkableCells = city.GetWalkableCells();
        List<Vector2Int> resourceOpportunities = city.GetResourceOpportunityCells();

        for (int i = 0; i < resourceOpportunities.Count; i++)
        {
            Vector2Int opportunity = resourceOpportunities[i];

            if (!city.IsCellNearBase(opportunity, minimumDistanceFromBase) &&
                city.IsCellWalkable(opportunity.x, opportunity.y))
            {
                candidates.Add(opportunity);
            }
        }

        for (int i = 0; i < walkableCells.Count; i++)
        {
            Vector2Int cell = walkableCells[i];

            if (city.IsCellNearBase(cell, minimumDistanceFromBase))
            {
                continue;
            }

            bool isGoodResourceArea =
                IsResourceOpportunityCell(cell) ||
                city.IsCellPlaza(cell.x, cell.y) ||
                ((city.IsCellAlley(cell.x, cell.y) || city.IsCellSideStreet(cell.x, cell.y)) && HasNonWalkableCellNearby(cell, 1));

            if (isGoodResourceArea)
            {
                candidates.Add(cell);
            }
        }

        return candidates;
    }

    List<Vector2Int> BuildHazardCandidates()
    {
        List<Vector2Int> candidates = new List<Vector2Int>();
        List<Vector2Int> walkableCells = city.GetWalkableCells();
        float safeZoneRadius = Mathf.Max(minimumDistanceFromBase + 2f, generalHazardMinimumCellDistance);

        for (int i = 0; i < walkableCells.Count; i++)
        {
            Vector2Int cell = walkableCells[i];

            if (city.IsCellNearBase(cell, safeZoneRadius))
            {
                continue;
            }

            bool nearResourceRoute = IsNearResourceOpportunityRoute(cell);
            bool isGoodHazardArea =
                city.IsCellAlley(cell.x, cell.y) ||
                city.IsCellSideStreet(cell.x, cell.y) ||
                (city.IsCellMainStreet(cell.x, cell.y) && nearResourceRoute);

            if (!isGoodHazardArea || city.IsCellPlaza(cell.x, cell.y))
            {
                continue;
            }

            int sectorIndex = city.GetRestorationDistrictIndex(cell);
            float keepChance = sectorIndex <= 0 ? 0.42f : sectorIndex == 1 ? 0.72f : 1f;

            if (nearResourceRoute)
            {
                keepChance = Mathf.Min(1f, keepChance + 0.24f);
            }

            if (Random.value <= keepChance)
            {
                candidates.Add(cell);
            }
        }

        return candidates;
    }

    List<Vector2Int> BuildInfrastructureNodeCandidates()
    {
        List<Vector2Int> candidates = new List<Vector2Int>();
        List<Vector2Int> walkableCells = city.GetWalkableCells();

        for (int i = 0; i < walkableCells.Count; i++)
        {
            Vector2Int cell = walkableCells[i];

            if (city.IsCellNearBase(cell, Mathf.Max(city.baseCampRadius + 2f, signalRelayMinCellDistance - 1f)))
            {
                continue;
            }

            bool isGoodNodeArea =
                city.IsCellPlaza(cell.x, cell.y) ||
                city.IsCellMainStreet(cell.x, cell.y) ||
                city.IsCellSideStreet(cell.x, cell.y);

            if (isGoodNodeArea)
            {
                candidates.Add(cell);
            }
        }

        return candidates;
    }

    Vector2Int PickWeightedCandidate(List<Vector2Int> candidates, float distanceBias)
    {
        Vector2Int bestCell = candidates[Random.Range(0, candidates.Count)];
        float bestScore = -999f;

        for (int i = 0; i < 8; i++)
        {
            Vector2Int candidate = candidates[Random.Range(0, candidates.Count)];

            float distanceFromBase = city.GetDistanceFromBase(candidate);
            float randomBonus = Random.Range(0f, 10f);
            float score = distanceFromBase * distanceBias + randomBonus;

            if (score > bestScore)
            {
                bestScore = score;
                bestCell = candidate;
            }
        }

        return bestCell;
    }

    bool IsValidResourceCell(Vector2Int cell)
    {
        if (!city.IsCellWalkable(cell.x, cell.y)) return false;
        if (city.IsCellNearBase(cell, minimumDistanceFromBase)) return false;
        if (!IsResourceOpportunityCell(cell) && !city.IsCellPlaza(cell.x, cell.y) && !HasNonWalkableCellNearby(cell, 1)) return false;
        if (IsTooCloseToCells(cell, spawnedResourceCells, minimumResourceSpacing)) return false;
        if (IsTooCloseToCells(cell, spawnedHazardCells, minimumResourceHazardSpacing)) return false;
        if (IsTooCloseToCells(cell, spawnedInfrastructureCells, 4)) return false;

        return true;
    }

    bool IsValidHazardCell(Vector2Int cell)
    {
        if (!city.IsCellWalkable(cell.x, cell.y)) return false;
        if (city.IsCellNearBase(cell, Mathf.Max(minimumDistanceFromBase + 2f, generalHazardMinimumCellDistance))) return false;
        if (city.IsCellPlaza(cell.x, cell.y)) return false;
        if (IsTooCloseToCells(cell, spawnedHazardCells, minimumHazardSpacing)) return false;
        if (IsTooCloseToCells(cell, spawnedResourceCells, minimumResourceHazardSpacing)) return false;
        if (IsTooCloseToCells(cell, spawnedInfrastructureCells, 4)) return false;

        return true;
    }

    bool IsValidInfrastructureNodeCell(Vector2Int cell)
    {
        if (!city.IsCellWalkable(cell.x, cell.y)) return false;
        if (city.IsCellNearBase(cell, Mathf.Max(city.baseCampRadius + 2f, signalRelayMinCellDistance - 1f))) return false;
        if (IsTooCloseToCells(cell, spawnedInfrastructureCells, 8)) return false;
        if (IsTooCloseToCells(cell, spawnedHazardCells, 3)) return false;

        return true;
    }

    bool IsTooCloseToCells(Vector2Int cell, List<Vector2Int> existingCells, int minimumSpacing)
    {
        for (int i = 0; i < existingCells.Count; i++)
        {
            if (Vector2Int.Distance(cell, existingCells[i]) < minimumSpacing)
            {
                return true;
            }
        }

        return false;
    }

    bool HasNonWalkableCellNearby(Vector2Int cell, int radius)
    {
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                if (x == 0 && y == 0)
                {
                    continue;
                }

                int checkX = cell.x + x;
                int checkY = cell.y + y;

                if (checkX < 0 || checkX >= city.GetGridWidth() || checkY < 0 || checkY >= city.GetGridHeight())
                {
                    continue;
                }

                if (!city.IsCellWalkable(checkX, checkY))
                {
                    return true;
                }
            }
        }

        return false;
    }

    bool IsResourceOpportunityCell(Vector2Int cell)
    {
        List<Vector2Int> opportunities = city.GetResourceOpportunityCells();

        for (int i = 0; i < opportunities.Count; i++)
        {
            if (opportunities[i] == cell)
            {
                return true;
            }
        }

        return false;
    }

    bool IsNearResourceOpportunityRoute(Vector2Int cell)
    {
        List<Vector2Int> opportunities = city.GetResourceOpportunityCells();

        for (int i = 0; i < opportunities.Count; i++)
        {
            float distance = Vector2Int.Distance(cell, opportunities[i]);

            if (distance >= 3f && distance <= 11f)
            {
                return true;
            }
        }

        return false;
    }

    void SpawnResourceAtCell(Vector2Int cell)
    {
        SpawnResourceAtCell(cell, PickGeneratedResourceType(cell), PickGeneratedResourceAmount(cell), PickResourceSourceContext(cell));
    }

    void SpawnResourceAtCell(Vector2Int cell, ItemType itemType, int amount, string sourceContext)
    {
        Vector3 position = city.CellToWorld(cell.x, cell.y, resourceYOffset);
        GameObject spawnedObject = Instantiate(resourcePrefab, position, Quaternion.identity, transform);
        spawnedObject.name = "Generated_ResourcePile_" + GetResourceSourceName(sourceContext);
        GeneratedResourceMarker marker = spawnedObject.AddComponent<GeneratedResourceMarker>();
        marker.spawner = this;
        marker.cell = cell;

        ScrapPickup pickup = spawnedObject.GetComponent<ScrapPickup>();

        if (pickup != null)
        {
            pickup.itemType = itemType;
            pickup.itemAmount = amount;
            pickup.sourceContext = sourceContext;
        }

        ApplyResourcePlaceholderVisuals(spawnedObject, itemType, sourceContext);

        EnsureLookupCaches();
        spawnedResourceCells.Add(cell);
        spawnedResourceLookup.Add(cell);
    }

    void ApplyResourcePlaceholderVisuals(GameObject resourceObject, ItemType itemType, string sourceContext)
    {
        if (resourceObject == null)
        {
            return;
        }

        bool starterCache = IsStarterResourceContext(sourceContext);
        resourceObject.transform.localScale = GetResourceSourceScale(sourceContext, starterCache);

        GameObject marker = GameObject.CreatePrimitive(GetResourceSourceShape(sourceContext, itemType));
        marker.name = "ResourceVisual_" + itemType;
        marker.transform.SetParent(resourceObject.transform);
        marker.transform.localPosition = GetResourceMarkerPosition(sourceContext);
        marker.transform.localScale = GetResourceMarkerScale(sourceContext, itemType);

        Collider markerCollider = marker.GetComponent<Collider>();

        if (markerCollider != null)
        {
            if (Application.isPlaying)
            {
                Destroy(markerCollider);
            }
            else
            {
                DestroyImmediate(markerCollider);
            }
        }

        Renderer markerRenderer = marker.GetComponent<Renderer>();

        if (markerRenderer != null)
        {
            Renderer baseRenderer = resourceObject.GetComponent<Renderer>();
            markerRenderer.sharedMaterial = baseRenderer != null ? baseRenderer.sharedMaterial : markerRenderer.sharedMaterial;
            ApplyRendererColor(markerRenderer, GetResourceVisualColor(itemType, starterCache));
        }

        GameObject labelObject = new GameObject("ResourceVisual_Label");
        labelObject.transform.SetParent(resourceObject.transform);
        labelObject.transform.localPosition = new Vector3(0f, 1.2f, 0f);

        TextMesh label = labelObject.AddComponent<TextMesh>();
        label.text = GetResourceSourceLabel(itemType, sourceContext);
        label.fontSize = 36;
        label.characterSize = 0.05f;
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.color = starterCache ? new Color(0.9f, 1f, 0.72f, 1f) : new Color(0.65f, 0.95f, 0.75f, 1f);
    }

    void ApplyRendererColor(Renderer targetRenderer, Color color)
    {
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        targetRenderer.GetPropertyBlock(block);
        block.SetColor("_BaseColor", color);
        block.SetColor("_Color", color);
        targetRenderer.SetPropertyBlock(block);
    }

    Color GetResourceVisualColor(ItemType itemType, bool starterCache)
    {
        if (starterCache)
        {
            return new Color(0.85f, 0.95f, 0.5f, 1f);
        }

        switch (itemType)
        {
            case ItemType.Wiring:
                return new Color(0.95f, 0.72f, 0.25f, 1f);

            case ItemType.CoreFragment:
                return new Color(0.45f, 0.8f, 1f, 1f);

            default:
                return new Color(0.65f, 0.75f, 0.7f, 1f);
        }
    }

    string GetResourceShortLabel(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.Wiring:
                return "WIRE";

            case ItemType.CoreFragment:
                return "CORE";

            default:
                return "SCRAP";
        }
    }

    void SpawnHazardAtCell(Vector2Int cell, HazardZoneType? forcedType = null, string objectSuffix = "")
    {
        Vector3 position = city.CellToWorld(cell.x, cell.y, hazardYOffset);
        GameObject spawnedObject = Instantiate(hazardPrefab, position, Quaternion.identity, transform);
        spawnedObject.name = "Generated_HazardZone" + objectSuffix;

        HazardZone hazard = spawnedObject.GetComponent<HazardZone>();

        if (hazard != null)
        {
            hazard.hazardType = forcedType ?? PickGeneratedHazardType(cell);
            hazard.affectedSystem = GetAffectedSystemForHazard(hazard.hazardType);
            hazard.damagePerSecond = PickHazardDamagePerSecond(hazard.hazardType);
            ApplyHazardPlaceholderVisuals(spawnedObject, hazard);
        }

        EnsureLookupCaches();
        spawnedHazardCells.Add(cell);
        spawnedHazardLookup.Add(cell);
    }

    void SpawnInfrastructureNodeAtCell(Vector2Int cell, InfrastructureNodeType nodeType, bool requiredChainLandmark = false)
    {
        Vector3 position = city.CellToWorld(cell.x, cell.y, 0.75f);
        GameObject spawnedObject;

        if (infrastructureNodePrefab != null)
        {
            spawnedObject = Instantiate(infrastructureNodePrefab, position, Quaternion.identity, transform);
        }
        else
        {
            spawnedObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            spawnedObject.transform.SetParent(transform);
            spawnedObject.transform.position = position;
            spawnedObject.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);

            Collider collider = spawnedObject.GetComponent<Collider>();

            if (collider != null)
            {
                collider.isTrigger = true;
            }
        }

        InfrastructureNode node = spawnedObject.GetComponent<InfrastructureNode>();

        if (node == null)
        {
            node = spawnedObject.AddComponent<InfrastructureNode>();
        }

        node.Configure(nodeType, cell, GetRepairCostsForNode(nodeType), requiredChainLandmark);
        EnsureLookupCaches();
        spawnedInfrastructureCells.Add(cell);
        spawnedInfrastructureLookup.Add(cell);

        if (requiredChainLandmark)
        {
            if (!requiredChainNodes.Contains(node))
            {
                requiredChainNodes.Add(node);
            }

            RecordRequiredChainCell(nodeType, cell);
            SpawnLocalGridLandmarkContent(node);
            SpawnLocalGridLandmarkSite(node);
        }
    }

    void SpawnDistrictRestorationVisuals()
    {
        if (!spawnDistrictRestorationVisuals || city == null || !city.HasGeneratedMap())
        {
            return;
        }

        for (int i = 0; i < requiredChainNodes.Count; i++)
        {
            InfrastructureNode node = requiredChainNodes[i];

            if (node == null)
            {
                continue;
            }

            List<Vector2Int> fixtureCells = city.GetRestorationDistrictSampleCells(
                node.nodeType,
                Mathf.Max(4, districtRestorationFixtureCount),
                Mathf.Max(2, districtRestorationFixtureSpacing)
            );

            if (fixtureCells.Count == 0)
            {
                continue;
            }

            GameObject districtObject = new GameObject("RestorationDistrict_" + node.nodeType);
            districtObject.transform.SetParent(transform);
            RestorationDistrictVisual districtVisual = districtObject.AddComponent<RestorationDistrictVisual>();
            districtVisual.Configure(node, GetDistrictRestorationColor(node.nodeType));

            for (int fixtureIndex = 0; fixtureIndex < fixtureCells.Count; fixtureIndex++)
            {
                Vector3 position = city.CellToWorld(fixtureCells[fixtureIndex].x, fixtureCells[fixtureIndex].y, 0f);
                districtVisual.AddFixture(position, city.GetCellSize());
            }
        }
    }

    Color GetDistrictRestorationColor(InfrastructureNodeType nodeType)
    {
        switch (nodeType)
        {
            case InfrastructureNodeType.PowerJunction:
                return new Color(0.2f, 0.68f, 1f, 1f);

            case InfrastructureNodeType.TransitLift:
                return new Color(0.42f, 0.84f, 1f, 1f);

            default:
                return new Color(0.18f, 0.95f, 1f, 1f);
        }
    }

    void SpawnLocalGridLandmarkSite(InfrastructureNode node)
    {
        GameObject siteObject = new GameObject("LocalGridSite_" + node.nodeType);
        siteObject.transform.SetParent(transform);
        siteObject.transform.position = new Vector3(node.transform.position.x, 0f, node.transform.position.z);
        Vector3 approachDirection = city.CellToWorldPosition(city.GetBaseCell(), 0f) - siteObject.transform.position;
        approachDirection.y = 0f;

        if (approachDirection.sqrMagnitude > 0.1f)
        {
            siteObject.transform.rotation = Quaternion.LookRotation(approachDirection.normalized, Vector3.up);
        }

        LocalGridLandmarkSite site = siteObject.AddComponent<LocalGridLandmarkSite>();
        site.Configure(node);
    }

    void SpawnLocalGridLandmarkContent(InfrastructureNode node)
    {
        if (node == null)
        {
            return;
        }

        switch (node.nodeType)
        {
            case InfrastructureNodeType.SignalRelay:
                if (spawnLandmarkSupportSalvage)
                {
                    TrySpawnLandmarkResource(node.cell, ItemType.Wiring, 1, "signal relay service locker");
                    TrySpawnLandmarkResource(node.cell, ItemType.MetalScrap, 2, "collapsed signal mast");
                }

                if (spawnLandmarkDiscoveries)
                {
                    TrySpawnLandmarkDiscovery(
                        node.cell,
                        1,
                        "MAINTENANCE TRACE 04",
                        "\"FS-7 reassigned beneath civic layer. Prior identity fields intentionally scrubbed.\""
                    );
                }
                break;

            case InfrastructureNodeType.PowerJunction:
                if (spawnPowerTutorialHazard)
                {
                    TrySpawnPowerTutorialHazard(node.cell);
                }

                if (spawnLandmarkSupportSalvage)
                {
                    TrySpawnLandmarkResource(node.cell, ItemType.MetalScrap, 2, "failed transformer housing");
                    TrySpawnLandmarkResource(node.cell, ItemType.CoreFragment, 1, "junction reserve cell");
                }

                if (spawnLandmarkDiscoveries)
                {
                    TrySpawnLandmarkDiscovery(
                        node.cell,
                        2,
                        "GRID FAILURE TRACE",
                        "\"ISOLATE LOWER GRID. Maintenance units failed - or refused - to disconnect.\""
                    );
                }
                break;

            case InfrastructureNodeType.TransitLift:
                if (spawnLandmarkSupportSalvage)
                {
                    TrySpawnLandmarkResource(node.cell, ItemType.MetalScrap, 2, "sealed transit actuator");
                    TrySpawnLandmarkResource(node.cell, ItemType.CoreFragment, 1, "route-control memory housing");
                }

                if (spawnLandmarkDiscoveries)
                {
                    TrySpawnLandmarkDiscovery(
                        node.cell,
                        3,
                        "TRANSIT AUTHORIZATION",
                        "\"Deep-route lock accepts FS-7 credential. Authorization source: [SELF / CORRUPT].\""
                    );
                }
                break;
        }
    }

    void TrySpawnLandmarkResource(Vector2Int landmarkCell, ItemType itemType, int amount, string sourceContext)
    {
        if (resourcePrefab == null)
        {
            return;
        }

        Vector2Int cell;

        if (TryFindChapterPlacementCell(landmarkCell, ChapterPlacementKind.Resource, out cell))
        {
            SpawnResourceAtCell(cell, itemType, amount, sourceContext);
        }
    }

    void TrySpawnPowerTutorialHazard(Vector2Int landmarkCell)
    {
        if (hazardPrefab == null)
        {
            return;
        }

        Vector2Int cell;

        if (TryFindChapterPlacementCell(landmarkCell, ChapterPlacementKind.Hazard, out cell))
        {
            SpawnHazardAtCell(cell, HazardZoneType.UnstablePower, "_PowerJunctionInstability");
        }
    }

    void TrySpawnLandmarkDiscovery(Vector2Int landmarkCell, int fragmentIndex, string title, string text)
    {
        Vector2Int cell;

        if (!TryFindChapterPlacementCell(landmarkCell, ChapterPlacementKind.Discovery, out cell))
        {
            return;
        }

        Vector3 position = city.CellToWorld(cell.x, cell.y, 0.42f);
        GameObject fragmentObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        fragmentObject.name = "LocalGrid_DataFragment_" + fragmentIndex;
        fragmentObject.transform.SetParent(transform);
        fragmentObject.transform.position = position;
        fragmentObject.transform.localScale = new Vector3(0.42f, 0.16f, 0.42f);

        Collider fragmentCollider = fragmentObject.GetComponent<Collider>();

        if (fragmentCollider != null)
        {
            if (Application.isPlaying)
            {
                Destroy(fragmentCollider);
            }
            else
            {
                DestroyImmediate(fragmentCollider);
            }
        }

        Renderer fragmentRenderer = fragmentObject.GetComponent<Renderer>();

        if (fragmentRenderer != null)
        {
            ApplyRendererColor(fragmentRenderer, new Color(0.25f, 0.88f, 0.95f, 1f));
        }

        GameObject labelObject = new GameObject("DataFragment_Label");
        labelObject.transform.SetParent(fragmentObject.transform, false);
        labelObject.transform.localPosition = new Vector3(0f, 2.2f, 0f);

        TextMesh label = labelObject.AddComponent<TextMesh>();
        label.text = "DATA";
        label.fontSize = 34;
        label.characterSize = 0.09f;
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.color = new Color(0.5f, 0.98f, 1f, 1f);

        EnvironmentalFragment fragment = fragmentObject.AddComponent<EnvironmentalFragment>();
        fragment.Configure(fragmentIndex, title, text);
        spawnedDiscoveryCells.Add(cell);
    }

    bool TryFindChapterPlacementCell(Vector2Int landmarkCell, ChapterPlacementKind kind, out Vector2Int selected)
    {
        selected = landmarkCell;
        List<Vector2Int> walkableCells = city.GetWalkableCells();
        float bestScore = float.MaxValue;

        for (int i = 0; i < walkableCells.Count; i++)
        {
            Vector2Int cell = walkableCells[i];
            float distance = Vector2Int.Distance(landmarkCell, cell);

            if (distance < 2f || distance > 7f || spawnedInfrastructureCells.Contains(cell))
            {
                continue;
            }

            if (kind == ChapterPlacementKind.Resource)
            {
                if (IsTooCloseToCells(cell, spawnedResourceCells, 2) || IsTooCloseToCells(cell, spawnedHazardCells, 2))
                {
                    continue;
                }
            }
            else if (kind == ChapterPlacementKind.Hazard)
            {
                if (city.IsCellPlaza(cell.x, cell.y) ||
                    IsTooCloseToCells(cell, spawnedHazardCells, 3) ||
                    IsTooCloseToCells(cell, spawnedResourceCells, 2))
                {
                    continue;
                }
            }
            else if (IsTooCloseToCells(cell, spawnedDiscoveryCells, 3) ||
                     IsTooCloseToCells(cell, spawnedHazardCells, 2))
            {
                continue;
            }

            float score = Mathf.Abs(distance - (kind == ChapterPlacementKind.Hazard ? 4.5f : 3f));

            if (kind == ChapterPlacementKind.Hazard && (city.IsCellAlley(cell.x, cell.y) || city.IsCellSideStreet(cell.x, cell.y)))
            {
                score -= 1.25f;
            }
            else if (kind != ChapterPlacementKind.Hazard && HasNonWalkableCellNearby(cell, 1))
            {
                score -= 0.4f;
            }

            score += Random.Range(0f, 0.4f);

            if (score < bestScore)
            {
                bestScore = score;
                selected = cell;
            }
        }

        return bestScore < float.MaxValue;
    }

    enum ChapterPlacementKind
    {
        Resource,
        Hazard,
        Discovery
    }

    void RecordRequiredChainCell(InfrastructureNodeType nodeType, Vector2Int cell)
    {
        switch (nodeType)
        {
            case InfrastructureNodeType.PowerJunction:
                requiredPowerCell = cell;
                break;

            case InfrastructureNodeType.TransitLift:
                requiredTransitCell = cell;
                break;

            default:
                requiredSignalCell = cell;
                break;
        }
    }

    void ValidateOpeningPacing()
    {
        if (!validateOpeningPacing)
        {
            return;
        }

        List<string> issues = new List<string>();

        if (city.useIntentionalDistrictSkeleton)
        {
            ValidateRequiredSectorLayout(issues);
        }
        else
        {
            ValidateRequiredDistance("Signal relay", requiredSignalCell, signalRelayMaxCellDistance, issues);
            ValidateRequiredDistance("Power junction", requiredPowerCell, powerJunctionMaxCellDistance, issues);
            ValidateRequiredDistance("Transit lift", requiredTransitCell, transitLiftMaxCellDistance, issues);
        }

        ValidateRequiredCourtyard("Signal relay", requiredSignalCell, issues);
        ValidateRequiredCourtyard("Power junction", requiredPowerCell, issues);
        ValidateRequiredCourtyard("Transit lift", requiredTransitCell, issues);
        if (!city.useIntentionalDistrictSkeleton)
        {
            ValidateRequiredServiceCourt("Signal relay", InfrastructureNodeType.SignalRelay, issues);
            ValidateRequiredServiceCourt("Power junction", InfrastructureNodeType.PowerJunction, issues);
            ValidateRequiredServiceCourt("Transit lift", InfrastructureNodeType.TransitLift, issues);
        }

        if (city.useIntentionalDistrictSkeleton &&
            city.GetClosestMainStreetDistanceFromBase() > city.baseCampRadius + city.mainStreetRadius + 3f)
        {
            issues.Add("workshop does not have immediate access to a main street");
        }

        if (!city.useIntentionalDistrictSkeleton &&
            !city.OpeningRelayChainChangesDirection(minimumOpeningRelayDirectionChange))
        {
            issues.Add("relay route does not arc through distinct service branches");
        }

        int courtyardCount = city.GetCourtyardAnchorCells().Count;

        if (!city.generateNonRelayCourtyards && courtyardCount != 3)
        {
            issues.Add("opening chapter produced " + courtyardCount + " connected relay courtyards instead of 3");
        }

        if (!spawnSupplementalInfrastructureNodes && spawnedInfrastructureCells.Count != 3)
        {
            issues.Add("opening chapter produced " + spawnedInfrastructureCells.Count + " infrastructure nodes instead of 3");
        }

        if (spawnedRootSupplyCells.Count < 2)
        {
            issues.Add("root relay repair supplies were not both placed");
        }
        else
        {
            for (int i = 0; i < spawnedRootSupplyCells.Count; i++)
            {
                if (city.GetDistanceFromBase(spawnedRootSupplyCells[i]) > rootSupplyMaxCellDistance)
                {
                    issues.Add("a root relay repair supply spawned beyond its close-start range");
                    break;
                }
            }
        }

        int familiarResourceCount = CountResourcesInDistanceBand(minimumDistanceFromBase + 1f, familiarResourceMaximumCellDistance);

        if (familiarResourceCount < guaranteedFamiliarResourceClusters)
        {
            issues.Add("familiar low-risk resource band did not receive enough salvage");
        }

        if (!city.useIntentionalDistrictSkeleton &&
            IsRecordedCell(requiredSignalCell) && IsRecordedCell(requiredPowerCell) &&
            city.GetDistanceFromBase(requiredPowerCell) - city.GetDistanceFromBase(requiredSignalCell) < minimumRequiredRelayStageGap)
        {
            issues.Add("Power junction is too close to the Signal relay progression band");
        }
        else if (!city.useIntentionalDistrictSkeleton &&
            IsRecordedCell(requiredSignalCell) && IsRecordedCell(requiredPowerCell) &&
            city.GetDistanceFromBase(requiredPowerCell) - city.GetDistanceFromBase(requiredSignalCell) > maximumRequiredRelayStageGap)
        {
            issues.Add("Power junction is too far beyond the Signal relay progression band");
        }

        if (!city.useIntentionalDistrictSkeleton &&
            IsRecordedCell(requiredPowerCell) && IsRecordedCell(requiredTransitCell) &&
            city.GetDistanceFromBase(requiredTransitCell) - city.GetDistanceFromBase(requiredPowerCell) < minimumRequiredRelayStageGap)
        {
            issues.Add("Transit lift is too close to the Power junction progression band");
        }
        else if (!city.useIntentionalDistrictSkeleton &&
            IsRecordedCell(requiredPowerCell) && IsRecordedCell(requiredTransitCell) &&
            city.GetDistanceFromBase(requiredTransitCell) - city.GetDistanceFromBase(requiredPowerCell) > maximumRequiredRelayStageGap)
        {
            issues.Add("Transit lift is too far beyond the Power junction progression band");
        }

        string summary =
            "Opening run pacing: courtyards " + courtyardCount + "/3, nodes " + spawnedInfrastructureCells.Count + "/3, " +
            "root supplies " + spawnedRootSupplyCells.Count + "/2, " +
            "familiar salvage " + familiarResourceCount + "/" + guaranteedFamiliarResourceClusters + ", " +
            "Signal " + GetRecordedDistanceText(requiredSignalCell) + ", " +
            "Power " + GetRecordedDistanceText(requiredPowerCell) + ", " +
            "Transit " + GetRecordedDistanceText(requiredTransitCell) + ".";

        if (issues.Count > 0)
        {
            Debug.LogWarning(summary + " Issues: " + string.Join("; ", issues) + ".");
        }
        else if (logSuccessfulOpeningPacing)
        {
            Debug.Log(summary);
        }
    }

    void ValidateRequiredDistance(string label, Vector2Int cell, int maxDistance, List<string> issues)
    {
        if (!IsRecordedCell(cell))
        {
            issues.Add(label + " was not placed");
            return;
        }

        if (city.GetDistanceFromBase(cell) > maxDistance)
        {
            issues.Add(label + " spawned beyond its opening-run range");
        }
    }

    void ValidateRequiredSectorLayout(List<string> issues)
    {
        if (!IsRecordedCell(requiredSignalCell) ||
            !IsRecordedCell(requiredPowerCell) ||
            !IsRecordedCell(requiredTransitCell))
        {
            issues.Add("sector relay layout did not place all three required relays");
            return;
        }

        float signalDistance = city.GetDistanceFromBase(requiredSignalCell);
        float powerDistance = city.GetDistanceFromBase(requiredPowerCell);
        float transitDistance = city.GetDistanceFromBase(requiredTransitCell);

        if (signalDistance > powerDistance + 3f)
        {
            issues.Add("Signal relay sector is not the closest opening district");
        }

        if (powerDistance > transitDistance + 4f)
        {
            issues.Add("Power junction sector is not meaningfully before the transit sector");
        }

        Vector2 basePosition = new Vector2(city.GetBaseCell().x, city.GetBaseCell().y);
        Vector2 signalDirection = (new Vector2(requiredSignalCell.x, requiredSignalCell.y) - basePosition).normalized;
        Vector2 powerDirection = (new Vector2(requiredPowerCell.x, requiredPowerCell.y) - basePosition).normalized;
        Vector2 transitDirection = (new Vector2(requiredTransitCell.x, requiredTransitCell.y) - basePosition).normalized;
        float minimumSectorSeparation = 55f;

        if (Vector2.Angle(signalDirection, powerDirection) < minimumSectorSeparation ||
            Vector2.Angle(powerDirection, transitDirection) < minimumSectorSeparation ||
            Vector2.Angle(transitDirection, signalDirection) < minimumSectorSeparation)
        {
            issues.Add("required relay courtyards are not spread across distinct map sectors");
        }
    }

    void ValidateRequiredCourtyard(string label, Vector2Int cell, List<string> issues)
    {
        if (placePrimaryRelaysInCourtyards && IsRecordedCell(cell) && !city.IsCellPlaza(cell.x, cell.y))
        {
            issues.Add(label + " did not spawn in a courtyard");
        }
    }

    void ValidateRequiredServiceCourt(string label, InfrastructureNodeType nodeType, List<string> issues)
    {
        if (placePrimaryRelaysInCourtyards && !city.IsOpeningRelayCourtyardClearOfMainStreet(nodeType))
        {
            issues.Add(label + " courtyard is exposed to a main street instead of a service approach");
        }
    }

    bool IsRecordedCell(Vector2Int cell)
    {
        return cell.x >= 0 && cell.y >= 0;
    }

    string GetRecordedDistanceText(Vector2Int cell)
    {
        if (!IsRecordedCell(cell))
        {
            return "missing";
        }

        return Mathf.RoundToInt(city.GetDistanceFromBase(cell) * city.GetCellSize()) + "m";
    }

    int CountResourcesInDistanceBand(float minimumDistance, float maximumDistance)
    {
        int count = 0;

        for (int i = 0; i < spawnedResourceCells.Count; i++)
        {
            float distance = city.GetDistanceFromBase(spawnedResourceCells[i]);

            if (distance >= minimumDistance && distance <= maximumDistance)
            {
                count++;
            }
        }

        return count;
    }

    void EnsureLookupCaches()
    {
        if (lookupCachesValid)
        {
            return;
        }

        RebuildLookupCaches();
    }

    void RebuildLookupCaches()
    {
        RebuildLookupCache(spawnedResourceCells, spawnedResourceLookup);
        RebuildLookupCache(spawnedHazardCells, spawnedHazardLookup);
        RebuildLookupCache(spawnedInfrastructureCells, spawnedInfrastructureLookup);
        lookupCachesValid = true;
    }

    void RebuildLookupCache(List<Vector2Int> source, HashSet<Vector2Int> target)
    {
        target.Clear();

        for (int i = 0; i < source.Count; i++)
        {
            target.Add(source[i]);
        }
    }

    void InvalidateLookupCaches()
    {
        lookupCachesValid = false;
        spawnedResourceLookup.Clear();
        spawnedHazardLookup.Clear();
        spawnedInfrastructureLookup.Clear();
    }

    ItemType PickGeneratedResourceType(Vector2Int cell)
    {
        int sectorIndex = city.GetRestorationDistrictIndex(cell);

        if (sectorIndex == 2)
        {
            int roll = Random.Range(0, 4);

            if (roll <= 1) return ItemType.CoreFragment;
            if (roll == 2) return ItemType.Wiring;
            return ItemType.MetalScrap;
        }

        if (sectorIndex == 1)
        {
            int roll = Random.Range(0, 4);

            if (roll == 0) return ItemType.CoreFragment;
            if (roll <= 2) return ItemType.Wiring;
            return ItemType.MetalScrap;
        }

        float distance = city.GetDistanceFromBase(cell);
        float farThreshold = Mathf.Max(city.GetGridWidth(), city.GetGridHeight()) * 0.35f;

        if (distance > farThreshold)
        {
            int roll = Random.Range(0, 3);

            if (roll == 0) return ItemType.CoreFragment;
            if (roll == 1) return ItemType.Wiring;
            return ItemType.MetalScrap;
        }

        return Random.value < 0.7f ? ItemType.MetalScrap : ItemType.Wiring;
    }

    int PickGeneratedResourceAmount(Vector2Int cell)
    {
        int sectorIndex = city.GetRestorationDistrictIndex(cell);

        if (sectorIndex == 2)
        {
            return Random.Range(2, 5);
        }

        if (sectorIndex == 1)
        {
            return Random.Range(1, 4);
        }

        float distance = city.GetDistanceFromBase(cell);
        float farThreshold = Mathf.Max(city.GetGridWidth(), city.GetGridHeight()) * 0.35f;

        if (distance > farThreshold)
        {
            return Random.Range(1, 4);
        }

        return Random.Range(1, 3);
    }

    HazardZoneType PickGeneratedHazardType(Vector2Int cell)
    {
        if (city.IsCellAlley(cell.x, cell.y))
        {
            return Random.value < 0.65f
                ? HazardZoneType.CorruptedSignal
                : HazardZoneType.ElectromagneticInterference;
        }

        if (city.IsCellMainStreet(cell.x, cell.y))
        {
            return Random.value < 0.6f
                ? HazardZoneType.UnstablePower
                : HazardZoneType.CoolantLeak;
        }

        int roll = Random.Range(0, 4);

        if (roll == 0) return HazardZoneType.ElectromagneticInterference;
        if (roll == 1) return HazardZoneType.CoolantLeak;
        if (roll == 2) return HazardZoneType.UnstablePower;

        return HazardZoneType.CorruptedSignal;
    }

    string PickResourceSourceContext(Vector2Int cell)
    {
        if (IsResourceOpportunityCell(cell))
        {
            return PickResourceOpportunityContext(cell);
        }

        if (city.IsCellPlaza(cell.x, cell.y))
        {
            return "an abandoned supply cache";
        }

        if (city.IsCellAlley(cell.x, cell.y))
        {
            return Random.value < 0.5f ? "robot remains" : "collapsed service conduit";
        }

        if (city.IsCellSideStreet(cell.x, cell.y))
        {
            return Random.value < 0.5f ? "maintenance wreckage" : "broken utility cabinet";
        }

        return "city wreckage";
    }

    string PickResourceOpportunityContext(Vector2Int cell)
    {
        int sectorIndex = city.GetRestorationDistrictIndex(cell);

        if (sectorIndex == 2)
        {
            int roll = Random.Range(0, 4);

            if (roll == 0) return "ruined factory";
            if (roll == 1) return "crashed transport";
            if (roll == 2) return "robot remains";
            return "transit maintenance alcove";
        }

        if (sectorIndex == 1)
        {
            int roll = Random.Range(0, 4);

            if (roll == 0) return "old power hub";
            if (roll == 1) return "machine shop";
            if (roll == 2) return "storage room";
            return "broken utility cabinet";
        }

        return Random.value < 0.5f ? "abandoned depot" : "maintenance alcove";
    }

    string GetResourceSourceName(string context)
    {
        return context.Replace(" ", "_");
    }

    PlayerSystemType GetAffectedSystemForHazard(HazardZoneType hazardType)
    {
        switch (hazardType)
        {
            case HazardZoneType.CoolantLeak:
                return PlayerSystemType.Mobility;

            case HazardZoneType.CorruptedSignal:
                return PlayerSystemType.Perception;

            default:
                return PlayerSystemType.Core;
        }
    }

    InfrastructureNodeType PickInfrastructureNodeType(int index)
    {
        int roll = index % 3;

        if (roll == 1) return InfrastructureNodeType.PowerJunction;
        if (roll == 2) return InfrastructureNodeType.TransitLift;

        return InfrastructureNodeType.SignalRelay;
    }

    ItemCost[] GetRepairCostsForNode(InfrastructureNodeType nodeType)
    {
        switch (nodeType)
        {
            case InfrastructureNodeType.PowerJunction:
                return new ItemCost[]
                {
                    new ItemCost(ItemType.MetalScrap, 2),
                    new ItemCost(ItemType.Wiring, 1)
                };

            case InfrastructureNodeType.TransitLift:
                return new ItemCost[]
                {
                    new ItemCost(ItemType.MetalScrap, 2),
                    new ItemCost(ItemType.CoreFragment, 1)
                };

            default:
                return new ItemCost[]
                {
                    new ItemCost(ItemType.Wiring, 1),
                    new ItemCost(ItemType.MetalScrap, 1)
                };
        }
    }

    bool IsStarterResourceContext(string sourceContext)
    {
        return
            sourceContext.Contains("base") ||
            sourceContext.Contains("relay") ||
            sourceContext.Contains("reactivation") ||
            sourceContext.Contains("signal-route") ||
            sourceContext.Contains("utility trunk") ||
            sourceContext.Contains("machine frame");
    }

    Vector3 GetResourceSourceScale(string sourceContext, bool starterCache)
    {
        if (sourceContext.Contains("robot remains"))
        {
            return new Vector3(1.2f, 0.34f, 0.75f);
        }

        if (sourceContext.Contains("cabinet") || sourceContext.Contains("cache"))
        {
            return starterCache ? new Vector3(1.15f, 0.75f, 0.9f) : new Vector3(0.9f, 0.65f, 0.75f);
        }

        if (sourceContext.Contains("conduit") || sourceContext.Contains("trunk"))
        {
            return new Vector3(1.45f, 0.28f, 0.55f);
        }

        if (sourceContext.Contains("machine"))
        {
            return new Vector3(1.25f, 0.55f, 1f);
        }

        return starterCache ? new Vector3(1.15f, 0.48f, 1.15f) : new Vector3(0.9f, 0.38f, 0.9f);
    }

    PrimitiveType GetResourceSourceShape(string sourceContext, ItemType itemType)
    {
        if (sourceContext.Contains("robot remains") || sourceContext.Contains("conduit") || sourceContext.Contains("trunk"))
        {
            return PrimitiveType.Cylinder;
        }

        if (sourceContext.Contains("machine"))
        {
            return PrimitiveType.Sphere;
        }

        return itemType == ItemType.Wiring ? PrimitiveType.Cylinder : PrimitiveType.Cube;
    }

    Vector3 GetResourceMarkerPosition(string sourceContext)
    {
        if (sourceContext.Contains("conduit") || sourceContext.Contains("trunk"))
        {
            return new Vector3(0f, 0.65f, 0f);
        }

        if (sourceContext.Contains("robot remains"))
        {
            return new Vector3(0.28f, 0.7f, 0f);
        }

        return new Vector3(0f, 0.75f, 0f);
    }

    Vector3 GetResourceMarkerScale(string sourceContext, ItemType itemType)
    {
        if (sourceContext.Contains("conduit") || sourceContext.Contains("trunk"))
        {
            return new Vector3(0.16f, 0.9f, 0.16f);
        }

        if (sourceContext.Contains("robot remains"))
        {
            return new Vector3(0.24f, 0.68f, 0.24f);
        }

        return itemType == ItemType.Wiring
            ? new Vector3(0.18f, 0.55f, 0.18f)
            : new Vector3(0.42f, 0.42f, 0.42f);
    }

    string GetResourceSourceLabel(ItemType itemType, string sourceContext)
    {
        if (sourceContext.Contains("robot remains"))
        {
            return "ROBOT";
        }

        if (sourceContext.Contains("cabinet") || sourceContext.Contains("cache") || sourceContext.Contains("depot") || sourceContext.Contains("storage"))
        {
            return "DEPOT";
        }

        if (sourceContext.Contains("machine") || sourceContext.Contains("factory"))
        {
            return "MACH";
        }

        if (sourceContext.Contains("power") || sourceContext.Contains("transport") || sourceContext.Contains("transit"))
        {
            return "HUB";
        }

        if (sourceContext.Contains("conduit") || sourceContext.Contains("trunk") || sourceContext.Contains("alcove"))
        {
            return "CABLE";
        }

        return GetResourceShortLabel(itemType);
    }

    void ApplyHazardPlaceholderVisuals(GameObject hazardObject, HazardZone hazard)
    {
        if (hazardObject == null || hazard == null)
        {
            return;
        }

        hazardObject.name = "Generated_HazardZone_" + hazard.hazardType;

        switch (hazard.affectedSystem)
        {
            case PlayerSystemType.Mobility:
                hazardObject.transform.localScale = new Vector3(4.8f, 0.08f, 2.6f);
                break;

            case PlayerSystemType.Perception:
                hazardObject.transform.localScale = new Vector3(3.4f, 0.1f, 3.4f);
                break;

            default:
                hazardObject.transform.localScale = new Vector3(3.1f, 0.12f, 3.1f);
                break;
        }

        GameObject marker = GameObject.CreatePrimitive(GetHazardMarkerShape(hazard.affectedSystem));
        marker.name = "HazardVisual_" + hazard.affectedSystem;
        marker.transform.SetParent(hazardObject.transform);
        marker.transform.localPosition = new Vector3(0f, 1.05f, 0f);
        marker.transform.localScale = hazard.affectedSystem == PlayerSystemType.Mobility
            ? new Vector3(0.28f, 0.5f, 0.28f)
            : new Vector3(0.42f, 0.42f, 0.42f);

        Collider markerCollider = marker.GetComponent<Collider>();

        if (markerCollider != null)
        {
            if (Application.isPlaying)
            {
                Destroy(markerCollider);
            }
            else
            {
                DestroyImmediate(markerCollider);
            }
        }

        Renderer markerRenderer = marker.GetComponent<Renderer>();

        if (markerRenderer != null)
        {
            Renderer baseRenderer = hazardObject.GetComponent<Renderer>();
            markerRenderer.sharedMaterial = baseRenderer != null ? baseRenderer.sharedMaterial : markerRenderer.sharedMaterial;
            ApplyRendererColor(markerRenderer, GetHazardVisualColor(hazard.affectedSystem));
        }

        GameObject labelObject = new GameObject("HazardVisual_Label");
        labelObject.transform.SetParent(hazardObject.transform);
        labelObject.transform.localPosition = new Vector3(0f, 1.48f, 0f);

        TextMesh label = labelObject.AddComponent<TextMesh>();
        label.text = GetHazardShortLabel(hazard.affectedSystem);
        label.fontSize = 34;
        label.characterSize = 0.055f;
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.color = GetHazardVisualColor(hazard.affectedSystem);
    }

    PrimitiveType GetHazardMarkerShape(PlayerSystemType systemType)
    {
        switch (systemType)
        {
            case PlayerSystemType.Mobility:
                return PrimitiveType.Cylinder;

            case PlayerSystemType.Perception:
                return PrimitiveType.Sphere;

            default:
                return PrimitiveType.Cube;
        }
    }

    string GetHazardShortLabel(PlayerSystemType systemType)
    {
        switch (systemType)
        {
            case PlayerSystemType.Mobility:
                return "MOB";

            case PlayerSystemType.Perception:
                return "SENS";

            default:
                return "CORE";
        }
    }

    Color GetHazardVisualColor(PlayerSystemType systemType)
    {
        switch (systemType)
        {
            case PlayerSystemType.Mobility:
                return new Color(1f, 0.72f, 0.16f, 1f);

            case PlayerSystemType.Perception:
                return new Color(0.42f, 0.68f, 1f, 1f);

            default:
                return new Color(1f, 0.2f, 0.12f, 1f);
        }
    }

    float PickHazardDamagePerSecond(HazardZoneType hazardType)
    {
        switch (hazardType)
        {
            case HazardZoneType.CoolantLeak:
                return 8f;

            case HazardZoneType.CorruptedSignal:
                return 6.5f;

            case HazardZoneType.UnstablePower:
                return 12f;

            default:
                return 9f;
        }
    }
}
