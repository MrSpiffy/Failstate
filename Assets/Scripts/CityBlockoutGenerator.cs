using System.Collections.Generic;
using UnityEngine;

public enum CityGenerationMode
{
    Procedural,
    TemplateGuided
}

public class StarterAreaLayoutTemplate
{
    public Vector2Int baseOffset;
    public Vector2Int gateOffset;
    public readonly List<StarterAreaSectorTemplate> sectors = new List<StarterAreaSectorTemplate>();
    public readonly List<StarterAreaRouteTemplate> mainRoutes = new List<StarterAreaRouteTemplate>();

    public static StarterAreaLayoutTemplate CreateDefault()
    {
        StarterAreaLayoutTemplate template = new StarterAreaLayoutTemplate
        {
            baseOffset = new Vector2Int(0, 1),
            gateOffset = new Vector2Int(11, -53)
        };

        template.sectors.Add(new StarterAreaSectorTemplate(
            "Signal",
            0,
            new Vector2Int(-28, -4),
            23,
            new[]
            {
                new Vector2Int(-18, -14),
                new Vector2Int(-34, 8),
                new Vector2Int(-16, 10)
            },
            new[]
            {
                new Vector2Int(-36, -10),
                new Vector2Int(-25, 12),
                new Vector2Int(-12, -9)
            }));

        template.sectors.Add(new StarterAreaSectorTemplate(
            "Power",
            1,
            new Vector2Int(33, 25),
            27,
            new[]
            {
                new Vector2Int(22, 31),
                new Vector2Int(45, 18),
                new Vector2Int(29, 8),
                new Vector2Int(47, 34)
            },
            new[]
            {
                new Vector2Int(17, 22),
                new Vector2Int(40, 9),
                new Vector2Int(52, 27),
                new Vector2Int(28, 39)
            }));

        template.sectors.Add(new StarterAreaSectorTemplate(
            "Transit",
            2,
            new Vector2Int(8, -45),
            32,
            new[]
            {
                new Vector2Int(-7, -35),
                new Vector2Int(20, -34),
                new Vector2Int(1, -52),
                new Vector2Int(27, -49),
                new Vector2Int(13, -20)
            },
            new[]
            {
                new Vector2Int(-12, -43),
                new Vector2Int(24, -41),
                new Vector2Int(8, -57),
                new Vector2Int(31, -25),
                new Vector2Int(-2, -24)
            }));

        template.mainRoutes.Add(new StarterAreaRouteTemplate(0, new[]
        {
            new Vector2Int(0, 1),
            new Vector2Int(-9, 2),
            new Vector2Int(-20, -1),
            new Vector2Int(-28, -4)
        }));

        template.mainRoutes.Add(new StarterAreaRouteTemplate(1, new[]
        {
            new Vector2Int(0, 1),
            new Vector2Int(8, 8),
            new Vector2Int(22, 17),
            new Vector2Int(33, 25)
        }));

        template.mainRoutes.Add(new StarterAreaRouteTemplate(2, new[]
        {
            new Vector2Int(0, 1),
            new Vector2Int(5, -11),
            new Vector2Int(7, -29),
            new Vector2Int(8, -45)
        }));

        template.mainRoutes.Add(new StarterAreaRouteTemplate(2, new[]
        {
            new Vector2Int(8, -45),
            new Vector2Int(10, -50),
            new Vector2Int(11, -53)
        }));

        return template;
    }
}

public class StarterAreaSectorTemplate
{
    public readonly string name;
    public readonly int difficultyIndex;
    public readonly Vector2Int relayOffset;
    public readonly int maskRadius;
    public readonly List<Vector2Int> neighborhoodOffsets = new List<Vector2Int>();
    public readonly List<Vector2Int> resourceOpportunityOffsets = new List<Vector2Int>();

    public StarterAreaSectorTemplate(
        string name,
        int difficultyIndex,
        Vector2Int relayOffset,
        int maskRadius,
        IEnumerable<Vector2Int> neighborhoodOffsets,
        IEnumerable<Vector2Int> resourceOpportunityOffsets)
    {
        this.name = name;
        this.difficultyIndex = difficultyIndex;
        this.relayOffset = relayOffset;
        this.maskRadius = maskRadius;
        this.neighborhoodOffsets.AddRange(neighborhoodOffsets);
        this.resourceOpportunityOffsets.AddRange(resourceOpportunityOffsets);
    }
}

public class StarterAreaRouteTemplate
{
    public readonly int sectorIndex;
    public readonly List<Vector2Int> pointOffsets = new List<Vector2Int>();

    public StarterAreaRouteTemplate(int sectorIndex, IEnumerable<Vector2Int> pointOffsets)
    {
        this.sectorIndex = sectorIndex;
        this.pointOffsets.AddRange(pointOffsets);
    }
}

public class StarterAreaLayoutPlan
{
    public Vector2Int baseCell;
    public Vector2Int gateCell;
    public readonly List<StarterAreaSectorPlan> sectors = new List<StarterAreaSectorPlan>();
    public readonly List<StarterAreaRoutePlan> mainRoutes = new List<StarterAreaRoutePlan>();
}

public class StarterAreaSectorPlan
{
    public string name;
    public int difficultyIndex;
    public Vector2Int relayCell;
    public int maskRadius;
    public readonly List<Vector2Int> neighborhoodCells = new List<Vector2Int>();
    public readonly List<Vector2Int> resourceOpportunityCells = new List<Vector2Int>();
}

public class StarterAreaRoutePlan
{
    public int sectorIndex;
    public readonly List<Vector2Int> points = new List<Vector2Int>();
}

public class CityBlockoutGenerator : MonoBehaviour
{
    [SerializeField] private List<Vector2Int> savedWalkableCells = new List<Vector2Int>();
    [SerializeField] private List<Vector2Int> savedPlazaCells = new List<Vector2Int>();
    [SerializeField] private List<Vector2Int> savedMainStreetCells = new List<Vector2Int>();
    [SerializeField] private List<Vector2Int> savedSideStreetCells = new List<Vector2Int>();
    [SerializeField] private List<Vector2Int> savedAlleyCells = new List<Vector2Int>();
    [SerializeField] private List<Vector2Int> savedStarterAreaCells = new List<Vector2Int>();
    [SerializeField] private List<Vector2Int> savedResourceOpportunityCells = new List<Vector2Int>();
    [SerializeField] private List<Vector2Int> savedLandmarkAnchorCells = new List<Vector2Int>();
    [SerializeField] private List<Vector2Int> savedHazardAnchorCells = new List<Vector2Int>();
    [SerializeField] private List<Vector2Int> savedEnemyEncounterAnchorCells = new List<Vector2Int>();
    [SerializeField] private List<Vector2Int> savedVantageAnchorCells = new List<Vector2Int>();
    [SerializeField] private List<Vector2Int> savedSignalDistrictCells = new List<Vector2Int>();
    [SerializeField] private List<Vector2Int> savedPowerDistrictCells = new List<Vector2Int>();
    [SerializeField] private List<Vector2Int> savedTransitDistrictCells = new List<Vector2Int>();
    [SerializeField] private Vector2Int savedBaseCell = new Vector2Int(-1, -1);
    [SerializeField] private Vector2Int savedOpeningSignalCourtyardCell = new Vector2Int(-1, -1);
    [SerializeField] private Vector2Int savedOpeningPowerCourtyardCell = new Vector2Int(-1, -1);
    [SerializeField] private Vector2Int savedOpeningTransitCourtyardCell = new Vector2Int(-1, -1);
    private readonly HashSet<Vector2Int> walkableCellLookup = new HashSet<Vector2Int>();
    private readonly HashSet<Vector2Int> plazaCellLookup = new HashSet<Vector2Int>();
    private readonly HashSet<Vector2Int> mainStreetCellLookup = new HashSet<Vector2Int>();
    private readonly HashSet<Vector2Int> sideStreetCellLookup = new HashSet<Vector2Int>();
    private readonly HashSet<Vector2Int> alleyCellLookup = new HashSet<Vector2Int>();
    private readonly HashSet<Vector2Int> starterAreaCellLookup = new HashSet<Vector2Int>();
    private readonly HashSet<Vector2Int> signalDistrictCellLookup = new HashSet<Vector2Int>();
    private readonly HashSet<Vector2Int> powerDistrictCellLookup = new HashSet<Vector2Int>();
    private readonly HashSet<Vector2Int> transitDistrictCellLookup = new HashSet<Vector2Int>();
    private bool lookupCachesValid = false;

    [Header("Map Size")]
    public int width = 72;
    public int height = 72;
    public float cellSize = 4f;
    public int minimumIntentionalMapWidth = 128;
    public int minimumIntentionalMapHeight = 112;

    [Header("Generation")]
    public int seed = 12345;
    public bool randomizeSeed = true;
    public CityGenerationMode generationMode = CityGenerationMode.TemplateGuided;
    public bool generateUpperCityLayer = true;
    public UpperCityLayerGenerator upperCityLayerGenerator;

    [Header("Template Guided Layout")]
    public bool mirrorTemplateHorizontally = false;
    public bool randomizeTemplateMirror = true;
    public bool jitterTemplateCells = true;
    public int templateJitterCells = 2;

    [Header("Auto Scaling")]
    public bool autoScaleGeneration = true;
    public float sideStreetDensity = 0.008f;
    public float alleyDensity = 0.013f;
    public float alleyClusterDensity = 0.0025f;
    public float deadEndDensity = 0.006f;
    public float plazaDensity = 0.0008f;

    [Header("Main Streets")]
    public int mainStreetCount = 3;
    public int mainStreetLength = 45;
    public int mainStreetRadius = 1;

    [Header("Intentional District Structure")]
    public bool useIntentionalDistrictSkeleton = true;
    public int nearZoneLoopRadius = 7;
    public int nearZoneLoopVariation = 2;
    public int arterialBranchCount = 3;
    public int arterialGateInset = 2;
    public int arterialBendDistance = 10;

    [Header("Sector Route Structure")]
    public int signalSectorDistance = 13;
    public int powerSectorDistance = 21;
    public int transitSectorDistance = 30;
    public int sectorLoopRadius = 5;
    public int sectorConnectorRouteCount = 3;
    public int sectorServiceSpokeCount = 3;
    public int sectorMainStreetSetback = 7;
    public int centralSafeZoneRadius = 10;
    public int signalSectorRadius = 15;
    public int powerSectorRadius = 18;
    public int transitSectorRadius = 21;
    public int sectorCorridorWidth = 6;
    public int sectorRingCorridorWidth = 5;
    public int signalSectorResourceOpportunities = 3;
    public int powerSectorResourceOpportunities = 4;
    public int transitSectorResourceOpportunities = 5;
    public int signalSectorNeighborhoods = 3;
    public int powerSectorNeighborhoods = 4;
    public int transitSectorNeighborhoods = 5;

    [Header("Side Streets")]
    public int sideStreetCount = 18;
    public int sideStreetLength = 18;
    public int sideStreetRadius = 0;
    public float sideStreetMainStreetStartChance = 0.75f;
    public int structuredSideStreetInterval = 6;
    public int structuredSideStreetConnectorCount = 8;

    [Header("Alleys")]
    public int alleyCount = 35;
    public int alleyLength = 8;
    public int alleyRadius = 0;

    [Header("Alley Clusters")]
    public int alleyClusterCount = 6;
    public int alleyClusterPaths = 5;
    public int alleyClusterPathLength = 5;

    [Header("Dead Ends")]
    public int deadEndCount = 20;
    public int deadEndLength = 3;

    [Header("District Coverage")]
    public int districtSize = 8;
    public int districtCoveragePasses = 2;
    public int districtAlleyAttempts = 4;
    public float minimumDistrictWalkableRatio = 0.08f;

    [Header("Restoration Districts")]
    public int restorationDistrictInnerRadius = 8;
    public int restorationDistrictVisualSampleCount = 18;
    public int restorationDistrictVisualSampleSpacing = 5;

    [Header("Path Shape")]
    public int minSegmentLength = 4;
    public int maxSegmentLength = 10;
    public float turnChance = 0.18f;
    public float continueStraightChance = 0.72f;
    public float loopConnectionChance = 0.25f;

    [Header("Path Spacing")]
    public int minimumPathSpacing = 2;
    public int maxNearbyWalkableCells = 2;
    public int minimumSamePathSpacing = 3;

    [Header("Open Areas")]
    public int plazaCount = 2;
    public int minPlazaRadius = 4;
    public int maxPlazaRadius = 5;
    public int baseCampRadius = 2;
    public int minimumPlazaSpacing = 12;
    public int minimumRelayCourtyardCount = 3;
    public int additionalOpenCourtyards = 1;
    public bool generateNonRelayCourtyards = false;

    [Header("Base Camp Rules")]
    public int baseExclusionRadius = 7;
    public int baseCampMinimumCenterOffset = 8;
    public int baseCampMaximumCenterOffset = 18;
    public int baseCampPreferredAvenueOffset = 5;

    [Header("Opening Route")]
    public bool carveOpeningSignalCourtyard = true;
    public bool carveOpeningChainCourtyards = true;
    public int openingSignalCourtyardRadius = 2;
    public int openingSignalCourtyardMinDistance = 8;
    public int openingSignalCourtyardMaxDistance = 11;
    public int openingPowerCourtyardRadius = 2;
    public int openingPowerCourtyardMinDistance = 16;
    public int openingPowerCourtyardMaxDistance = 20;
    public int openingTransitCourtyardRadius = 2;
    public int openingTransitCourtyardMinDistance = 24;
    public int openingTransitCourtyardMaxDistance = 28;
    public int openingRelayCourtyardGap = 4;
    public int openingRelayMainStreetClearance = 2;
    public int openingRelayPreferredServiceSpurLength = 3;
    public float openingRelayRouteArcDegrees = 52f;
    public float openingRelayMaxArcDeviationDegrees = 18f;

    [Header("Buildings")]
    public float minBuildingHeight = 18f;
    public float maxBuildingHeight = 42f;
    public float floorThickness = 0.2f;

    [Header("Building Chunking")]
    public int maxBuildingChunkWidth = 4;
    public int maxBuildingChunkDepth = 4;
    public float buildingChunkGap = -0.15f;

    [Header("Debug Visuals")]
    public bool showWalkableMarkers = false;

    [Header("Materials")]
    public Material floorMaterial;
    public Material buildingMaterial;
    public Material baseMaterial;
    public Material walkableMaterial;
    public Material plazaMaterial;

    private bool[,] walkableCells;
    private bool[,] plazaCells;
    private bool[,] starterAreaCells;
    private List<Vector2Int> carvedCells = new List<Vector2Int>();

    private List<Vector2Int> mainStreetCells = new List<Vector2Int>();
    private List<Vector2Int> sideStreetCells = new List<Vector2Int>();
    private List<Vector2Int> alleyCells = new List<Vector2Int>();
    private List<Vector2Int> plazaCenters = new List<Vector2Int>();
    private readonly List<Vector2Int> plannedRelaySectorCells = new List<Vector2Int>();
    private StarterAreaLayoutPlan activeStarterAreaLayoutPlan;
    public bool IsCellPlaza(int x, int y)
    {
        EnsureLookupCaches();
        return plazaCellLookup.Contains(new Vector2Int(x, y));
    }

    public bool IsCellMainStreet(int x, int y)
    {
        EnsureLookupCaches();
        return mainStreetCellLookup.Contains(new Vector2Int(x, y));
    }

    public bool IsCellSideStreet(int x, int y)
    {
        EnsureLookupCaches();
        return sideStreetCellLookup.Contains(new Vector2Int(x, y));
    }

    public bool IsCellAlley(int x, int y)
    {
        EnsureLookupCaches();
        return alleyCellLookup.Contains(new Vector2Int(x, y));
    }

    public bool IsCellInsideStarterArea(int x, int y)
    {
        EnsureLookupCaches();

        if (savedStarterAreaCells == null || savedStarterAreaCells.Count == 0)
        {
            return x >= 0 && x < width && y >= 0 && y < height;
        }

        return starterAreaCellLookup.Contains(new Vector2Int(x, y));
    }

    public List<Vector2Int> GetSavedWalkableCells()
    {
        return new List<Vector2Int>(savedWalkableCells);
    }

    public List<Vector2Int> GetSavedPlazaCells()
    {
        return new List<Vector2Int>(savedPlazaCells);
    }

    public List<Vector2Int> GetResourceOpportunityCells()
    {
        return new List<Vector2Int>(savedResourceOpportunityCells);
    }

    public List<Vector2Int> GetLandmarkAnchorCells()
    {
        return new List<Vector2Int>(savedLandmarkAnchorCells);
    }

    public List<Vector2Int> GetHazardAnchorCells()
    {
        return new List<Vector2Int>(savedHazardAnchorCells);
    }

    public List<Vector2Int> GetEnemyEncounterAnchorCells()
    {
        return new List<Vector2Int>(savedEnemyEncounterAnchorCells);
    }

    public List<Vector2Int> GetVantageAnchorCells()
    {
        return new List<Vector2Int>(savedVantageAnchorCells);
    }

    public List<Vector2Int> GetLandmarkAnchorCellsForDistrict(int districtIndex)
    {
        return FilterAnchorsByDistrict(savedLandmarkAnchorCells, districtIndex);
    }

    public List<Vector2Int> GetHazardAnchorCellsForDistrict(int districtIndex)
    {
        return FilterAnchorsByDistrict(savedHazardAnchorCells, districtIndex);
    }

    public List<Vector2Int> GetEnemyEncounterAnchorCellsForDistrict(int districtIndex)
    {
        return FilterAnchorsByDistrict(savedEnemyEncounterAnchorCells, districtIndex);
    }

    public List<Vector2Int> GetVantageAnchorCellsForDistrict(int districtIndex)
    {
        return FilterAnchorsByDistrict(savedVantageAnchorCells, districtIndex);
    }

    public List<Vector2Int> GetRestorationDistrictCells(int districtIndex)
    {
        if (districtIndex == 0)
        {
            return new List<Vector2Int>(savedSignalDistrictCells);
        }

        if (districtIndex == 1)
        {
            return new List<Vector2Int>(savedPowerDistrictCells);
        }

        if (districtIndex == 2)
        {
            return new List<Vector2Int>(savedTransitDistrictCells);
        }

        return new List<Vector2Int>();
    }

    public List<Vector2Int> GetCourtyardAnchorCells()
    {
        EnsureLookupCaches();

        List<Vector2Int> anchors = new List<Vector2Int>();
        HashSet<Vector2Int> unvisited = new HashSet<Vector2Int>(savedPlazaCells);
        Vector2Int[] neighbors =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        while (unvisited.Count > 0)
        {
            Vector2Int start = default;

            foreach (Vector2Int cell in unvisited)
            {
                start = cell;
                break;
            }

            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            List<Vector2Int> region = new List<Vector2Int>();
            queue.Enqueue(start);
            unvisited.Remove(start);

            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();
                region.Add(current);

                for (int i = 0; i < neighbors.Length; i++)
                {
                    Vector2Int neighbor = current + neighbors[i];

                    if (unvisited.Remove(neighbor))
                    {
                        queue.Enqueue(neighbor);
                    }
                }
            }

            anchors.Add(FindRegionCenterCell(region));
        }

        return anchors;
    }

    public Vector2Int GetBaseCell()
    {
        if (savedBaseCell.x >= 0 && savedBaseCell.x < width && savedBaseCell.y >= 0 && savedBaseCell.y < height)
        {
            return savedBaseCell;
        }

        return new Vector2Int(width / 2, height / 2);
    }

    public Vector2Int GetOpeningSignalCourtyardCell()
    {
        return savedOpeningSignalCourtyardCell;
    }

    public bool HasOpeningSignalCourtyard()
    {
        return IsSavedOpeningCourtyardCell(savedOpeningSignalCourtyardCell);
    }

    public bool TryGetOpeningRelayCourtyardCell(InfrastructureNodeType nodeType, out Vector2Int cell)
    {
        if (nodeType == InfrastructureNodeType.PowerJunction)
        {
            cell = savedOpeningPowerCourtyardCell;
        }
        else if (nodeType == InfrastructureNodeType.TransitLift)
        {
            cell = savedOpeningTransitCourtyardCell;
        }
        else
        {
            cell = savedOpeningSignalCourtyardCell;
        }

        return IsSavedOpeningCourtyardCell(cell);
    }

    public bool TryGetRestorationDistrictAnchor(InfrastructureNodeType nodeType, out Vector2Int cell)
    {
        return TryGetOpeningRelayCourtyardCell(nodeType, out cell);
    }

    public int GetRestorationDistrictIndexForNodeType(InfrastructureNodeType nodeType)
    {
        if (nodeType == InfrastructureNodeType.PowerJunction)
        {
            return 1;
        }

        if (nodeType == InfrastructureNodeType.TransitLift)
        {
            return 2;
        }

        return 0;
    }

    public int GetRestorationDistrictIndex(Vector2Int cell)
    {
        if (!HasGeneratedMap() || IsCellNearBase(cell, restorationDistrictInnerRadius))
        {
            return -1;
        }

        EnsureLookupCaches();

        if (signalDistrictCellLookup.Contains(cell))
        {
            return 0;
        }

        if (powerDistrictCellLookup.Contains(cell))
        {
            return 1;
        }

        if (transitDistrictCellLookup.Contains(cell))
        {
            return 2;
        }

        Vector2Int[] anchors =
        {
            savedOpeningSignalCourtyardCell,
            savedOpeningPowerCourtyardCell,
            savedOpeningTransitCourtyardCell
        };

        int bestIndex = -1;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < anchors.Length; i++)
        {
            if (!IsSavedOpeningCourtyardCell(anchors[i]))
            {
                continue;
            }

            Vector2Int offset = cell - anchors[i];
            float distance = offset.sqrMagnitude;

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    public bool IsCellInRestorationDistrict(int x, int y, InfrastructureNodeType nodeType)
    {
        Vector2Int cell = new Vector2Int(x, y);
        return IsCellWalkable(x, y) &&
               GetRestorationDistrictIndex(cell) == GetRestorationDistrictIndexForNodeType(nodeType);
    }

    public List<Vector2Int> GetRestorationDistrictSampleCells(
        InfrastructureNodeType nodeType,
        int maximumSamples,
        int minimumSpacing)
    {
        List<Vector2Int> candidates = new List<Vector2Int>();
        List<Vector2Int> samples = new List<Vector2Int>();
        int districtIndex = GetRestorationDistrictIndexForNodeType(nodeType);

        for (int i = 0; i < savedWalkableCells.Count; i++)
        {
            Vector2Int cell = savedWalkableCells[i];

            if (GetRestorationDistrictIndex(cell) != districtIndex)
            {
                continue;
            }

            if (IsCellPlaza(cell.x, cell.y))
            {
                continue;
            }

            bool routeFixtureCandidate =
                IsCellMainStreet(cell.x, cell.y) ||
                IsCellSideStreet(cell.x, cell.y) ||
                (IsCellAlley(cell.x, cell.y) && HasNonWalkableNeighbor(cell));

            if (routeFixtureCandidate)
            {
                candidates.Add(cell);
            }
        }

        int attempts = Mathf.Max(maximumSamples * 8, candidates.Count);

        while (samples.Count < maximumSamples && candidates.Count > 0 && attempts > 0)
        {
            attempts--;
            int index = Random.Range(0, candidates.Count);
            Vector2Int candidate = candidates[index];
            candidates.RemoveAt(index);

            if (IsFarEnoughFromCells(candidate, samples, minimumSpacing))
            {
                samples.Add(candidate);
            }
        }

        return samples;
    }

    public bool IsOpeningRelayCourtyardClearOfMainStreet(InfrastructureNodeType nodeType)
    {
        Vector2Int cell;

        if (!TryGetOpeningRelayCourtyardCell(nodeType, out cell))
        {
            return false;
        }

        int radius = nodeType == InfrastructureNodeType.PowerJunction
            ? openingPowerCourtyardRadius
            : nodeType == InfrastructureNodeType.TransitLift
                ? openingTransitCourtyardRadius
                : openingSignalCourtyardRadius;

        return !IsCourtyardNearMainStreet(cell, radius);
    }

    public float GetDistanceFromBase(Vector2Int cell)
    {
        return Vector2Int.Distance(cell, GetBaseCell());
    }

    public float GetClosestMainStreetDistanceFromBase()
    {
        List<Vector2Int> routes = savedMainStreetCells.Count > 0 ? savedMainStreetCells : mainStreetCells;

        if (routes.Count == 0)
        {
            return float.MaxValue;
        }

        return Vector2Int.Distance(GetBaseCell(), FindNearestCell(GetBaseCell(), routes));
    }

    public bool OpeningRelayChainChangesDirection(float minimumDegrees)
    {
        if (!IsSavedOpeningCourtyardCell(savedOpeningSignalCourtyardCell) ||
            !IsSavedOpeningCourtyardCell(savedOpeningPowerCourtyardCell) ||
            !IsSavedOpeningCourtyardCell(savedOpeningTransitCourtyardCell))
        {
            return false;
        }

        Vector2 basePosition = new Vector2(GetBaseCell().x, GetBaseCell().y);
        Vector2 signalDirection = (new Vector2(savedOpeningSignalCourtyardCell.x, savedOpeningSignalCourtyardCell.y) - basePosition).normalized;
        Vector2 powerDirection = (new Vector2(savedOpeningPowerCourtyardCell.x, savedOpeningPowerCourtyardCell.y) - basePosition).normalized;
        Vector2 transitDirection = (new Vector2(savedOpeningTransitCourtyardCell.x, savedOpeningTransitCourtyardCell.y) - basePosition).normalized;

        return Vector2.Angle(signalDirection, powerDirection) >= minimumDegrees &&
               Vector2.Angle(powerDirection, transitDirection) >= minimumDegrees * 0.6f;
    }

    public Vector3 CellToWorldPosition(Vector2Int cell, float yPosition = 0f)
    {
        return CellToWorld(cell.x, cell.y, yPosition);
    }

    public bool TryWorldToCell(Vector3 worldPosition, out Vector2Int cell)
    {
        Vector3 local = worldPosition - transform.position;
        float gridX = (local.x + width * cellSize * 0.5f) / cellSize;
        float gridY = (local.z + height * cellSize * 0.5f) / cellSize;
        cell = new Vector2Int(Mathf.FloorToInt(gridX), Mathf.FloorToInt(gridY));
        return cell.x >= 0 && cell.x < width && cell.y >= 0 && cell.y < height;
    }

    public bool IsCellNearBase(Vector2Int cell, float radius)
    {
        return Vector2Int.Distance(cell, GetBaseCell()) <= radius;
    }

    public bool HasGeneratedMap()
    {
        return savedWalkableCells != null && savedWalkableCells.Count > 0;
    }

    public bool IsCellWalkable(int x, int y)
    {
        EnsureLookupCaches();
        return walkableCellLookup.Contains(new Vector2Int(x, y));
    }

    public Vector3 GetBaseCampApproachDirection()
    {
        Vector2Int baseCell = GetBaseCell();
        List<Vector2Int> routeCells = savedMainStreetCells.Count > 0 ? savedMainStreetCells : mainStreetCells;
        Vector2Int nearestAvenue = FindNearestCell(baseCell, routeCells);
        Vector3 approach = CellToWorldPosition(nearestAvenue, 0f) - CellToWorldPosition(baseCell, 0f);
        approach.y = 0f;

        if (approach.sqrMagnitude < 0.01f)
        {
            Vector2Int center = new Vector2Int(width / 2, height / 2);
            approach = CellToWorldPosition(center, 0f) - CellToWorldPosition(baseCell, 0f);
            approach.y = 0f;
        }

        return approach.sqrMagnitude > 0.01f ? approach.normalized : Vector3.forward;
    }

    public int GetGridWidth()
    {
        return width;
    }

    public int GetGridHeight()
    {
        return height;
    }

    public float GetCellSize()
    {
        return cellSize;
    }

    public float GetWorldSizeX()
    {
        return width * cellSize;
    }

    public float GetWorldSizeZ()
    {
        return height * cellSize;
    }

    public Vector3 GetWorldOrigin()
    {
        return transform.position;
    }

    public List<Vector2Int> GetWalkableCells()
    {
        return new List<Vector2Int>(savedWalkableCells);
    }

    [ContextMenu("Generate City Blockout")]
    public void GenerateCityBlockout()
    {
        ClearPreviousGeneration();

        if (randomizeSeed)
        {
            seed = Random.Range(0, 999999);
        }

        Random.InitState(seed);

        if (autoScaleGeneration)
        {
            ApplyAutoScaledGenerationSettings();
        }

        walkableCells = new bool[width, height];
        plazaCells = new bool[width, height];
        starterAreaCells = new bool[width, height];
        carvedCells.Clear();
        savedWalkableCells.Clear();
        savedPlazaCells.Clear();
        savedMainStreetCells.Clear();
        savedSideStreetCells.Clear();
        savedAlleyCells.Clear();
        savedStarterAreaCells.Clear();
        savedResourceOpportunityCells.Clear();
        ClearGeneratedAnchorCells();
        ClearSavedRestorationDistrictCells();
        plazaCenters.Clear();
        savedOpeningSignalCourtyardCell = new Vector2Int(-1, -1);
        savedOpeningPowerCourtyardCell = new Vector2Int(-1, -1);
        savedOpeningTransitCourtyardCell = new Vector2Int(-1, -1);
        plannedRelaySectorCells.Clear();
        activeStarterAreaLayoutPlan = null;

        mainStreetCells.Clear();
        sideStreetCells.Clear();
        alleyCells.Clear();

        Vector2Int center = new Vector2Int(width / 2, height / 2);
        activeStarterAreaLayoutPlan = ShouldUseTemplateGuidedLayout()
            ? BuildStarterAreaLayoutPlan(center, StarterAreaLayoutTemplate.CreateDefault())
            : null;
        savedBaseCell = useIntentionalDistrictSkeleton
            ? activeStarterAreaLayoutPlan != null ? activeStarterAreaLayoutPlan.baseCell : PickCentralBaseCampSite(center)
            : center;

        if (useIntentionalDistrictSkeleton)
        {
            CarveIntentionalRouteSkeleton(savedBaseCell);
        }
        else
        {
            CarveMainStreets(center);
        }

        CarveSideStreets();
        CarveAlleys();
        CarveAlleyClusters();
        CarveUnderdevelopedDistricts();
        CarveDeadEnds();
        CarveLocalConnections();

        if (!useIntentionalDistrictSkeleton)
        {
            savedBaseCell = PickBaseCampServiceSite(center);
        }

        CarvePlaza(savedBaseCell, baseCampRadius, true);
        CarveBaseCampServiceAccesses(savedBaseCell);
        CarveOpeningRelayCourtyards();

        if (generateNonRelayCourtyards)
        {
            CarveRandomPlazas();
        }

        ConnectDisconnectedWalkableAreas();
        BuildRestorationDistrictOwnership();

        CreateFloor();
        CreateBuildings();
        CreateBaseMarker(savedBaseCell);
        RebuildLookupCaches();

        GeneratedWorldSpawner spawner = FindFirstObjectByType<GeneratedWorldSpawner>();

        if (spawner != null)
        {
            spawner.GenerateSpawnedObjects();
        }

        GenerateUpperCityLayer();
    }

    void GenerateUpperCityLayer()
    {
        if (!generateUpperCityLayer)
        {
            return;
        }

        if (upperCityLayerGenerator == null)
        {
            upperCityLayerGenerator = FindFirstObjectByType<UpperCityLayerGenerator>();
        }

        if (upperCityLayerGenerator == null)
        {
            GameObject upperLayerObject = new GameObject("UpperCityLayerGenerator");
            upperCityLayerGenerator = upperLayerObject.AddComponent<UpperCityLayerGenerator>();
        }

        upperCityLayerGenerator.city = this;
        upperCityLayerGenerator.GenerateUpperCityLayer();
    }
    void ApplyAutoScaledGenerationSettings()
    {
        if (useIntentionalDistrictSkeleton)
        {
            width = Mathf.Max(width, minimumIntentionalMapWidth, 128);
            height = Mathf.Max(height, minimumIntentionalMapHeight, 112);
        }

        float variation = Random.Range(0.85f, 1.15f);
        int mapArea = width * height;
        float longestDimension = Mathf.Max(width, height);

        mainStreetLength = Mathf.RoundToInt(longestDimension * 0.45f);

        sideStreetCount = Mathf.Max(8, Mathf.RoundToInt(mapArea * sideStreetDensity * variation));
        alleyCount = Mathf.Max(12, Mathf.RoundToInt(mapArea * alleyDensity * variation));
        alleyClusterCount = Mathf.Max(3, Mathf.RoundToInt(mapArea * alleyClusterDensity * variation));
        deadEndCount = Mathf.Max(6, Mathf.RoundToInt(mapArea * deadEndDensity * variation));
        plazaCount = Mathf.Max(minimumRelayCourtyardCount + additionalOpenCourtyards, Mathf.RoundToInt(mapArea * plazaDensity * variation));
        minPlazaRadius = Mathf.Max(4, minPlazaRadius);
        maxPlazaRadius = Mathf.Max(minPlazaRadius + 1, maxPlazaRadius);

        sideStreetLength = Mathf.Max(10, Mathf.RoundToInt(longestDimension * 0.28f));
        alleyLength = Mathf.Max(5, Mathf.RoundToInt(longestDimension * 0.13f));
        alleyClusterPathLength = Mathf.Max(4, Mathf.RoundToInt(longestDimension * 0.1f));
        signalSectorDistance = Mathf.Clamp(Mathf.RoundToInt(longestDimension * 0.24f), 26, 34);
        powerSectorDistance = Mathf.Clamp(Mathf.RoundToInt(longestDimension * 0.34f), 38, 48);
        transitSectorDistance = Mathf.Clamp(Mathf.RoundToInt(longestDimension * 0.42f), 48, 58);
        sectorLoopRadius = Mathf.Clamp(Mathf.RoundToInt(longestDimension * 0.075f), 7, 10);
        sectorMainStreetSetback = Mathf.Clamp(Mathf.RoundToInt(longestDimension * 0.045f), 5, 7);
        centralSafeZoneRadius = Mathf.Clamp(Mathf.RoundToInt(longestDimension * 0.12f), 11, 14);
        signalSectorRadius = Mathf.Clamp(Mathf.RoundToInt(longestDimension * 0.21f), 23, 29);
        powerSectorRadius = Mathf.Clamp(Mathf.RoundToInt(longestDimension * 0.23f), 25, 32);
        transitSectorRadius = Mathf.Clamp(Mathf.RoundToInt(longestDimension * 0.27f), 30, 38);
        sectorCorridorWidth = Mathf.Clamp(Mathf.RoundToInt(longestDimension * 0.055f), 5, 7);
        sectorRingCorridorWidth = Mathf.Clamp(Mathf.RoundToInt(longestDimension * 0.035f), 3, 5);
        sectorConnectorRouteCount = 1;
        sectorServiceSpokeCount = 2;

        districtSize = Mathf.Max(7, Mathf.RoundToInt(longestDimension * 0.15f));
        baseExclusionRadius = Mathf.Max(8, Mathf.RoundToInt(longestDimension * 0.16f));
        baseCampMinimumCenterOffset = Mathf.Max(7, Mathf.RoundToInt(longestDimension * 0.11f));
        baseCampMaximumCenterOffset = Mathf.Max(baseCampMinimumCenterOffset + 5, Mathf.RoundToInt(longestDimension * 0.26f));
        baseCampPreferredAvenueOffset = Mathf.Max(baseCampRadius + 2, Mathf.RoundToInt(longestDimension * 0.07f));
        minimumPlazaSpacing = Mathf.Max(maxPlazaRadius * 2 + 5, Mathf.RoundToInt(longestDimension * 0.22f));

        if (useIntentionalDistrictSkeleton)
        {
            nearZoneLoopRadius = Mathf.Clamp(Mathf.RoundToInt(longestDimension * 0.1f), baseCampRadius + 4, 10);
            mainStreetCount = Mathf.Clamp(arterialBranchCount, 2, 4);
            mainStreetRadius = 2;
            sideStreetRadius = 0;
            alleyRadius = 0;
            openingSignalCourtyardRadius = 4;
            openingPowerCourtyardRadius = 5;
            openingTransitCourtyardRadius = 5;
            signalSectorResourceOpportunities = 3;
            powerSectorResourceOpportunities = 4;
            transitSectorResourceOpportunities = 5;
            signalSectorNeighborhoods = 3;
            powerSectorNeighborhoods = 4;
            transitSectorNeighborhoods = 5;
            sideStreetMainStreetStartChance = 0.35f;
            structuredSideStreetConnectorCount = Mathf.Max(4, Mathf.RoundToInt(longestDimension * 0.055f));
            districtCoveragePasses = 1;
            minimumDistrictWalkableRatio = 0.065f;
            maxNearbyWalkableCells = 2;
            loopConnectionChance = 0.04f;
        }

        if (ShouldUseTemplateGuidedLayout())
        {
            openingSignalCourtyardRadius = 5;
            openingPowerCourtyardRadius = 6;
            openingTransitCourtyardRadius = 6;
            mainStreetRadius = 1;
            deadEndCount = 12;
            districtCoveragePasses = 0;
            structuredSideStreetConnectorCount = 2;
            maxNearbyWalkableCells = 1;
            loopConnectionChance = 0.015f;
            sectorServiceSpokeCount = 1;
        }
    }
    [ContextMenu("Clear City Blockout")]
    public void ClearPreviousGeneration()
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

        savedWalkableCells.Clear();
        savedPlazaCells.Clear();
        savedMainStreetCells.Clear();
        savedSideStreetCells.Clear();
        savedAlleyCells.Clear();
        savedStarterAreaCells.Clear();
        savedResourceOpportunityCells.Clear();
        ClearGeneratedAnchorCells();
        ClearSavedRestorationDistrictCells();
        plazaCenters.Clear();
        savedOpeningSignalCourtyardCell = new Vector2Int(-1, -1);
        savedOpeningPowerCourtyardCell = new Vector2Int(-1, -1);
        savedOpeningTransitCourtyardCell = new Vector2Int(-1, -1);
        plannedRelaySectorCells.Clear();
        activeStarterAreaLayoutPlan = null;
        InvalidateLookupCaches();
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
        RebuildLookupCache(savedWalkableCells, walkableCellLookup);
        RebuildLookupCache(savedPlazaCells, plazaCellLookup);
        RebuildLookupCache(savedMainStreetCells, mainStreetCellLookup);
        RebuildLookupCache(savedSideStreetCells, sideStreetCellLookup);
        RebuildLookupCache(savedAlleyCells, alleyCellLookup);
        RebuildLookupCache(savedStarterAreaCells, starterAreaCellLookup);
        RebuildLookupCache(savedSignalDistrictCells, signalDistrictCellLookup);
        RebuildLookupCache(savedPowerDistrictCells, powerDistrictCellLookup);
        RebuildLookupCache(savedTransitDistrictCells, transitDistrictCellLookup);
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

    List<Vector2Int> FilterAnchorsByDistrict(List<Vector2Int> source, int districtIndex)
    {
        List<Vector2Int> filtered = new List<Vector2Int>();

        if (source == null)
        {
            return filtered;
        }

        for (int i = 0; i < source.Count; i++)
        {
            if (GetRestorationDistrictIndex(source[i]) == districtIndex)
            {
                filtered.Add(source[i]);
            }
        }

        return filtered;
    }

    void ClearGeneratedAnchorCells()
    {
        savedLandmarkAnchorCells.Clear();
        savedHazardAnchorCells.Clear();
        savedEnemyEncounterAnchorCells.Clear();
        savedVantageAnchorCells.Clear();
    }

    void ClearSavedRestorationDistrictCells()
    {
        savedSignalDistrictCells.Clear();
        savedPowerDistrictCells.Clear();
        savedTransitDistrictCells.Clear();
        signalDistrictCellLookup.Clear();
        powerDistrictCellLookup.Clear();
        transitDistrictCellLookup.Clear();
    }

    void BuildRestorationDistrictOwnership()
    {
        ClearSavedRestorationDistrictCells();

        if (savedStarterAreaCells.Count == 0)
        {
            return;
        }

        for (int i = 0; i < savedStarterAreaCells.Count; i++)
        {
            Vector2Int cell = savedStarterAreaCells[i];

            if (IsCellNearBase(cell, restorationDistrictInnerRadius))
            {
                continue;
            }

            int districtIndex = PickRestorationDistrictIndex(cell);

            if (districtIndex >= 0)
            {
                AddRestorationDistrictCell(cell, districtIndex);
            }
        }
    }

    int PickRestorationDistrictIndex(Vector2Int cell)
    {
        int districtCount = Mathf.Max(3, plannedRelaySectorCells.Count);
        int bestIndex = -1;
        float bestScore = float.MaxValue;

        for (int i = 0; i < districtCount && i < 3; i++)
        {
            Vector2Int anchor = GetRestorationDistrictAnchorCell(i);

            if (!IsSavedOpeningCourtyardCell(anchor))
            {
                continue;
            }

            float score = Mathf.Abs(cell.x - anchor.x) + Mathf.Abs(cell.y - anchor.y);

            if (activeStarterAreaLayoutPlan != null && i < activeStarterAreaLayoutPlan.sectors.Count)
            {
                StarterAreaSectorPlan sector = activeStarterAreaLayoutPlan.sectors[i];
                score = Mathf.Min(score, Mathf.Abs(cell.x - sector.relayCell.x) + Mathf.Abs(cell.y - sector.relayCell.y) + 3f);

                for (int n = 0; n < sector.neighborhoodCells.Count; n++)
                {
                    Vector2Int neighborhood = sector.neighborhoodCells[n];
                    score = Mathf.Min(score, Mathf.Abs(cell.x - neighborhood.x) + Mathf.Abs(cell.y - neighborhood.y) + 8f);
                }

                for (int p = 0; p < sector.resourceOpportunityCells.Count; p++)
                {
                    Vector2Int opportunity = sector.resourceOpportunityCells[p];
                    score = Mathf.Min(score, Mathf.Abs(cell.x - opportunity.x) + Mathf.Abs(cell.y - opportunity.y) + 10f);
                }
            }

            score += i * 0.35f;

            if (score < bestScore)
            {
                bestScore = score;
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    Vector2Int GetRestorationDistrictAnchorCell(int districtIndex)
    {
        if (districtIndex == 0 && IsSavedOpeningCourtyardCell(savedOpeningSignalCourtyardCell))
        {
            return savedOpeningSignalCourtyardCell;
        }

        if (districtIndex == 1 && IsSavedOpeningCourtyardCell(savedOpeningPowerCourtyardCell))
        {
            return savedOpeningPowerCourtyardCell;
        }

        if (districtIndex == 2 && IsSavedOpeningCourtyardCell(savedOpeningTransitCourtyardCell))
        {
            return savedOpeningTransitCourtyardCell;
        }

        if (districtIndex >= 0 && districtIndex < plannedRelaySectorCells.Count)
        {
            return plannedRelaySectorCells[districtIndex];
        }

        return new Vector2Int(-1, -1);
    }

    void AddRestorationDistrictCell(Vector2Int cell, int districtIndex)
    {
        List<Vector2Int> target = districtIndex == 0
            ? savedSignalDistrictCells
            : districtIndex == 1
                ? savedPowerDistrictCells
                : savedTransitDistrictCells;

        if (!target.Contains(cell))
        {
            target.Add(cell);
        }
    }

    void InvalidateLookupCaches()
    {
        lookupCachesValid = false;
        walkableCellLookup.Clear();
        plazaCellLookup.Clear();
        mainStreetCellLookup.Clear();
        sideStreetCellLookup.Clear();
        alleyCellLookup.Clear();
        starterAreaCellLookup.Clear();
        signalDistrictCellLookup.Clear();
        powerDistrictCellLookup.Clear();
        transitDistrictCellLookup.Clear();
    }

    void CarveDeadEnds()
    {
        for (int i = 0; i < deadEndCount; i++)
        {
            if (alleyCells.Count == 0) return;

            Vector2Int start = alleyCells[Random.Range(0, alleyCells.Count)];
            Vector2Int direction = RandomDirection();

            CarvePath(start, direction, deadEndLength, alleyRadius, alleyCells);
        }
    }

    void CarveAlleyClusters()
    {
        if (useIntentionalDistrictSkeleton)
        {
            return;
        }

        if (sideStreetCells.Count == 0 && alleyCells.Count == 0)
        {
            return;
        }

        for (int i = 0; i < alleyClusterCount; i++)
        {
            Vector2Int clusterCenter = PickAlleyClusterCenter();

            for (int j = 0; j < alleyClusterPaths; j++)
            {
                Vector2Int start = clusterCenter;

                if (Random.value < 0.6f && alleyCells.Count > 0)
                {
                    start = alleyCells[Random.Range(0, alleyCells.Count)];
                }

                Vector2Int direction = RandomDirection();

                CarvePath(
                    start,
                    direction,
                    Random.Range(2, alleyClusterPathLength + 1),
                    alleyRadius,
                    alleyCells
                );
            }
        }
    }

    Vector2Int PickAlleyClusterCenter()
    {
        List<Vector2Int> source = alleyCells.Count > 0 ? alleyCells : sideStreetCells;

        Vector2Int center = new Vector2Int(width / 2, height / 2);

        for (int i = 0; i < 20; i++)
        {
            Vector2Int candidate = source[Random.Range(0, source.Count)];

            if (Vector2Int.Distance(candidate, center) > baseExclusionRadius)
            {
                return candidate;
            }
        }

        return source[Random.Range(0, source.Count)];
    }

    void CarveUnderdevelopedDistricts()
    {
        for (int pass = 0; pass < districtCoveragePasses; pass++)
        {
            for (int startX = 1; startX < width - 1; startX += districtSize)
            {
                for (int startY = 1; startY < height - 1; startY += districtSize)
                {
                    RectInt district = new RectInt(
                        startX,
                        startY,
                        Mathf.Min(districtSize, width - 1 - startX),
                        Mathf.Min(districtSize, height - 1 - startY)
                    );

                    if (IsDistrictNearBase(district))
                    {
                        continue;
                    }

                    if (useIntentionalDistrictSkeleton && !HasEstablishedRouteNearDistrict(district, 2))
                    {
                        continue;
                    }

                    float walkableRatio = GetDistrictWalkableRatio(district);

                    float targetRatio = minimumDistrictWalkableRatio + Random.Range(-0.03f, 0.05f);

                    if (walkableRatio < targetRatio)
                    {
                        CarveSmallAlleyNetworkInDistrict(district);
                    }
                }
            }
        }
    }

    bool IsDistrictNearBase(RectInt district)
    {
        Vector2Int center = GetBaseCell();

        for (int x = district.xMin; x < district.xMax; x++)
        {
            for (int y = district.yMin; y < district.yMax; y++)
            {
                if (Vector2Int.Distance(new Vector2Int(x, y), center) <= baseExclusionRadius)
                {
                    return true;
                }
            }
        }

        return false;
    }

    float GetDistrictWalkableRatio(RectInt district)
    {
        int totalCells = 0;
        int walkableCount = 0;

        for (int x = district.xMin; x < district.xMax; x++)
        {
            for (int y = district.yMin; y < district.yMax; y++)
            {
                if (!IsInsideBounds(new Vector2Int(x, y)))
                {
                    continue;
                }

                totalCells++;

                if (walkableCells[x, y])
                {
                    walkableCount++;
                }
            }
        }

        if (totalCells == 0)
        {
            return 0f;
        }

        return (float)walkableCount / totalCells;
    }

    void CarveSmallAlleyNetworkInDistrict(RectInt district)
    {
        if (useIntentionalDistrictSkeleton)
        {
            return;
        }

        Vector2Int start = PickDistrictStartCell(district);

        for (int i = 0; i < districtAlleyAttempts; i++)
        {
            Vector2Int direction = RandomDirection();

            CarvePath(
                start,
                direction,
                Random.Range(3, alleyLength + 1),
                alleyRadius,
                alleyCells
            );

            start = PickDistrictStartCell(district);
        }
    }

    Vector2Int PickDistrictStartCell(RectInt district)
    {
        for (int i = 0; i < 20; i++)
        {
            int x = Random.Range(district.xMin, district.xMax);
            int y = Random.Range(district.yMin, district.yMax);

            Vector2Int candidate = new Vector2Int(x, y);

            if (IsInsideBounds(candidate))
            {
                return candidate;
            }
        }

        Vector2Int center = new Vector2Int(
    Mathf.RoundToInt(district.center.x),
    Mathf.RoundToInt(district.center.y)
);

        return new Vector2Int(
            Mathf.Clamp(center.x, 1, width - 2),
            Mathf.Clamp(center.y, 1, height - 2)
        );
    }

    void ConnectDisconnectedWalkableAreas()
    {
        List<List<Vector2Int>> regions = FindWalkableRegions();

        if (regions.Count <= 1)
        {
            return;
        }

        List<Vector2Int> mainRegion = regions[0];

        for (int i = 1; i < regions.Count; i++)
        {
            Vector2Int from = FindClosestCell(regions[i], mainRegion);
            Vector2Int to = FindClosestCell(mainRegion, regions[i]);

            CarveDirectConnector(from, to);

            mainRegion.AddRange(regions[i]);
        }
    }

    List<List<Vector2Int>> FindWalkableRegions()
    {
        bool[,] visited = new bool[width, height];
        List<List<Vector2Int>> regions = new List<List<Vector2Int>>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!walkableCells[x, y] || visited[x, y])
                {
                    continue;
                }

                List<Vector2Int> region = FloodFillRegion(new Vector2Int(x, y), visited);
                regions.Add(region);
            }
        }

        regions.Sort((a, b) => b.Count.CompareTo(a.Count));
        return regions;
    }

    List<Vector2Int> FloodFillRegion(Vector2Int start, bool[,] visited)
    {
        List<Vector2Int> region = new List<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        queue.Enqueue(start);
        visited[start.x, start.y] = true;

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            region.Add(current);

            Vector2Int[] neighbors =
            {
            current + Vector2Int.up,
            current + Vector2Int.down,
            current + Vector2Int.left,
            current + Vector2Int.right
        };

            for (int i = 0; i < neighbors.Length; i++)
            {
                Vector2Int neighbor = neighbors[i];

                if (!IsInsideBounds(neighbor))
                {
                    continue;
                }

                if (visited[neighbor.x, neighbor.y])
                {
                    continue;
                }

                if (!walkableCells[neighbor.x, neighbor.y])
                {
                    continue;
                }

                visited[neighbor.x, neighbor.y] = true;
                queue.Enqueue(neighbor);
            }
        }

        return region;
    }

    Vector2Int FindClosestCell(List<Vector2Int> fromRegion, List<Vector2Int> toRegion)
    {
        Vector2Int bestCell = fromRegion[0];
        float bestDistance = float.MaxValue;

        for (int i = 0; i < fromRegion.Count; i++)
        {
            for (int j = 0; j < toRegion.Count; j++)
            {
                float distance = Vector2Int.Distance(fromRegion[i], toRegion[j]);

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestCell = fromRegion[i];
                }
            }
        }

        return bestCell;
    }

    void CarveDirectConnector(Vector2Int from, Vector2Int to)
    {
        Vector2Int current = from;
        int safety = width + height;

        while (current != to && safety > 0)
        {
            safety--;

            CarveCorridor(current, alleyRadius, alleyCells, true);

            Vector2Int delta = to - current;

            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                current += delta.x > 0 ? Vector2Int.right : Vector2Int.left;
            }
            else
            {
                current += delta.y > 0 ? Vector2Int.up : Vector2Int.down;
            }

            if (!IsInsideBounds(current))
            {
                break;
            }
        }

        CarveCorridor(to, alleyRadius, alleyCells, true);
    }

    void CarveMainStreets(Vector2Int center)
    {
        Vector2Int[] directions =
        {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

        ShuffleDirections(directions);

        int streetsToCreate = Mathf.Min(mainStreetCount, directions.Length);

        for (int i = 0; i < streetsToCreate; i++)
        {
            Vector2Int direction = directions[i];
            CarveMainStreetLine(center, direction);
        }
    }

    Vector2Int PickCentralBaseCampSite(Vector2Int center)
    {
        Vector2Int offsetDirection = RandomDirection();
        Vector2Int lateralDirection = new Vector2Int(-offsetDirection.y, offsetDirection.x);
        int outwardOffset = Random.Range(0, 3);
        int lateralOffset = Random.Range(-2, 3);
        Vector2Int candidate = center + offsetDirection * outwardOffset + lateralDirection * lateralOffset;
        int margin = baseCampRadius + nearZoneLoopRadius + nearZoneLoopVariation + 3;

        return new Vector2Int(
            Mathf.Clamp(candidate.x, margin, width - margin - 1),
            Mathf.Clamp(candidate.y, margin, height - margin - 1)
        );
    }

    bool ShouldUseTemplateGuidedLayout()
    {
        return useIntentionalDistrictSkeleton && generationMode == CityGenerationMode.TemplateGuided;
    }

    StarterAreaLayoutPlan BuildStarterAreaLayoutPlan(Vector2Int mapCenter, StarterAreaLayoutTemplate template)
    {
        StarterAreaLayoutPlan plan = new StarterAreaLayoutPlan();
        bool mirror = randomizeTemplateMirror ? Random.value < 0.5f : mirrorTemplateHorizontally;
        int margin = Mathf.Max(maxPlazaRadius + openingRelayMainStreetClearance + 3, 7);

        plan.baseCell = ResolveTemplateOffset(template.baseOffset, mapCenter, mirror, margin, 0);
        plan.gateCell = ResolveTemplateOffset(template.gateOffset, mapCenter, mirror, margin, 0);

        for (int i = 0; i < template.sectors.Count; i++)
        {
            StarterAreaSectorTemplate sectorTemplate = template.sectors[i];
            StarterAreaSectorPlan sectorPlan = new StarterAreaSectorPlan
            {
                name = sectorTemplate.name,
                difficultyIndex = sectorTemplate.difficultyIndex,
                relayCell = ResolveTemplateOffset(sectorTemplate.relayOffset, mapCenter, mirror, margin, i + 1),
                maskRadius = sectorTemplate.maskRadius
            };

            for (int neighborhood = 0; neighborhood < sectorTemplate.neighborhoodOffsets.Count; neighborhood++)
            {
                sectorPlan.neighborhoodCells.Add(ResolveTemplateOffset(
                    sectorTemplate.neighborhoodOffsets[neighborhood],
                    mapCenter,
                    mirror,
                    margin,
                    i * 10 + neighborhood + 4));
            }

            for (int opportunity = 0; opportunity < sectorTemplate.resourceOpportunityOffsets.Count; opportunity++)
            {
                sectorPlan.resourceOpportunityCells.Add(ResolveTemplateOffset(
                    sectorTemplate.resourceOpportunityOffsets[opportunity],
                    mapCenter,
                    mirror,
                    margin,
                    i * 10 + opportunity + 7));
            }

            plan.sectors.Add(sectorPlan);
        }

        for (int i = 0; i < template.mainRoutes.Count; i++)
        {
            StarterAreaRouteTemplate routeTemplate = template.mainRoutes[i];
            StarterAreaRoutePlan routePlan = new StarterAreaRoutePlan
            {
                sectorIndex = routeTemplate.sectorIndex
            };

            for (int point = 0; point < routeTemplate.pointOffsets.Count; point++)
            {
                routePlan.points.Add(ResolveTemplateRoutePoint(
                    routeTemplate.pointOffsets[point],
                    routeTemplate.sectorIndex,
                    template,
                    plan,
                    mapCenter,
                    mirror,
                    margin,
                    i * 10 + point + 11));
            }

            plan.mainRoutes.Add(routePlan);
        }

        return plan;
    }

    Vector2Int ResolveTemplateRoutePoint(
        Vector2Int offset,
        int sectorIndex,
        StarterAreaLayoutTemplate template,
        StarterAreaLayoutPlan plan,
        Vector2Int mapCenter,
        bool mirror,
        int margin,
        int jitterSalt)
    {
        if (offset == template.baseOffset)
        {
            return plan.baseCell;
        }

        if (offset == template.gateOffset)
        {
            return plan.gateCell;
        }

        if (sectorIndex >= 0 && sectorIndex < plan.sectors.Count &&
            sectorIndex < template.sectors.Count &&
            offset == template.sectors[sectorIndex].relayOffset)
        {
            return plan.sectors[sectorIndex].relayCell;
        }

        return ResolveTemplateOffset(offset, mapCenter, mirror, margin, jitterSalt);
    }

    Vector2Int ResolveTemplateOffset(Vector2Int offset, Vector2Int mapCenter, bool mirror, int margin, int jitterSalt)
    {
        Vector2Int resolvedOffset = mirror ? new Vector2Int(-offset.x, offset.y) : offset;
        Vector2Int cell = mapCenter + resolvedOffset;

        if (jitterTemplateCells && templateJitterCells > 0 && jitterSalt > 0)
        {
            int jitterX = Random.Range(-templateJitterCells, templateJitterCells + 1);
            int jitterY = Random.Range(-templateJitterCells, templateJitterCells + 1);
            cell += new Vector2Int(jitterX, jitterY);
        }

        return ClampToMapMargin(cell, margin);
    }

    void CarveIntentionalRouteSkeleton(Vector2Int baseCell)
    {
        int minimumLoopRadius = baseCampRadius + 3;
        int radiusX = Mathf.Max(minimumLoopRadius, nearZoneLoopRadius + Random.Range(-nearZoneLoopVariation, nearZoneLoopVariation + 1));
        int radiusY = Mathf.Max(minimumLoopRadius, nearZoneLoopRadius + Random.Range(-nearZoneLoopVariation, nearZoneLoopVariation + 1));

        if (ShouldUseTemplateGuidedLayout())
        {
            if (activeStarterAreaLayoutPlan == null)
            {
                activeStarterAreaLayoutPlan = BuildStarterAreaLayoutPlan(new Vector2Int(width / 2, height / 2), StarterAreaLayoutTemplate.CreateDefault());
            }

            PlanRelaySectorCellsFromTemplate(activeStarterAreaLayoutPlan);
            BuildTemplateStarterAreaMask(activeStarterAreaLayoutPlan, baseCell);
            CarveNearZoneLoop(baseCell, radiusX, radiusY);
            CarveTemplateRouteNetwork(activeStarterAreaLayoutPlan, baseCell);
            CarveTemplateResourceOpportunityNetwork(activeStarterAreaLayoutPlan);
            CarveTemplateSectorLandmarkNetwork(activeStarterAreaLayoutPlan);
            return;
        }

        PlanRelaySectorCells(baseCell);
        BuildStarterAreaMask(baseCell);
        CarveNearZoneLoop(baseCell, radiusX, radiusY);
        CarveSectorRouteNetwork(baseCell, radiusX, radiusY);
        CarveSectorResourceOpportunityNetwork(baseCell);
    }

    void PlanRelaySectorCellsFromTemplate(StarterAreaLayoutPlan plan)
    {
        plannedRelaySectorCells.Clear();

        if (plan == null)
        {
            return;
        }

        for (int i = 0; i < plan.sectors.Count; i++)
        {
            plannedRelaySectorCells.Add(plan.sectors[i].relayCell);
        }
    }

    void BuildTemplateStarterAreaMask(StarterAreaLayoutPlan plan, Vector2Int baseCell)
    {
        if (starterAreaCells == null || starterAreaCells.GetLength(0) != width || starterAreaCells.GetLength(1) != height)
        {
            starterAreaCells = new bool[width, height];
        }

        savedStarterAreaCells.Clear();

        if (plan == null)
        {
            BuildStarterAreaMask(baseCell);
            return;
        }

        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                Vector2Int cell = new Vector2Int(x, y);
                bool inside = IsTemplateUrbanDistrictCell(cell, plan, baseCell);

                if (!inside)
                {
                    starterAreaCells[x, y] = false;
                    continue;
                }

                MarkStarterArea(cell);
            }
        }
    }

    bool IsTemplateUrbanDistrictCell(Vector2Int cell, StarterAreaLayoutPlan plan, Vector2Int baseCell)
    {
        if (IsInsideBlockyRect(cell, baseCell, centralSafeZoneRadius + 3, centralSafeZoneRadius, 0))
        {
            return true;
        }

        for (int i = 0; i < plan.sectors.Count; i++)
        {
            if (IsInsideTemplateSectorDistrict(cell, plan.sectors[i], i))
            {
                return true;
            }
        }

        for (int route = 0; route < plan.mainRoutes.Count; route++)
        {
            StarterAreaRoutePlan routePlan = plan.mainRoutes[route];

            for (int point = 0; point < routePlan.points.Count - 1; point++)
            {
                if (IsInsideBlockyRouteEnvelope(cell, routePlan.points[point], routePlan.points[point + 1], 4))
                {
                    return true;
                }
            }
        }

        return IsInsideBlockyRouteEnvelope(cell, GetTransitRelayCell(plan), plan.gateCell, 3);
    }

    bool IsInsideTemplateSectorDistrict(Vector2Int cell, StarterAreaSectorPlan sector, int sectorIndex)
    {
        int primaryHalfWidth = Mathf.Max(12, sector.maskRadius - (sectorIndex == 0 ? 3 : 1));
        int primaryHalfHeight = Mathf.Max(10, sector.maskRadius - (sectorIndex == 0 ? 8 : 6));

        if (sectorIndex == 2)
        {
            primaryHalfWidth += 3;
            primaryHalfHeight += 2;
        }

        if (IsInsideBlockyRect(cell, sector.relayCell, primaryHalfWidth, primaryHalfHeight, sectorIndex + 1))
        {
            return true;
        }

        for (int i = 0; i < sector.neighborhoodCells.Count; i++)
        {
            Vector2Int neighborhood = sector.neighborhoodCells[i];
            int halfWidth = sectorIndex == 0 ? 8 : sectorIndex == 1 ? 10 : 11;
            int halfHeight = sectorIndex == 0 ? 7 : sectorIndex == 1 ? 8 : 9;

            if (IsInsideBlockyRect(cell, neighborhood, halfWidth, halfHeight, sectorIndex * 11 + i + 5))
            {
                return true;
            }
        }

        for (int i = 0; i < sector.resourceOpportunityCells.Count; i++)
        {
            Vector2Int opportunity = sector.resourceOpportunityCells[i];
            int halfWidth = sectorIndex == 0 ? 5 : 6;
            int halfHeight = sectorIndex == 2 ? 6 : 5;

            if (IsInsideBlockyRect(cell, opportunity, halfWidth, halfHeight, sectorIndex * 13 + i + 19))
            {
                return true;
            }
        }

        return false;
    }

    bool IsInsideBlockyRect(Vector2Int cell, Vector2Int center, int halfWidth, int halfHeight, int salt)
    {
        int edgeOffset = GetBlockyEdgeOffset(cell, salt);
        int dx = Mathf.Abs(cell.x - center.x);
        int dy = Mathf.Abs(cell.y - center.y);

        if (dx > halfWidth + edgeOffset || dy > halfHeight + edgeOffset)
        {
            return false;
        }

        bool nearHorizontalEdge = dx >= halfWidth - 2;
        bool nearVerticalEdge = dy >= halfHeight - 2;

        if (nearHorizontalEdge || nearVerticalEdge)
        {
            float notchNoise = GetTemplateBlockNoise(cell, salt + 37);

            if (notchNoise < 0.16f)
            {
                return false;
            }
        }

        return true;
    }

    bool IsInsideBlockyRouteEnvelope(Vector2Int cell, Vector2Int a, Vector2Int b, int halfWidth)
    {
        if (DistanceToSegment(cell, a, b) > halfWidth)
        {
            return false;
        }

        return GetTemplateBlockNoise(cell, halfWidth + 71) > 0.08f;
    }

    int GetBlockyEdgeOffset(Vector2Int cell, int salt)
    {
        float noise = GetTemplateBlockNoise(cell, salt);

        if (noise < 0.22f)
        {
            return -3;
        }

        if (noise > 0.78f)
        {
            return 2;
        }

        return 0;
    }

    float GetTemplateBlockNoise(Vector2Int cell, int salt)
    {
        float blockX = Mathf.Floor((cell.x + salt * 17) / 4f);
        float blockY = Mathf.Floor((cell.y - salt * 23) / 4f);
        return Mathf.PerlinNoise((blockX + seed * 0.031f) * 0.31f, (blockY - seed * 0.027f) * 0.31f);
    }

    void BuildStarterAreaMask(Vector2Int baseCell)
    {
        if (starterAreaCells == null || starterAreaCells.GetLength(0) != width || starterAreaCells.GetLength(1) != height)
        {
            starterAreaCells = new bool[width, height];
        }

        savedStarterAreaCells.Clear();

        int[] sectorRadii =
        {
            signalSectorRadius,
            powerSectorRadius,
            transitSectorRadius
        };

        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                Vector2Int cell = new Vector2Int(x, y);
                float noise = Mathf.PerlinNoise((x + seed * 0.013f) * 0.18f, (y - seed * 0.017f) * 0.18f);
                float edgeBreakup = Mathf.Lerp(-2.2f, 2.4f, noise);
                bool inside = Vector2Int.Distance(cell, baseCell) <= centralSafeZoneRadius + edgeBreakup;

                for (int i = 0; i < plannedRelaySectorCells.Count; i++)
                {
                    float sectorDistance = Vector2Int.Distance(cell, plannedRelaySectorCells[i]);

                    if (sectorDistance <= sectorRadii[Mathf.Min(i, sectorRadii.Length - 1)] + edgeBreakup)
                    {
                        inside = true;
                    }

                    float corridorWidth = sectorCorridorWidth + Mathf.Lerp(-1.2f, 1.2f, noise);

                    if (DistanceToSegment(cell, baseCell, plannedRelaySectorCells[i]) <= corridorWidth)
                    {
                        inside = true;
                    }
                }

                for (int i = 0; i < plannedRelaySectorCells.Count - 1; i++)
                {
                    Vector2Int a = plannedRelaySectorCells[i];
                    Vector2Int b = plannedRelaySectorCells[i + 1];

                    if (DistanceToSegment(cell, a, b) <= sectorRingCorridorWidth + Mathf.Lerp(-1f, 1.4f, noise))
                    {
                        inside = true;
                    }
                }

                if (!inside)
                {
                    starterAreaCells[x, y] = false;
                    continue;
                }

                MarkStarterArea(cell);
            }
        }
    }

    float DistanceToSegment(Vector2Int cell, Vector2Int a, Vector2Int b)
    {
        Vector2 point = new Vector2(cell.x, cell.y);
        Vector2 start = new Vector2(a.x, a.y);
        Vector2 end = new Vector2(b.x, b.y);
        Vector2 segment = end - start;

        if (segment.sqrMagnitude < 0.001f)
        {
            return Vector2.Distance(point, start);
        }

        float t = Mathf.Clamp01(Vector2.Dot(point - start, segment) / segment.sqrMagnitude);
        return Vector2.Distance(point, start + segment * t);
    }

    void MarkStarterArea(Vector2Int cell)
    {
        if (!IsInsideBounds(cell))
        {
            return;
        }

        starterAreaCells[cell.x, cell.y] = true;

        if (!savedStarterAreaCells.Contains(cell))
        {
            savedStarterAreaCells.Add(cell);
        }
    }

    void PlanRelaySectorCells(Vector2Int baseCell)
    {
        plannedRelaySectorCells.Clear();

        float orientationJitter = Random.Range(-14f, 14f);
        float mirror = Random.value < 0.5f ? -1f : 1f;
        float[] sectorAngles =
        {
            160f,
            28f,
            -92f
        };

        int[] distances =
        {
            signalSectorDistance,
            powerSectorDistance,
            transitSectorDistance
        };

        for (int i = 0; i < 3; i++)
        {
            float angle = sectorAngles[i] * mirror + orientationJitter + Random.Range(-10f, 10f);
            Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;
            Vector2Int target = new Vector2Int(
                Mathf.RoundToInt(baseCell.x + direction.x * distances[i]),
                Mathf.RoundToInt(baseCell.y + direction.y * distances[i])
            );

            plannedRelaySectorCells.Add(ClampToMapMargin(target, maxPlazaRadius + openingRelayMainStreetClearance + 2));
        }
    }

    void CarveSectorRouteNetwork(Vector2Int baseCell, int radiusX, int radiusY)
    {
        for (int i = 0; i < plannedRelaySectorCells.Count; i++)
        {
            Vector2Int target = plannedRelaySectorCells[i];
            Vector2Int gate = GetNearZoneGateCell(baseCell, target, radiusX, radiusY);
            Vector2Int sectorDirection = GetDominantCardinalDirection(target - baseCell);
            Vector2Int avenueEnd = ClampToMapMargin(target - sectorDirection * sectorMainStreetSetback, 2);

            CarveRouteBetweenCells(baseCell, gate, mainStreetRadius, mainStreetCells, true, 0.02f);
            CarveRouteBetweenCells(gate, avenueEnd, mainStreetRadius, mainStreetCells, true, 0.1f);
            CarveRouteBetweenCells(avenueEnd, target, mainStreetRadius, mainStreetCells, true, 0.04f);
            CarveSectorServiceLoop(target, Mathf.Max(3, sectorLoopRadius + Random.Range(-1, 2)), baseCell);
            CarveSectorServiceSpokes(target, baseCell, i);
        }

        CarveSectorRingConnectors();
    }

    void CarveTemplateRouteNetwork(StarterAreaLayoutPlan plan, Vector2Int baseCell)
    {
        if (plan == null)
        {
            CarveSectorRouteNetwork(baseCell, nearZoneLoopRadius, nearZoneLoopRadius);
            return;
        }

        for (int route = 0; route < plan.mainRoutes.Count; route++)
        {
            StarterAreaRoutePlan routePlan = plan.mainRoutes[route];
            CarveTemplatePolylineRoute(routePlan.points, mainStreetRadius, mainStreetCells, true, 0.08f);
        }

        CarveTemplateMainRouteBranches(plan);
        CarveTemplateWorkshopBypassStreets(plan, baseCell);

        for (int i = 0; i < plan.sectors.Count; i++)
        {
            StarterAreaSectorPlan sector = plan.sectors[i];
            CarveSectorServiceLoop(sector.relayCell, Mathf.Max(4, sectorLoopRadius + Random.Range(-1, 2)), baseCell);
            CarveSectorServiceSpokes(sector.relayCell, baseCell, i);
        }
    }

    void CarveTemplatePolylineRoute(
        List<Vector2Int> points,
        int corridorRadius,
        List<Vector2Int> categoryList,
        bool ignoreSpacing,
        float wanderChance)
    {
        if (points == null || points.Count == 0)
        {
            return;
        }

        for (int i = 0; i < points.Count - 1; i++)
        {
            List<Vector2Int> streetPoints = BuildNaturalStreetSegment(points[i], points[i + 1]);

            for (int point = 0; point < streetPoints.Count - 1; point++)
            {
                CarveRouteBetweenCells(streetPoints[point], streetPoints[point + 1], corridorRadius, categoryList, ignoreSpacing, wanderChance);
            }
        }
    }

    List<Vector2Int> BuildNaturalStreetSegment(Vector2Int start, Vector2Int end)
    {
        List<Vector2Int> points = new List<Vector2Int>
        {
            start
        };

        Vector2Int delta = end - start;
        int distance = Mathf.Abs(delta.x) + Mathf.Abs(delta.y);

        if (distance >= 12)
        {
            Vector2Int dominant = GetDominantCardinalDirection(delta);
            Vector2Int lateral = new Vector2Int(-dominant.y, dominant.x);
            int bend = Random.Range(-3, 4);

            if (bend == 0)
            {
                bend = Random.value < 0.5f ? -2 : 2;
            }

            Vector2Int midpoint = new Vector2Int(
                Mathf.RoundToInt((start.x + end.x) * 0.5f),
                Mathf.RoundToInt((start.y + end.y) * 0.5f)
            );

            Vector2Int bendPoint = ClampToMapMargin(midpoint + lateral * bend, 3);

            if (IsCellInsideStarterArea(bendPoint.x, bendPoint.y))
            {
                points.Add(bendPoint);
            }
        }

        points.Add(end);
        return points;
    }

    void CarveTemplateMainRouteBranches(StarterAreaLayoutPlan plan)
    {
        if (plan == null)
        {
            return;
        }

        for (int route = 0; route < plan.mainRoutes.Count; route++)
        {
            StarterAreaRoutePlan routePlan = plan.mainRoutes[route];

            if (routePlan.points.Count < 3 || routePlan.sectorIndex < 0 || routePlan.sectorIndex >= plan.sectors.Count)
            {
                continue;
            }

            StarterAreaSectorPlan sector = plan.sectors[routePlan.sectorIndex];
            Vector2Int branchStart = routePlan.points[Mathf.Clamp(routePlan.points.Count - 2, 0, routePlan.points.Count - 1)];
            Vector2Int branchTarget = PickNearestTemplateAnchor(branchStart, sector.neighborhoodCells, sector.relayCell);

            if (branchTarget == branchStart || Vector2Int.Distance(branchStart, branchTarget) < 6f)
            {
                continue;
            }

            CarveRouteBetweenCells(branchStart, branchTarget, sideStreetRadius, sideStreetCells, false, 0.18f);
        }
    }

    void CarveTemplateWorkshopBypassStreets(StarterAreaLayoutPlan plan, Vector2Int baseCell)
    {
        if (plan == null || plan.sectors.Count < 3)
        {
            return;
        }

        Vector2Int signalApproach = PickRouteCellNearSector(plan.sectors[0].relayCell, mainStreetCells, plan.sectors[0].relayCell, 0);
        Vector2Int powerApproach = PickRouteCellNearSector(plan.sectors[1].relayCell, mainStreetCells, plan.sectors[1].relayCell, 1);
        Vector2Int transitApproach = PickRouteCellNearSector(plan.sectors[2].relayCell, mainStreetCells, plan.sectors[2].relayCell, 2);

        CarveWorkshopBypass(signalApproach, powerApproach, baseCell, 0);
        CarveWorkshopBypass(powerApproach, transitApproach, baseCell, 1);
    }

    void CarveWorkshopBypass(Vector2Int start, Vector2Int target, Vector2Int baseCell, int salt)
    {
        if (start == target)
        {
            return;
        }

        Vector2Int delta = target - start;
        Vector2Int dominant = GetDominantCardinalDirection(delta);
        Vector2Int lateral = new Vector2Int(-dominant.y, dominant.x);
        Vector2Int midpoint = new Vector2Int(
            Mathf.RoundToInt((start.x + target.x) * 0.5f),
            Mathf.RoundToInt((start.y + target.y) * 0.5f)
        );

        Vector2Int awayFromBase = GetDominantCardinalDirection(midpoint - baseCell);

        if (awayFromBase == Vector2Int.zero)
        {
            awayFromBase = lateral;
        }

        Vector2Int bypassPoint = ClampToMapMargin(midpoint + awayFromBase * (nearZoneLoopRadius + 2 + salt * 2), 3);

        if (!IsCellInsideStarterArea(bypassPoint.x, bypassPoint.y))
        {
            return;
        }

        CarveRouteBetweenCells(start, bypassPoint, sideStreetRadius, sideStreetCells, false, 0.22f);
        CarveRouteBetweenCells(bypassPoint, target, sideStreetRadius, sideStreetCells, false, 0.22f);
    }

    Vector2Int PickNearestTemplateAnchor(Vector2Int start, List<Vector2Int> anchors, Vector2Int fallback)
    {
        Vector2Int best = fallback;
        float bestDistance = Vector2Int.Distance(start, fallback);

        if (anchors == null)
        {
            return best;
        }

        for (int i = 0; i < anchors.Count; i++)
        {
            float distance = Vector2Int.Distance(start, anchors[i]);

            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = anchors[i];
            }
        }

        return best;
    }

    Vector2Int GetTransitRelayCell(StarterAreaLayoutPlan plan)
    {
        if (plan != null && plan.sectors.Count > 2)
        {
            return plan.sectors[2].relayCell;
        }

        if (plannedRelaySectorCells.Count > 2)
        {
            return plannedRelaySectorCells[2];
        }

        return GetBaseCell();
    }

    Vector2Int GetNearZoneGateCell(Vector2Int baseCell, Vector2Int target, int radiusX, int radiusY)
    {
        Vector2 direction = new Vector2(target.x - baseCell.x, target.y - baseCell.y).normalized;
        Vector2 scaled = new Vector2(direction.x * radiusX, direction.y * radiusY);
        return ClampToMapMargin(
            baseCell + new Vector2Int(Mathf.RoundToInt(scaled.x), Mathf.RoundToInt(scaled.y)),
            2
        );
    }

    Vector2Int ClampToMapMargin(Vector2Int cell, int margin)
    {
        return new Vector2Int(
            Mathf.Clamp(cell.x, margin, width - margin - 1),
            Mathf.Clamp(cell.y, margin, height - margin - 1)
        );
    }

    void CarveSectorServiceLoop(Vector2Int center, int radius, Vector2Int baseCell)
    {
        Vector2Int approach = GetDominantCardinalDirection(baseCell - center);
        Vector2Int lateral = new Vector2Int(-approach.y, approach.x);
        Vector2Int farSide = -approach;
        int nearSideOffset = Mathf.Max(3, radius / 2);

        CarveStructuredServicePath(center + lateral * radius, farSide, radius * 2, sideStreetRadius, sideStreetCells, false);
        CarveStructuredServicePath(center - lateral * radius, farSide, radius * 2, sideStreetRadius, sideStreetCells, false);
        CarveStructuredServicePath(center + lateral * radius + approach * nearSideOffset, -lateral, radius * 2, sideStreetRadius, sideStreetCells, false);

        if (Random.value < 0.5f)
        {
            CarveStructuredServicePath(center + farSide * radius, lateral, radius * 2, sideStreetRadius, sideStreetCells, false);
        }
    }

    void CarveSectorServiceSpokes(Vector2Int center, Vector2Int baseCell, int sectorIndex)
    {
        Vector2Int outward = GetDominantCardinalDirection(center - baseCell);
        Vector2Int lateral = new Vector2Int(-outward.y, outward.x);
        Vector2Int[] directions =
        {
            outward,
            lateral,
            -lateral,
            -outward
        };

        ShuffleDirections(directions);
        int spokes = Mathf.Min(directions.Length, Mathf.Max(2, sectorServiceSpokeCount + sectorIndex));

        for (int i = 0; i < spokes; i++)
        {
            CarveStructuredServicePath(
                center,
                directions[i],
                Random.Range(6, Mathf.Max(8, sectorLoopRadius + 5 + sectorIndex * 3)),
                sideStreetRadius,
                sideStreetCells,
                false
            );
        }
    }

    void CarveSectorRingConnectors()
    {
        if (plannedRelaySectorCells.Count < 2)
        {
            return;
        }

        int routeCount = Mathf.Min(plannedRelaySectorCells.Count - 1, Mathf.Max(1, sectorConnectorRouteCount));

        for (int i = 0; i < routeCount; i++)
        {
            Vector2Int start = plannedRelaySectorCells[i];
            Vector2Int target = plannedRelaySectorCells[(i + 1) % plannedRelaySectorCells.Count];
            CarveRouteBetweenCells(start, target, sideStreetRadius, sideStreetCells, false, 0.26f);
        }
    }

    void CarveSectorResourceOpportunityNetwork(Vector2Int baseCell)
    {
        for (int sectorIndex = 0; sectorIndex < plannedRelaySectorCells.Count; sectorIndex++)
        {
            Vector2Int sectorCenter = plannedRelaySectorCells[sectorIndex];
            int opportunityCount = GetResourceOpportunityCountForSector(sectorIndex);
            List<Vector2Int> sectorOpportunities = new List<Vector2Int>();

            for (int i = 0; i < opportunityCount; i++)
            {
                Vector2Int opportunity = PickResourceOpportunityCell(sectorCenter, baseCell, sectorIndex, sectorOpportunities);

                if (!IsInsideBounds(opportunity) || !IsCellInsideStarterArea(opportunity.x, opportunity.y))
                {
                    continue;
                }

                if (!IsResourceOpportunityPlacementValid(opportunity, sectorIndex))
                {
                    continue;
                }

                CarveResourceOpportunityPocket(opportunity, sectorIndex);
                Vector2Int routeSeed = FindNearestRouteCell(opportunity, true);
                CarveRouteBetweenCells(routeSeed, opportunity, sideStreetRadius, sideStreetCells, false, 0.18f);

                if (sectorOpportunities.Count > 0 && !ShouldUseTemplateGuidedLayout() && Random.value < 0.48f + sectorIndex * 0.08f)
                {
                    Vector2Int previous = sectorOpportunities[sectorOpportunities.Count - 1];
                    CarveRouteBetweenCells(previous, opportunity, sideStreetRadius, sideStreetCells, false, 0.36f);
                }

                CarveResourceOpportunityAlleys(opportunity, sectorIndex);
                sectorOpportunities.Add(opportunity);
            }
        }
    }

    int GetResourceOpportunityCountForSector(int sectorIndex)
    {
        if (sectorIndex == 0)
        {
            return Mathf.Max(1, signalSectorResourceOpportunities);
        }

        if (sectorIndex == 1)
        {
            return Mathf.Max(1, powerSectorResourceOpportunities);
        }

        return Mathf.Max(1, transitSectorResourceOpportunities);
    }

    void CarveTemplateResourceOpportunityNetwork(StarterAreaLayoutPlan plan)
    {
        if (plan == null)
        {
            CarveSectorResourceOpportunityNetwork(GetBaseCell());
            return;
        }

        for (int sectorIndex = 0; sectorIndex < plan.sectors.Count; sectorIndex++)
        {
            StarterAreaSectorPlan sector = plan.sectors[sectorIndex];
            Vector2Int previousOpportunity = sector.relayCell;

            for (int i = 0; i < sector.resourceOpportunityCells.Count; i++)
            {
                Vector2Int opportunity = ClampToMapMargin(sector.resourceOpportunityCells[i], maxPlazaRadius + 2);

                if (!IsInsideBounds(opportunity) || !IsCellInsideStarterArea(opportunity.x, opportunity.y))
                {
                    continue;
                }

                if (!IsResourceOpportunityPlacementValid(opportunity, sectorIndex))
                {
                    continue;
                }

                CarveResourceOpportunityPocket(opportunity, sectorIndex);
                CarveRouteBetweenCells(
                    FindNearestRouteCell(opportunity, true),
                    opportunity,
                    sideStreetRadius,
                    sideStreetCells,
                    false,
                    0.12f + sectorIndex * 0.03f);

                CarveResourceOpportunityApproaches(opportunity, sector.relayCell, sectorIndex);
                if (i > 0 && !ShouldUseTemplateGuidedLayout())
                {
                    CarveRouteBetweenCells(
                        previousOpportunity,
                        opportunity,
                        sideStreetRadius,
                        sideStreetCells,
                        false,
                        0.3f + sectorIndex * 0.04f);
                }

                CarveResourceOpportunityAlleys(opportunity, sectorIndex);
                previousOpportunity = opportunity;
            }
        }
    }

    void CarveTemplateSectorLandmarkNetwork(StarterAreaLayoutPlan plan)
    {
        if (plan == null)
        {
            return;
        }

        for (int sectorIndex = 0; sectorIndex < plan.sectors.Count; sectorIndex++)
        {
            StarterAreaSectorPlan sector = plan.sectors[sectorIndex];
            List<Vector2Int> candidates = BuildSectorLandmarkCandidates(sector);
            int targetCount = sectorIndex == 0 ? 2 : sectorIndex == 1 ? 3 : 4;
            List<Vector2Int> sectorLandmarks = new List<Vector2Int>();

            for (int i = 0; i < candidates.Count && sectorLandmarks.Count < targetCount; i++)
            {
                Vector2Int anchor = candidates[i];

                if (!IsCellInsideStarterArea(anchor.x, anchor.y) ||
                    !IsFarEnoughFromCells(anchor, sectorLandmarks, 8f) ||
                    !IsFarEnoughFromCells(anchor, savedLandmarkAnchorCells, 8f))
                {
                    continue;
                }

                CarveCompactLandmarkStreetFrame(anchor, sector.relayCell, sectorIndex, sectorLandmarks.Count);
                AddUniqueGeneratedAnchor(savedLandmarkAnchorCells, anchor);
                AddFutureSystemAnchorsNearLandmark(anchor, sector.relayCell, sectorIndex, sectorLandmarks.Count);
                sectorLandmarks.Add(anchor);
            }
        }
    }

    List<Vector2Int> BuildSectorLandmarkCandidates(StarterAreaSectorPlan sector)
    {
        List<Vector2Int> candidates = new List<Vector2Int>();

        for (int i = 0; i < sector.resourceOpportunityCells.Count; i++)
        {
            candidates.Add(sector.resourceOpportunityCells[i]);
        }

        for (int i = 0; i < sector.neighborhoodCells.Count; i++)
        {
            candidates.Add(sector.neighborhoodCells[i]);
        }

        for (int i = 0; i < candidates.Count; i++)
        {
            int swapIndex = Random.Range(i, candidates.Count);
            Vector2Int temp = candidates[i];
            candidates[i] = candidates[swapIndex];
            candidates[swapIndex] = temp;
        }

        return candidates;
    }

    void CarveCompactLandmarkStreetFrame(Vector2Int anchor, Vector2Int sectorRelayCell, int sectorIndex, int landmarkIndex)
    {
        Vector2Int routeSeed = FindNearestRouteCell(anchor, true);
        CarveRouteBetweenCells(routeSeed, anchor, sideStreetRadius, sideStreetCells, false, 0.12f);

        Vector2Int radial = GetDominantCardinalDirection(anchor - sectorRelayCell);

        if (radial == Vector2Int.zero)
        {
            radial = GetDominantCardinalDirection(anchor - GetBaseCell());
        }

        Vector2Int lateral = new Vector2Int(-radial.y, radial.x);
        int halfWidth = sectorIndex == 0 ? 2 : 3;
        int halfHeight = sectorIndex == 2 && landmarkIndex % 2 == 0 ? 3 : 2;

        CarveStraightServiceSegment(anchor - lateral * halfWidth, anchor + lateral * halfWidth, sideStreetRadius, sideStreetCells);
        CarveStraightServiceSegment(anchor - radial * halfHeight, anchor + radial * halfHeight, alleyRadius, alleyCells);
        CarveLandmarkIntersection(anchor, radial, lateral, sectorIndex, landmarkIndex);

        if (sectorIndex > 0)
        {
            Vector2Int serviceBend = anchor + radial * halfHeight + lateral * (landmarkIndex % 2 == 0 ? 2 : -2);
            CarveRouteBetweenCells(anchor + radial * halfHeight, serviceBend, alleyRadius, alleyCells, false, 0.45f);
        }
    }

    void CarveLandmarkIntersection(Vector2Int anchor, Vector2Int radial, Vector2Int lateral, int sectorIndex, int landmarkIndex)
    {
        int spurCount = sectorIndex == 0 ? 1 : sectorIndex == 1 ? 2 : 3;
        Vector2Int[] directions =
        {
            radial,
            -radial,
            lateral,
            -lateral
        };

        for (int i = 0; i < spurCount && i < directions.Length; i++)
        {
            Vector2Int direction = directions[(i + landmarkIndex) % directions.Length];
            int length = sectorIndex == 0 ? 2 : sectorIndex == 1 ? 3 : 4;
            List<Vector2Int> category = i == 0 ? sideStreetCells : alleyCells;
            CarveStraightServiceSegment(anchor, anchor + direction * length, category == sideStreetCells ? sideStreetRadius : alleyRadius, category);
        }
    }

    void AddFutureSystemAnchorsNearLandmark(Vector2Int landmark, Vector2Int sectorRelayCell, int sectorIndex, int landmarkIndex)
    {
        Vector2Int radial = GetDominantCardinalDirection(landmark - sectorRelayCell);

        if (radial == Vector2Int.zero)
        {
            radial = GetDominantCardinalDirection(landmark - GetBaseCell());
        }

        Vector2Int lateral = new Vector2Int(-radial.y, radial.x);
        Vector2Int hazardAnchor = ClampToMapMargin(landmark + lateral * (landmarkIndex % 2 == 0 ? 3 : -3), 3);
        Vector2Int encounterAnchor = ClampToMapMargin(landmark - radial * (sectorIndex + 3), 3);
        Vector2Int vantageAnchor = ClampToMapMargin(landmark + radial * (sectorIndex == 0 ? 4 : sectorIndex == 1 ? 5 : 6), 3);

        TryAddFutureAnchor(savedHazardAnchorCells, hazardAnchor, 7f);
        TryAddFutureAnchor(savedEnemyEncounterAnchorCells, encounterAnchor, 8f);

        if (landmarkIndex == 0 || sectorIndex == 2)
        {
            TryAddFutureAnchor(savedVantageAnchorCells, vantageAnchor, 12f);
        }
    }

    bool TryAddFutureAnchor(List<Vector2Int> anchors, Vector2Int cell, float minimumSpacing)
    {
        if (!IsInsideBounds(cell) || !IsCellInsideStarterArea(cell.x, cell.y))
        {
            return false;
        }

        if (!IsFarEnoughFromCells(cell, anchors, minimumSpacing))
        {
            return false;
        }

        CarveCorridor(cell, alleyRadius, alleyCells, true);
        AddUniqueGeneratedAnchor(anchors, cell);
        return true;
    }

    void AddUniqueGeneratedAnchor(List<Vector2Int> anchors, Vector2Int cell)
    {
        if (anchors != null && !anchors.Contains(cell))
        {
            anchors.Add(cell);
        }
    }

    int GetSectorRadius(int sectorIndex)
    {
        if (ShouldUseTemplateGuidedLayout() &&
            activeStarterAreaLayoutPlan != null &&
            sectorIndex >= 0 &&
            sectorIndex < activeStarterAreaLayoutPlan.sectors.Count)
        {
            return activeStarterAreaLayoutPlan.sectors[sectorIndex].maskRadius;
        }

        if (sectorIndex == 0)
        {
            return signalSectorRadius;
        }

        if (sectorIndex == 1)
        {
            return powerSectorRadius;
        }

        return transitSectorRadius;
    }

    Vector2Int PickResourceOpportunityCell(
        Vector2Int sectorCenter,
        Vector2Int baseCell,
        int sectorIndex,
        List<Vector2Int> existingSectorOpportunities)
    {
        Vector2 awayFromBase = new Vector2(sectorCenter.x - baseCell.x, sectorCenter.y - baseCell.y).normalized;

        if (awayFromBase.sqrMagnitude < 0.01f)
        {
            awayFromBase = Vector2.up;
        }

        float sectorRadius = GetSectorRadius(sectorIndex);
        float bestScore = float.MinValue;
        Vector2Int bestCell = sectorCenter;

        for (int attempt = 0; attempt < 60; attempt++)
        {
            float angle = Random.Range(-145f, 145f);
            Vector2 direction = RotateDirection(awayFromBase, angle);
            float distance = Random.Range(sectorLoopRadius + 5f, Mathf.Max(sectorLoopRadius + 6f, sectorRadius - 5f));
            Vector2Int candidate = ClampToMapMargin(
                new Vector2Int(
                    Mathf.RoundToInt(sectorCenter.x + direction.x * distance),
                    Mathf.RoundToInt(sectorCenter.y + direction.y * distance)
                ),
                4
            );

            if (!IsCellInsideStarterArea(candidate.x, candidate.y))
            {
                continue;
            }

            if (!IsFarEnoughFromCells(candidate, savedResourceOpportunityCells, 8f) ||
                !IsFarEnoughFromCells(candidate, existingSectorOpportunities, 8f))
            {
                continue;
            }

            if (Vector2Int.Distance(candidate, sectorCenter) < sectorLoopRadius + 3f)
            {
                continue;
            }

            float score = Vector2Int.Distance(candidate, sectorCenter);
            score += GetNearestServiceRouteDistance(candidate) * 0.45f;
            score -= Vector2Int.Distance(candidate, baseCell) * 0.04f;
            score += Random.Range(0f, 4f);

            if (score > bestScore)
            {
                bestScore = score;
                bestCell = candidate;
            }
        }

        return bestCell;
    }

    void CarveResourceOpportunityPocket(Vector2Int center, int sectorIndex)
    {
        Vector2Int routeSeed = FindNearestRouteCell(center, true);
        Vector2Int approach = GetDominantCardinalDirection(center - routeSeed);

        if (approach == Vector2Int.zero)
        {
            approach = GetDominantCardinalDirection(center - GetBaseCell());
        }

        Vector2Int lateral = new Vector2Int(-approach.y, approach.x);
        List<Vector2Int> pocketCells = new List<Vector2Int>
        {
            center
        };

        if (sectorIndex > 0 || Random.value < 0.35f)
        {
            pocketCells.Add(center + lateral);
        }

        if (sectorIndex > 1)
        {
            pocketCells.Add(center + approach);
        }

        for (int i = 0; i < pocketCells.Count; i++)
        {
            Vector2Int cell = pocketCells[i];

            if (!IsInsideBounds(cell) || !IsCellInsideStarterArea(cell.x, cell.y))
            {
                continue;
            }

            MarkWalkable(cell);

            List<Vector2Int> category = i == 0 ? sideStreetCells : alleyCells;

            if (!category.Contains(cell))
            {
                category.Add(cell);
            }

            SaveCellCategory(cell, category);
        }

        if (!savedResourceOpportunityCells.Contains(center))
        {
            savedResourceOpportunityCells.Add(center);
        }
    }

    void CarveResourceOpportunityApproaches(Vector2Int center, Vector2Int sectorRelayCell, int sectorIndex)
    {
        Vector2Int radial = GetDominantCardinalDirection(center - sectorRelayCell);

        if (radial == Vector2Int.zero)
        {
            radial = GetDominantCardinalDirection(center - GetBaseCell());
        }

        Vector2Int lateral = new Vector2Int(-radial.y, radial.x);
        int approachCount = sectorIndex == 0 ? 2 : 3;
        int approachLength = sectorIndex == 0 ? 3 : sectorIndex == 1 ? 4 : 5;
        Vector2Int[] approachDirections =
        {
            lateral,
            -lateral,
            -radial
        };

        for (int i = 0; i < approachCount; i++)
        {
            Vector2Int start = center + approachDirections[i] * approachLength;

            if (!IsCellInsideStarterArea(start.x, start.y))
            {
                continue;
            }

            List<Vector2Int> category = i == 0 ? sideStreetCells : alleyCells;
            CarveBentLocalConnector(start, center, category == sideStreetCells ? sideStreetRadius : alleyRadius, category, i + sectorIndex);
        }
    }

    void CarveResourceOpportunityAlleys(Vector2Int center, int sectorIndex)
    {
        int branchCount = sectorIndex == 0 ? 0 : sectorIndex == 1 ? 1 : 2;
        int maxLength = sectorIndex == 0 ? 3 : sectorIndex == 1 ? 4 : 5;

        for (int i = 0; i < branchCount; i++)
        {
            Vector2Int end = CarveMazeLikePath(
                center,
                RandomDirection(),
                Random.Range(3, maxLength + 1),
                alleyRadius,
                alleyCells,
                0.62f,
                true
            );

            if (sectorIndex > 1 && Random.value < 0.18f)
            {
                CarveMazeLikePath(
                    end,
                    PickNewDirection(RandomDirection()),
                    Random.Range(2, Mathf.Max(3, maxLength / 2)),
                    alleyRadius,
                    alleyCells,
                    0.72f,
                    false
                );
            }
        }
    }

    bool IsResourceOpportunityPlacementValid(Vector2Int center, int sectorIndex)
    {
        if (!IsInsideBounds(center) || !IsCellInsideStarterArea(center.x, center.y))
        {
            return false;
        }

        float minimumSpacing = sectorIndex == 0 ? 8f : sectorIndex == 1 ? 9f : 10f;

        if (!IsFarEnoughFromCells(center, savedResourceOpportunityCells, minimumSpacing))
        {
            return false;
        }

        if (IsCellPlaza(center.x, center.y) || CountWalkableCellsInEnvelope(center, 2) > 12)
        {
            return false;
        }

        return true;
    }

    Vector2Int FindNearestRouteCell(Vector2Int cell, bool includeMainStreet)
    {
        List<Vector2Int> candidates = new List<Vector2Int>();

        if (includeMainStreet)
        {
            AddCells(mainStreetCells, candidates);
        }

        AddCells(sideStreetCells, candidates);

        if (candidates.Count == 0)
        {
            return cell;
        }

        return FindNearestCell(cell, candidates);
    }

    void AddCells(List<Vector2Int> source, List<Vector2Int> target)
    {
        if (source == null || target == null)
        {
            return;
        }

        for (int i = 0; i < source.Count; i++)
        {
            target.Add(source[i]);
        }
    }

    void CarveRouteBetweenCells(
        Vector2Int start,
        Vector2Int target,
        int corridorRadius,
        List<Vector2Int> categoryList,
        bool ignoreSpacing,
        float wanderChance)
    {
        Vector2Int current = start;
        int safety = width + height;
        int stepsSinceWander = 0;

        while (current != target && safety > 0)
        {
            safety--;
            stepsSinceWander++;
            CarveCorridor(current, corridorRadius, categoryList, ignoreSpacing);

            Vector2Int delta = target - current;
            Vector2Int nextStep;

            if (Random.value < wanderChance && stepsSinceWander > 3)
            {
                Vector2Int dominant = GetDominantCardinalDirection(delta);
                nextStep = Random.value < 0.5f
                    ? new Vector2Int(-dominant.y, dominant.x)
                    : new Vector2Int(dominant.y, -dominant.x);
                stepsSinceWander = 0;
            }
            else if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                nextStep = delta.x > 0 ? Vector2Int.right : Vector2Int.left;
            }
            else
            {
                nextStep = delta.y > 0 ? Vector2Int.up : Vector2Int.down;
            }

            Vector2Int next = current + nextStep;

            if (!IsInsideBounds(next) || Vector2Int.Distance(next, target) > Vector2Int.Distance(current, target) + 1.1f)
            {
                if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                {
                    next = current + (delta.x > 0 ? Vector2Int.right : Vector2Int.left);
                }
                else
                {
                    next = current + (delta.y > 0 ? Vector2Int.up : Vector2Int.down);
                }
            }

            if (!IsInsideBounds(next))
            {
                break;
            }

            current = next;
        }

        CarveCorridor(target, corridorRadius, categoryList, ignoreSpacing);
    }

    void CarveLegacyArterialRoutes(Vector2Int baseCell, int radiusX, int radiusY)
    {

        Vector2Int[] directions =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        ShuffleDirections(directions);
        int branchCount = Mathf.Clamp(arterialBranchCount, 2, directions.Length);

        for (int i = 0; i < branchCount; i++)
        {
            CarveArterialGate(baseCell, directions[i], radiusX, radiusY);
        }
    }

    void CarveNearZoneLoop(Vector2Int center, int radiusX, int radiusY)
    {
        for (int x = -radiusX; x <= radiusX; x++)
        {
            CarveCorridor(center + new Vector2Int(x, -radiusY), sideStreetRadius, sideStreetCells, true);
            CarveCorridor(center + new Vector2Int(x, radiusY), sideStreetRadius, sideStreetCells, true);
        }

        for (int y = -radiusY + 1; y < radiusY; y++)
        {
            CarveCorridor(center + new Vector2Int(-radiusX, y), sideStreetRadius, sideStreetCells, true);
            CarveCorridor(center + new Vector2Int(radiusX, y), sideStreetRadius, sideStreetCells, true);
        }
    }

    void CarveArterialGate(Vector2Int center, Vector2Int direction, int radiusX, int radiusY)
    {
        Vector2Int lateral = new Vector2Int(-direction.y, direction.x);
        int edgeDistance = direction.x != 0 ? radiusX : radiusY;
        Vector2Int gate = center + direction * edgeDistance + lateral * Random.Range(-2, 3);

        for (int i = 1; i <= arterialGateInset; i++)
        {
            CarveCorridor(gate + direction * i, sideStreetRadius, sideStreetCells, true);
        }

        CarveArterialBranch(gate + direction * (arterialGateInset + 1), direction, lateral);
    }

    void CarveArterialBranch(Vector2Int start, Vector2Int direction, Vector2Int lateral)
    {
        Vector2Int current = start;
        int bendAt = Mathf.Max(4, arterialBendDistance + Random.Range(-3, 4));
        int bendDirection = Random.value < 0.5f ? -1 : 1;

        for (int i = 0; i < mainStreetLength; i++)
        {
            if (!IsInsideBounds(current))
            {
                break;
            }

            CarveCorridor(current, mainStreetRadius, mainStreetCells, true);

            if (i == bendAt)
            {
                current += lateral * bendDirection;
                CarveCorridor(current, mainStreetRadius, mainStreetCells, true);
            }

            current += direction;
        }
    }

    void CarveSideStreets()
    {
        if (useIntentionalDistrictSkeleton)
        {
            CarveSectorSideStreetWeb();
            return;
        }

        int expansions = sideStreetCount * sideStreetLength;

        for (int i = 0; i < expansions; i++)
        {
            if (mainStreetCells.Count == 0 && sideStreetCells.Count == 0)
            {
                return;
            }

            Vector2Int start;

            bool shouldStartFromMainStreet =
                mainStreetCells.Count > 0 &&
                (sideStreetCells.Count == 0 || Random.value < sideStreetMainStreetStartChance);

            if (shouldStartFromMainStreet)
            {
                start = mainStreetCells[Random.Range(0, mainStreetCells.Count)];
            }
            else
            {
                start = sideStreetCells[Random.Range(0, sideStreetCells.Count)];
            }

            Vector2Int direction = PickSideStreetDirection(start);

            int length = useIntentionalDistrictSkeleton
                ? Random.Range(Mathf.Max(4, sideStreetLength / 3), Mathf.Max(5, sideStreetLength + 1))
                : Random.Range(4, 9);

            CarvePath(start, direction, length, sideStreetRadius, sideStreetCells);

            // Occasionally spawn a local cluster at the end
            float distanceFromBase = GetDistanceFromBase(start);

            float normalizedDistance = distanceFromBase / (width * 0.5f);

            // farther from base = more likely to create clusters
            float clusterChance = Mathf.Lerp(0.15f, 0.55f, normalizedDistance);

            if (Random.value < clusterChance)
            {
                Vector2Int clusterCenter = start + direction * length;
                SpawnSideStreetCluster(clusterCenter);
            }
        }
    }

    Vector2Int PickSideStreetDirection(Vector2Int start)
    {
        Vector2Int nearestMainStreet = FindNearestCell(start, mainStreetCells);
        Vector2Int awayFromMain = start - nearestMainStreet;

        if (awayFromMain == Vector2Int.zero)
        {
            return RandomDirection();
        }

        Vector2Int preferredDirection;

        if (Mathf.Abs(awayFromMain.x) > Mathf.Abs(awayFromMain.y))
        {
            preferredDirection = awayFromMain.x > 0 ? Vector2Int.right : Vector2Int.left;
        }
        else
        {
            preferredDirection = awayFromMain.y > 0 ? Vector2Int.up : Vector2Int.down;
        }

        if (Random.value < 0.7f)
        {
            return preferredDirection;
        }

        return RandomDirection();
    }
    void SpawnSideStreetCluster(Vector2Int center)
    {
        if (!IsInsideBounds(center))
        {
            return;
        }

        int clusterPaths = Random.Range(2, 4);

        for (int i = 0; i < clusterPaths; i++)
        {
            Vector2Int dir = RandomDirection();

            CarvePath(
                center,
                dir,
                Random.Range(3, 7),
                sideStreetRadius,
                sideStreetCells
            );
        }
    }
    void CarveAlleys()
    {
        if (useIntentionalDistrictSkeleton)
        {
            CarveSectorAlleyMazes();
            return;
        }

        int expansions = alleyCount * alleyLength;

        for (int i = 0; i < expansions; i++)
        {
            if (sideStreetCells.Count == 0 && alleyCells.Count == 0)
                return;

            Vector2Int start;

            if (alleyCells.Count == 0 || Random.value < 0.7f)
            {
                start = sideStreetCells[Random.Range(0, sideStreetCells.Count)];
            }
            else
            {
                start = alleyCells[Random.Range(0, alleyCells.Count)];
            }

            Vector2Int direction = RandomDirection();

            int length = useIntentionalDistrictSkeleton
                ? Random.Range(2, Mathf.Max(3, alleyLength + 1))
                : Random.Range(2, 6);

            CarvePath(start, direction, length, alleyRadius, alleyCells);
        }
    }

    void CarveSectorAlleyMazes()
    {
        for (int i = 0; i < plannedRelaySectorCells.Count; i++)
        {
            Vector2Int sectorCenter = plannedRelaySectorCells[i];
            int shortcutCount = i == 0 ? 4 : i == 1 ? 7 : 10;
            int deadEndPocketCount = i == 0 ? 1 : i == 1 ? 2 : 3;
            int maxLength = i == 0 ? 4 : i == 1 ? 6 : 8;

            if (ShouldUseTemplateGuidedLayout())
            {
                shortcutCount = i == 0 ? 2 : i == 1 ? 3 : 5;
                deadEndPocketCount = i == 0 ? 0 : 1;
                maxLength = i == 0 ? 3 : i == 1 ? 5 : 6;
            }

            for (int shortcut = 0; shortcut < shortcutCount; shortcut++)
            {
                if (!TryCarveAlleyShortcut(sectorCenter, i, maxLength + 3))
                {
                    break;
                }
            }

            for (int pocket = 0; pocket < deadEndPocketCount; pocket++)
            {
                Vector2Int center = PickRouteCellNearSector(sectorCenter, sideStreetCells, sectorCenter, i);

                for (int path = 0; path < 2; path++)
                {
                    Vector2Int end = CarveMazeLikePath(
                        center,
                        RandomDirection(),
                        Random.Range(3, maxLength + 1),
                        alleyRadius,
                        alleyCells,
                        0.5f + i * 0.08f,
                        false
                    );

                    if (i > 0 && Random.value < 0.34f)
                    {
                        CarveMazeLikePath(
                            end,
                            RandomDirection(),
                            Random.Range(2, Mathf.Max(3, maxLength / 2)),
                            alleyRadius,
                            alleyCells,
                            0.62f,
                            false
                        );
                    }
                }
            }
        }
    }

    bool TryCarveAlleyShortcut(Vector2Int sectorCenter, int sectorIndex, int maxLength)
    {
        List<Vector2Int> candidates = new List<Vector2Int>();
        float maxSectorDistance = GetSectorRadius(sectorIndex) + 4f;

        for (int i = 0; i < sideStreetCells.Count; i++)
        {
            Vector2Int cell = sideStreetCells[i];

            if (Vector2Int.Distance(cell, sectorCenter) <= maxSectorDistance)
            {
                candidates.Add(cell);
            }
        }

        if (candidates.Count < 2)
        {
            return false;
        }

        for (int attempt = 0; attempt < 24; attempt++)
        {
            Vector2Int start = candidates[Random.Range(0, candidates.Count)];
            Vector2Int target = candidates[Random.Range(0, candidates.Count)];
            float distance = Vector2Int.Distance(start, target);

            if (distance < 4f || distance > maxLength)
            {
                continue;
            }

            if (HasMostlyDirectWalkableConnection(start, target))
            {
                continue;
            }

            CarveAlleyShortcutPath(start, target, sectorIndex);
            return true;
        }

        return false;
    }

    void CarveAlleyShortcutPath(Vector2Int start, Vector2Int target, int sectorIndex)
    {
        Vector2Int delta = target - start;
        Vector2Int dominant = GetDominantCardinalDirection(delta);
        Vector2Int lateral = new Vector2Int(-dominant.y, dominant.x);
        Vector2Int bend = new Vector2Int(
            Mathf.RoundToInt((start.x + target.x) * 0.5f),
            Mathf.RoundToInt((start.y + target.y) * 0.5f)
        ) + lateral * (Random.value < 0.5f ? 2 : -2);

        if (!IsCellInsideStarterArea(bend.x, bend.y))
        {
            bend = start + dominant * Mathf.Max(2, Mathf.RoundToInt(Vector2Int.Distance(start, target) * 0.45f));
        }

        CarveMazeLikePath(start, GetDominantCardinalDirection(bend - start), Mathf.Max(2, Mathf.RoundToInt(Vector2Int.Distance(start, bend))), alleyRadius, alleyCells, 0.72f, true);
        CarveMazeLikePath(bend, GetDominantCardinalDirection(target - bend), Mathf.Max(2, Mathf.RoundToInt(Vector2Int.Distance(bend, target))), alleyRadius, alleyCells, 0.78f, true);
    }

    bool HasMostlyDirectWalkableConnection(Vector2Int start, Vector2Int target)
    {
        Vector2Int current = start;
        int walkableHits = 0;
        int steps = 0;
        int safety = width + height;

        while (current != target && safety > 0)
        {
            safety--;
            steps++;

            if (walkableCells[current.x, current.y])
            {
                walkableHits++;
            }

            Vector2Int delta = target - current;
            current += Mathf.Abs(delta.x) > Mathf.Abs(delta.y)
                ? (delta.x > 0 ? Vector2Int.right : Vector2Int.left)
                : (delta.y > 0 ? Vector2Int.up : Vector2Int.down);

            if (!IsInsideBounds(current))
            {
                break;
            }
        }

        return steps > 0 && walkableHits > steps * 0.72f;
    }

    void CarveSectorSideStreetWeb()
    {
        for (int i = 0; i < plannedRelaySectorCells.Count; i++)
        {
            Vector2Int sectorCenter = plannedRelaySectorCells[i];
            List<Vector2Int> neighborhoods = PickSectorNeighborhoodCenters(sectorCenter, i);

            for (int neighborhood = 0; neighborhood < neighborhoods.Count; neighborhood++)
            {
                CarveNeighborhoodCluster(neighborhoods[neighborhood], sectorCenter, i);

                if (!ShouldUseTemplateGuidedLayout() &&
                    neighborhood > 0 &&
                    Random.value < 0.35f + i * 0.1f)
                {
                    CarveRouteBetweenCells(
                        neighborhoods[neighborhood - 1],
                        neighborhoods[neighborhood],
                        sideStreetRadius,
                        sideStreetCells,
                        false,
                        0.34f + i * 0.04f
                    );
                }
            }

            CarvePlazaNeighborhoodConnectors(sectorCenter, i);
        }

        if (ShouldUseTemplateGuidedLayout())
        {
            CarveTemplateInterSectorContinuityLinks();
        }

        for (int i = 0; i < Mathf.Max(1, structuredSideStreetConnectorCount / 4); i++)
        {
            Vector2Int sectorCenter = plannedRelaySectorCells[Random.Range(0, plannedRelaySectorCells.Count)];
            Vector2Int start = PickRouteCellNearSector(sectorCenter, sideStreetCells, sectorCenter, 1);
            Vector2Int target = FindNearbyWalkableCell(start, 7, 15);

            if (target != start)
            {
                CarveServiceConnectorPath(start, target, sideStreetRadius, sideStreetCells);
            }
        }
    }

    void CarveTemplateInterSectorContinuityLinks()
    {
        if (activeStarterAreaLayoutPlan == null || activeStarterAreaLayoutPlan.sectors.Count < 3)
        {
            return;
        }

        CarveTemplateContinuityLink(0, 1, true);
        CarveTemplateContinuityLink(0, 2, false);
        CarveTemplateContinuityLink(1, 2, true);
    }

    void CarveTemplateContinuityLink(int sectorA, int sectorB, bool preferSideStreet)
    {
        if (sectorA >= activeStarterAreaLayoutPlan.sectors.Count || sectorB >= activeStarterAreaLayoutPlan.sectors.Count)
        {
            return;
        }

        StarterAreaSectorPlan a = activeStarterAreaLayoutPlan.sectors[sectorA];
        StarterAreaSectorPlan b = activeStarterAreaLayoutPlan.sectors[sectorB];
        Vector2Int start = PickSectorContinuityCell(a, b.relayCell, sectorA);
        Vector2Int target = PickSectorContinuityCell(b, a.relayCell, sectorB);

        if (start == target || Vector2Int.Distance(start, target) < 8f)
        {
            return;
        }

        List<Vector2Int> category = preferSideStreet && Random.value < 0.45f ? sideStreetCells : alleyCells;
        int radius = category == sideStreetCells ? sideStreetRadius : alleyRadius;

        CarveRouteBetweenCells(start, target, radius, category, false, 0.34f);
    }

    Vector2Int PickSectorContinuityCell(StarterAreaSectorPlan sector, Vector2Int toward, int sectorIndex)
    {
        List<Vector2Int> candidates = new List<Vector2Int>();
        float maxDistance = GetSectorRadius(sectorIndex) + 8f;

        AddContinuityCandidates(sideStreetCells, sector.relayCell, toward, maxDistance, candidates);
        AddContinuityCandidates(alleyCells, sector.relayCell, toward, maxDistance, candidates);

        if (candidates.Count == 0)
        {
            return PickNearestTemplateAnchor(toward, sector.neighborhoodCells, sector.relayCell);
        }

        Vector2Int best = candidates[0];
        float bestScore = float.MaxValue;

        for (int i = 0; i < candidates.Count; i++)
        {
            Vector2Int candidate = candidates[i];
            float score = Vector2Int.Distance(candidate, toward);
            score += Mathf.Max(0f, 9f - Vector2Int.Distance(candidate, sector.relayCell)) * 8f;
            score += mainStreetCells.Contains(candidate) ? 20f : 0f;
            score += Random.Range(0f, 2f);

            if (score < bestScore)
            {
                bestScore = score;
                best = candidate;
            }
        }

        return best;
    }

    void AddContinuityCandidates(List<Vector2Int> source, Vector2Int sectorCenter, Vector2Int toward, float maxDistance, List<Vector2Int> candidates)
    {
        if (source == null)
        {
            return;
        }

        Vector2Int directionToNeighbor = GetDominantCardinalDirection(toward - sectorCenter);

        for (int i = 0; i < source.Count; i++)
        {
            Vector2Int cell = source[i];

            if (Vector2Int.Distance(cell, sectorCenter) > maxDistance ||
                Vector2Int.Distance(cell, sectorCenter) < 8f ||
                !IsCellInsideStarterArea(cell.x, cell.y))
            {
                continue;
            }

            Vector2Int candidateDirection = GetDominantCardinalDirection(cell - sectorCenter);

            if (candidateDirection == directionToNeighbor && !candidates.Contains(cell))
            {
                candidates.Add(cell);
            }
        }
    }

    List<Vector2Int> PickSectorNeighborhoodCenters(Vector2Int sectorCenter, int sectorIndex)
    {
        if (ShouldUseTemplateGuidedLayout() &&
            activeStarterAreaLayoutPlan != null &&
            sectorIndex >= 0 &&
            sectorIndex < activeStarterAreaLayoutPlan.sectors.Count)
        {
            return new List<Vector2Int>(activeStarterAreaLayoutPlan.sectors[sectorIndex].neighborhoodCells);
        }

        List<Vector2Int> centers = new List<Vector2Int>();
        List<Vector2Int> opportunities = GetResourceOpportunitiesNearSector(sectorCenter, sectorIndex);
        int targetCount = GetNeighborhoodCountForSector(sectorIndex);

        for (int i = 0; i < opportunities.Count && centers.Count < targetCount; i++)
        {
            if (IsFarEnoughFromCells(opportunities[i], centers, sectorLoopRadius * 0.8f))
            {
                centers.Add(opportunities[i]);
            }
        }

        int attempts = targetCount * 40;

        while (centers.Count < targetCount && attempts > 0)
        {
            attempts--;
            Vector2Int candidate = PickNeighborhoodCandidate(sectorCenter, sectorIndex);

            if (!IsCellInsideStarterArea(candidate.x, candidate.y))
            {
                continue;
            }

            if (Vector2Int.Distance(candidate, sectorCenter) < sectorLoopRadius + 4f)
            {
                continue;
            }

            if (!IsFarEnoughFromCells(candidate, centers, sectorLoopRadius * 0.75f))
            {
                continue;
            }

            centers.Add(candidate);
        }

        return centers;
    }

    int GetNeighborhoodCountForSector(int sectorIndex)
    {
        if (sectorIndex == 0)
        {
            return Mathf.Max(1, signalSectorNeighborhoods);
        }

        if (sectorIndex == 1)
        {
            return Mathf.Max(1, powerSectorNeighborhoods);
        }

        return Mathf.Max(1, transitSectorNeighborhoods);
    }

    List<Vector2Int> GetResourceOpportunitiesNearSector(Vector2Int sectorCenter, int sectorIndex)
    {
        List<Vector2Int> opportunities = new List<Vector2Int>();
        float maxDistance = GetSectorRadius(sectorIndex) + 4f;

        for (int i = 0; i < savedResourceOpportunityCells.Count; i++)
        {
            Vector2Int opportunity = savedResourceOpportunityCells[i];

            if (Vector2Int.Distance(opportunity, sectorCenter) <= maxDistance)
            {
                opportunities.Add(opportunity);
            }
        }

        return opportunities;
    }

    Vector2Int PickNeighborhoodCandidate(Vector2Int sectorCenter, int sectorIndex)
    {
        float radius = GetSectorRadius(sectorIndex);
        float angle = Random.Range(0f, 360f);
        float distance = Random.Range(sectorLoopRadius + 5f, Mathf.Max(sectorLoopRadius + 6f, radius - 4f));
        Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

        return ClampToMapMargin(
            new Vector2Int(
                Mathf.RoundToInt(sectorCenter.x + direction.x * distance),
                Mathf.RoundToInt(sectorCenter.y + direction.y * distance)
            ),
            maxPlazaRadius + 2
        );
    }

    void CarveNeighborhoodCluster(Vector2Int center, Vector2Int sectorCenter, int sectorIndex)
    {
        if (ShouldUseTemplateGuidedLayout())
        {
            CarveTemplateNeighborhoodBlock(center, sectorCenter, sectorIndex);
            return;
        }

        int loopCount = sectorIndex == 0 ? 1 : sectorIndex == 1 ? (Random.value < 0.35f ? 2 : 1) : (Random.value < 0.65f ? 2 : 1);

        if (ShouldUseTemplateGuidedLayout())
        {
            loopCount = 1;
        }

        Vector2Int routeSeed = FindNearestRouteCell(center, true);
        CarveRouteBetweenCells(routeSeed, center, sideStreetRadius, sideStreetCells, true, 0.22f + sectorIndex * 0.04f);

        for (int i = 0; i < loopCount; i++)
        {
            int halfWidth = Random.Range(3, sectorIndex == 0 ? 5 : sectorIndex == 1 ? 6 : 7);
            int halfHeight = Random.Range(3, sectorIndex == 0 ? 5 : sectorIndex == 1 ? 6 : 7);
            Vector2Int offset = new Vector2Int(
                Random.Range(-2, 3),
                Random.Range(-2, 3)
            );

            CarveNeighborhoodBlockLoop(center + offset, halfWidth, halfHeight, sectorIndex);
        }

        CarveNeighborhoodCrossStreets(center, sectorIndex);
    }

    void CarveTemplateNeighborhoodBlock(Vector2Int center, Vector2Int sectorCenter, int sectorIndex)
    {
        Vector2Int routeSeed = FindNearestRouteCell(center, true);
        CarveRouteBetweenCells(routeSeed, center, sideStreetRadius, sideStreetCells, true, 0.08f);

        Vector2Int radial = GetDominantCardinalDirection(center - sectorCenter);

        if (radial == Vector2Int.zero)
        {
            radial = GetDominantCardinalDirection(center - GetBaseCell());
        }

        Vector2Int lateral = new Vector2Int(-radial.y, radial.x);
        int halfWidth = sectorIndex == 0 ? 3 : sectorIndex == 1 ? 5 : 6;
        int halfHeight = sectorIndex == 0 ? 3 : sectorIndex == 1 ? 4 : 5;

        CarveOrganicNeighborhoodLoop(center, radial, lateral, halfWidth, halfHeight, sectorIndex, 0);
        CarveStraightServiceSegment(center - lateral * halfWidth, center + lateral * halfWidth, sideStreetRadius, sideStreetCells);
        CarveLocalReturnLoop(center, radial, lateral, halfWidth, halfHeight, sectorIndex);

        if (sectorIndex > 0)
        {
            Vector2Int secondaryCenter = center + radial * (halfHeight + 4) + lateral * (Random.value < 0.5f ? 1 : -1);
            CarveOrganicNeighborhoodLoop(secondaryCenter, radial, lateral, Mathf.Max(3, halfWidth - 1), Mathf.Max(3, halfHeight - 1), sectorIndex, 1);
            CarveBentLocalConnector(center + radial * halfHeight, secondaryCenter - radial * (halfHeight - 1), sideStreetRadius, sideStreetCells, sectorIndex);
        }

        if (sectorIndex == 2)
        {
            Vector2Int alleyStart = center - radial * halfHeight;
            Vector2Int alleyEnd = alleyStart - radial * 5;
            CarveBentLocalConnector(alleyStart, alleyEnd, alleyRadius, alleyCells, 2);
            CarveBentLocalConnector(alleyEnd - lateral * 2, alleyEnd + lateral * 2, alleyRadius, alleyCells, 3);
        }
    }

    void CarveOrganicNeighborhoodLoop(Vector2Int center, Vector2Int radial, Vector2Int lateral, int halfWidth, int halfHeight, int sectorIndex, int variant)
    {
        Vector2Int offset = sectorIndex == 0
            ? Vector2Int.zero
            : lateral * (variant % 2 == 0 ? 1 : -1);
        Vector2Int loopCenter = center + offset;
        bool brokenLoop = sectorIndex == 0 && Random.value < 0.25f;

        CarveNeighborhoodBlockLoop(loopCenter, halfWidth, halfHeight, sectorIndex, brokenLoop);

        Vector2Int notchA = loopCenter + lateral * halfWidth + radial * (variant % 2 == 0 ? 1 : -1);
        Vector2Int notchB = loopCenter - lateral * (Mathf.Max(2, halfWidth - 1)) - radial * halfHeight;
        CarveStraightServiceSegment(notchA, notchA + radial * (sectorIndex == 0 ? 1 : 2), alleyRadius, alleyCells);

        if (sectorIndex > 0)
        {
            CarveStraightServiceSegment(notchB, notchB + lateral * 2, alleyRadius, alleyCells);
        }

        if (sectorIndex == 2)
        {
            Vector2Int diagonalStart = loopCenter - lateral * halfWidth + radial * halfHeight;
            Vector2Int diagonalEnd = loopCenter + lateral * Mathf.Max(2, halfWidth - 2) - radial * Mathf.Max(2, halfHeight - 2);
            CarveBentLocalConnector(diagonalStart, diagonalEnd, alleyRadius, alleyCells, variant + 4);
        }
    }

    void CarveLocalReturnLoop(Vector2Int center, Vector2Int radial, Vector2Int lateral, int halfWidth, int halfHeight, int sectorIndex)
    {
        if (sectorIndex == 0 && Random.value < 0.35f)
        {
            return;
        }

        Vector2Int start = center - lateral * halfWidth;
        Vector2Int returnPoint = center + radial * (halfHeight + (sectorIndex == 2 ? 3 : 2));
        Vector2Int end = center + lateral * halfWidth;

        CarveBentLocalConnector(start, returnPoint, alleyRadius, alleyCells, sectorIndex + 5);
        CarveBentLocalConnector(returnPoint, end, alleyRadius, alleyCells, sectorIndex + 7);
    }

    void CarveNeighborhoodBlockLoop(Vector2Int center, int halfWidth, int halfHeight, int sectorIndex)
    {
        CarveNeighborhoodBlockLoop(center, halfWidth, halfHeight, sectorIndex, sectorIndex == 0 && Random.value < 0.35f);
    }

    void CarveNeighborhoodBlockLoop(Vector2Int center, int halfWidth, int halfHeight, int sectorIndex, bool brokenLoop)
    {
        bool skipCornerA = sectorIndex > 0 && Random.value < 0.55f;
        bool skipCornerB = sectorIndex == 2 && Random.value < 0.65f;

        for (int x = -halfWidth; x <= halfWidth; x++)
        {
            bool atLeftCorner = x <= -halfWidth + 1;
            bool atRightCorner = x >= halfWidth - 1;

            if ((!brokenLoop || x < halfWidth - 1) && !(skipCornerA && atRightCorner))
            {
                CarveCorridor(center + new Vector2Int(x, -halfHeight), sideStreetRadius, sideStreetCells, true);
            }

            if ((!brokenLoop || x > -halfWidth + 1) && !(skipCornerB && atLeftCorner))
            {
                CarveCorridor(center + new Vector2Int(x, halfHeight), sideStreetRadius, sideStreetCells, true);
            }
        }

        for (int y = -halfHeight + 1; y < halfHeight; y++)
        {
            bool atBottomCorner = y <= -halfHeight + 2;
            bool atTopCorner = y >= halfHeight - 2;

            if ((!brokenLoop || y > -halfHeight + 2) && !(skipCornerB && atBottomCorner))
            {
                CarveCorridor(center + new Vector2Int(-halfWidth, y), sideStreetRadius, sideStreetCells, true);
            }

            if ((!brokenLoop || y < halfHeight - 2) && !(skipCornerA && atTopCorner))
            {
                CarveCorridor(center + new Vector2Int(halfWidth, y), sideStreetRadius, sideStreetCells, true);
            }
        }
    }

    void CarveBentLocalConnector(Vector2Int start, Vector2Int end, int corridorRadius, List<Vector2Int> categoryList, int salt)
    {
        Vector2Int delta = end - start;
        Vector2Int dominant = GetDominantCardinalDirection(delta);
        Vector2Int lateral = new Vector2Int(-dominant.y, dominant.x);
        int bendDistance = Mathf.Clamp(Mathf.Abs(delta.x) + Mathf.Abs(delta.y), 2, 7);
        Vector2Int midpoint = new Vector2Int(
            Mathf.RoundToInt((start.x + end.x) * 0.5f),
            Mathf.RoundToInt((start.y + end.y) * 0.5f)
        );
        Vector2Int bend = midpoint + lateral * ((salt % 2 == 0 ? 1 : -1) * Mathf.Max(1, bendDistance / 4));

        if (!IsCellInsideStarterArea(bend.x, bend.y))
        {
            CarveStraightServiceSegment(start, end, corridorRadius, categoryList);
            return;
        }

        CarveStraightServiceSegment(start, bend, corridorRadius, categoryList);
        CarveStraightServiceSegment(bend, end, corridorRadius, categoryList);
    }

    void CarveStraightServiceSegment(Vector2Int start, Vector2Int end, int corridorRadius, List<Vector2Int> categoryList)
    {
        Vector2Int current = start;
        int safety = width + height;

        while (current != end && safety > 0)
        {
            safety--;
            CarveCorridor(current, corridorRadius, categoryList, true);

            Vector2Int delta = end - current;

            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                current += delta.x > 0 ? Vector2Int.right : Vector2Int.left;
            }
            else if (delta.y != 0)
            {
                current += delta.y > 0 ? Vector2Int.up : Vector2Int.down;
            }

            if (!IsInsideBounds(current))
            {
                break;
            }
        }

        CarveCorridor(end, corridorRadius, categoryList, true);
    }

    void CarveNeighborhoodCrossStreets(Vector2Int center, int sectorIndex)
    {
        int crossStreetCount = sectorIndex == 0 ? 1 : sectorIndex == 1 ? 1 : 2;

        if (ShouldUseTemplateGuidedLayout())
        {
            crossStreetCount = 1;
        }

        Vector2Int[] directions =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        ShuffleDirections(directions);

        for (int i = 0; i < crossStreetCount && i < directions.Length; i++)
        {
            int length = Random.Range(4, sectorIndex == 0 ? 8 : sectorIndex == 1 ? 10 : 13);
            Vector2Int end = CarveStructuredServicePath(center, directions[i], length, sideStreetRadius, sideStreetCells, true);

            if (Random.value < 0.5f + sectorIndex * 0.12f)
            {
                Vector2Int target = FindNearbyWalkableCell(end, 6, 14 + sectorIndex * 3);

                if (target != end && !mainStreetCells.Contains(target))
                {
                    CarveServiceConnectorPath(end, target, sideStreetRadius, sideStreetCells);
                }
            }
        }
    }

    void CarvePlazaNeighborhoodConnectors(Vector2Int sectorCenter, int sectorIndex)
    {
        int connectorCount = sectorIndex == 0 ? 2 : sectorIndex == 1 ? 2 : 3;

        if (ShouldUseTemplateGuidedLayout())
        {
            connectorCount = sectorIndex == 2 ? 2 : 1;
        }

        for (int i = 0; i < connectorCount; i++)
        {
            Vector2Int start = PickPlazaEdgeSeed(sectorCenter, sectorIndex);
            Vector2Int target = PickRouteCellNearSector(sectorCenter, sideStreetCells, sectorCenter, sectorIndex);

            if (target != sectorCenter)
            {
                CarveRouteBetweenCells(start, target, sideStreetRadius, sideStreetCells, true, 0.26f);
            }
        }
    }

    Vector2Int PickPlazaEdgeSeed(Vector2Int sectorCenter, int sectorIndex)
    {
        int radius = sectorIndex == 0
            ? openingSignalCourtyardRadius
            : sectorIndex == 1
                ? openingPowerCourtyardRadius
                : openingTransitCourtyardRadius;

        Vector2Int[] directions =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        Vector2Int direction = directions[Random.Range(0, directions.Length)];
        return ClampToMapMargin(sectorCenter + direction * (radius + 1), radius + 2);
    }

    Vector2Int PickRouteCellNearSector(Vector2Int sectorCenter, List<Vector2Int> source, Vector2Int fallback, int sectorIndex)
    {
        List<Vector2Int> candidates = new List<Vector2Int>();
        float maxDistance = sectorIndex == 0 ? signalSectorRadius + 6f : sectorIndex == 1 ? powerSectorRadius + 6f : transitSectorRadius + 6f;

        for (int i = 0; i < source.Count; i++)
        {
            if (Vector2Int.Distance(source[i], sectorCenter) <= maxDistance)
            {
                candidates.Add(source[i]);
            }
        }

        if (candidates.Count == 0)
        {
            return fallback;
        }

        return candidates[Random.Range(0, candidates.Count)];
    }

    Vector2Int PickSectorBranchDirection(Vector2Int seed, Vector2Int sectorCenter)
    {
        Vector2Int radial = GetDominantCardinalDirection(seed - sectorCenter);

        if (radial == Vector2Int.zero)
        {
            radial = RandomDirection();
        }

        Vector2Int lateral = new Vector2Int(-radial.y, radial.x);
        float roll = Random.value;

        if (roll < 0.34f)
        {
            return lateral;
        }

        if (roll < 0.68f)
        {
            return -lateral;
        }

        return Random.value < 0.5f ? radial : -radial;
    }

    void CarveStructuredAlleyPockets()
    {
        if (sideStreetCells.Count == 0)
        {
            return;
        }

        List<Vector2Int> centers = PickSpacedRouteSeeds(
            sideStreetCells,
            Mathf.Max(6, districtSize / 2),
            Mathf.Max(baseExclusionRadius + 1f, nearZoneLoopRadius + 2f)
        );

        int pocketCount = Mathf.Min(centers.Count, Mathf.Max(alleyClusterCount, alleyCount / 4));

        for (int i = 0; i < pocketCount; i++)
        {
            Vector2Int center = centers[i];
            Vector2Int[] directions =
            {
                Vector2Int.up,
                Vector2Int.down,
                Vector2Int.left,
                Vector2Int.right
            };

            ShuffleDirections(directions);
            int paths = Random.Range(2, Mathf.Max(3, alleyClusterPaths + 1));

            for (int pathIndex = 0; pathIndex < paths && pathIndex < directions.Length; pathIndex++)
            {
                int length = Random.Range(3, Mathf.Max(4, alleyClusterPathLength + 2));
                Vector2Int end = CarveStructuredServicePath(center, directions[pathIndex], length, alleyRadius, alleyCells, true);

                if (Random.value < 0.45f)
                {
                    CarveStructuredServicePath(
                        end,
                        PickNewDirection(directions[pathIndex]),
                        Random.Range(2, Mathf.Max(3, alleyLength / 2)),
                        alleyRadius,
                        alleyCells,
                        true
                    );
                }
            }
        }
    }

    void CarveStructuredSideStreetNetwork()
    {
        if (mainStreetCells.Count == 0)
        {
            return;
        }

        List<Vector2Int> seeds = PickSpacedRouteSeeds(
            mainStreetCells,
            Mathf.Max(3, structuredSideStreetInterval),
            Mathf.Max(baseExclusionRadius + 1f, nearZoneLoopRadius + 2f)
        );

        int branchBudget = Mathf.Min(seeds.Count, Mathf.Max(8, sideStreetCount));

        for (int i = 0; i < branchBudget; i++)
        {
            Vector2Int seed = seeds[i];
            Vector2Int radial = GetDominantCardinalDirection(seed - GetBaseCell());
            Vector2Int lateral = new Vector2Int(-radial.y, radial.x);

            if (lateral == Vector2Int.zero)
            {
                lateral = RandomDirection();
            }

            if (Random.value < 0.5f)
            {
                lateral = -lateral;
            }

            int length = Random.Range(
                Mathf.Max(5, sideStreetLength / 3),
                Mathf.Max(7, Mathf.RoundToInt(sideStreetLength * 0.8f))
            );

            Vector2Int end = CarveStructuredServicePath(seed, lateral, length, sideStreetRadius, sideStreetCells, true);

            if (Random.value < 0.55f)
            {
                Vector2Int bendDirection = Random.value < 0.5f ? radial : -radial;
                CarveStructuredServicePath(
                    end,
                    bendDirection,
                    Random.Range(4, Mathf.Max(6, sideStreetLength / 2)),
                    sideStreetRadius,
                    sideStreetCells,
                    true
                );
            }
        }

        int connectorBudget = Mathf.Max(3, structuredSideStreetConnectorCount);

        for (int i = 0; i < connectorBudget; i++)
        {
            if (sideStreetCells.Count == 0)
            {
                return;
            }

            Vector2Int start = sideStreetCells[Random.Range(0, sideStreetCells.Count)];
            Vector2Int target = FindNearbyWalkableCell(start, 7, 15);

            if (target == start || mainStreetCells.Contains(target))
            {
                continue;
            }

            CarveServiceConnectorPath(start, target, sideStreetRadius, sideStreetCells);
        }
    }

    List<Vector2Int> PickSpacedRouteSeeds(List<Vector2Int> source, int spacing, float minimumDistanceFromBase)
    {
        List<Vector2Int> candidates = new List<Vector2Int>(source);
        List<Vector2Int> seeds = new List<Vector2Int>();

        for (int i = 0; i < candidates.Count; i++)
        {
            int swapIndex = Random.Range(i, candidates.Count);
            Vector2Int temp = candidates[i];
            candidates[i] = candidates[swapIndex];
            candidates[swapIndex] = temp;
        }

        for (int i = 0; i < candidates.Count; i++)
        {
            Vector2Int candidate = candidates[i];

            if (GetDistanceFromBase(candidate) < minimumDistanceFromBase)
            {
                continue;
            }

            if (IsFarEnoughFromCells(candidate, seeds, spacing))
            {
                seeds.Add(candidate);
            }
        }

        return seeds;
    }

    Vector2Int CarveStructuredServicePath(
        Vector2Int start,
        Vector2Int direction,
        int length,
        int corridorRadius,
        List<Vector2Int> categoryList,
        bool ignoreSpacing = false)
    {
        Vector2Int current = start;
        Vector2Int currentDirection = direction;

        for (int i = 0; i < length; i++)
        {
            if (!IsInsideBounds(current))
            {
                break;
            }

            CarveCorridor(current, corridorRadius, categoryList, ignoreSpacing);

            if (i > 2 && Random.value < 0.14f)
            {
                currentDirection = PickNewDirection(currentDirection);
            }

            Vector2Int next = current + currentDirection;

            if (!IsInsideBounds(next))
            {
                break;
            }

            current = next;
        }

        return current;
    }

    Vector2Int CarveMazeLikePath(
        Vector2Int start,
        Vector2Int direction,
        int length,
        int corridorRadius,
        List<Vector2Int> categoryList,
        float turnProbability,
        bool ignoreSpacing)
    {
        Vector2Int current = start;
        Vector2Int currentDirection = direction;
        int straightBudget = Random.Range(2, 5);
        bool leavingMainStreet = mainStreetCells.Contains(start);

        for (int i = 0; i < length; i++)
        {
            if (!IsInsideBounds(current))
            {
                break;
            }

            bool ignoreInitialSpacing = leavingMainStreet && i <= mainStreetRadius + 1;
            CarveCorridor(current, corridorRadius, categoryList, ignoreSpacing || ignoreInitialSpacing);
            straightBudget--;

            if (straightBudget <= 0 || Random.value < turnProbability)
            {
                currentDirection = PickNewDirection(currentDirection);
                straightBudget = Random.Range(2, 5);
            }

            Vector2Int next = current + currentDirection;

            if (!IsInsideBounds(next))
            {
                break;
            }

            current = next;
        }

        return current;
    }

    void CarveServiceConnectorPath(
        Vector2Int start,
        Vector2Int target,
        int corridorRadius,
        List<Vector2Int> categoryList)
    {
        Vector2Int current = start;
        int safety = width + height;

        while (current != target && safety > 0)
        {
            safety--;
            CarveCorridor(current, corridorRadius, categoryList, true);

            Vector2Int delta = target - current;

            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                current += delta.x > 0 ? Vector2Int.right : Vector2Int.left;
            }
            else
            {
                current += delta.y > 0 ? Vector2Int.up : Vector2Int.down;
            }

            if (!IsInsideBounds(current))
            {
                break;
            }
        }
    }

    void CarvePath(
    Vector2Int start,
    Vector2Int direction,
    int length,
    int corridorRadius,
    List<Vector2Int> categoryList
)
    {
        Vector2Int current = start;
        Vector2Int currentDirection = direction;

        bool isMainStreetPath = categoryList == mainStreetCells;
        List<Vector2Int> cellsCarvedByThisPath = new List<Vector2Int>();

        int segmentLength = Random.Range(minSegmentLength, maxSegmentLength + 1);
        int segmentProgress = 0;

        for (int i = 0; i < length; i++)
        {
            bool ignoreSpacing = isMainStreetPath;

            if (isMainStreetPath && WouldOverlapCurrentPath(current, cellsCarvedByThisPath, minimumSamePathSpacing))
            {
                break;
            }

            CarveCorridor(current, corridorRadius, categoryList, ignoreSpacing);

            if (isMainStreetPath)
            {
                cellsCarvedByThisPath.Add(current);
            }

            segmentProgress++;

            bool shouldTurn =
                segmentProgress >= segmentLength ||
                Random.value < turnChance;

            if (shouldTurn)
            {
                currentDirection = PickNewDirection(currentDirection);
                segmentLength = Random.Range(minSegmentLength, maxSegmentLength + 1);
                segmentProgress = 0;
            }

            if (Random.value < 0.12f)
            {
                currentDirection = RandomDirection();
            }

            Vector2Int next = current + currentDirection;

            if (!IsInsideBounds(next))
            {
                break;
            }

            current = next;
        }
    }
    bool WouldOverlapCurrentPath(
    Vector2Int current,
    List<Vector2Int> cellsCarvedByThisPath,
    int minimumSpacing
)
    {
        for (int i = 0; i < cellsCarvedByThisPath.Count; i++)
        {
            Vector2Int previousCell = cellsCarvedByThisPath[i];

            int recentCellBuffer = 4;

            if (i >= cellsCarvedByThisPath.Count - recentCellBuffer)
            {
                continue;
            }

            if (Vector2Int.Distance(current, previousCell) < minimumSpacing)
            {
                return true;
            }
        }

        return false;
    }
    void CarveCorridor(Vector2Int center, int radius, List<Vector2Int> categoryList, bool ignoreSpacing)
    {
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                Vector2Int cell = new Vector2Int(center.x + x, center.y + y);

                if (!IsInsideBounds(cell))
                {
                    continue;
                }

                bool alreadyWalkable = walkableCells[cell.x, cell.y];

                if (useIntentionalDistrictSkeleton && !IsCellInsideStarterArea(cell.x, cell.y))
                {
                    continue;
                }

                if (!ignoreSpacing && !alreadyWalkable && WouldOvercrowdPaths(cell))
                {
                    continue;
                }

                MarkWalkable(cell);

                if (categoryList != null && !categoryList.Contains(cell))
                {
                    categoryList.Add(cell);
                }

                SaveCellCategory(cell, categoryList);
            }
        }
    }

    bool WouldOvercrowdPaths(Vector2Int cell)
    {
        int nearbyWalkableCount = 0;

        if (mainStreetCells.Contains(cell))
        {
            return false;
        }

        for (int x = -minimumPathSpacing; x <= minimumPathSpacing; x++)
        {
            for (int y = -minimumPathSpacing; y <= minimumPathSpacing; y++)
            {
                if (x == 0 && y == 0)
                {
                    continue;
                }

                Vector2Int checkCell = new Vector2Int(cell.x + x, cell.y + y);

                if (!IsInsideBounds(checkCell))
                {
                    continue;
                }

                if (walkableCells[checkCell.x, checkCell.y])
                {
                    nearbyWalkableCount++;

                    if (nearbyWalkableCount > maxNearbyWalkableCells)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    Vector2Int PickPathStartCell(
    List<Vector2Int> primaryCells,
    List<Vector2Int> secondaryCells,
    float primaryChance
)
    {
        bool usePrimary =
            primaryCells.Count > 0 &&
            (secondaryCells.Count == 0 || Random.value < primaryChance);

        List<Vector2Int> source = usePrimary ? primaryCells : secondaryCells;

        if (source == null || source.Count == 0)
        {
            if (carvedCells.Count > 0)
            {
                return carvedCells[Random.Range(0, carvedCells.Count)];
            }

            return new Vector2Int(width / 2, height / 2);
        }

        return source[Random.Range(0, source.Count)];
    }

    Vector2Int PickNewDirection(Vector2Int currentDirection)
    {
        if (Random.value < continueStraightChance)
        {
            return currentDirection;
        }

        if (currentDirection == Vector2Int.up || currentDirection == Vector2Int.down)
        {
            return Random.value < 0.5f ? Vector2Int.left : Vector2Int.right;
        }

        return Random.value < 0.5f ? Vector2Int.up : Vector2Int.down;
    }

    void CarveRandomPlazas()
    {
        List<Vector2Int> candidates = GetPlazaCandidates();

        int plazasCreated = 0;
        int attempts = Mathf.Max(plazaCount * 30, 30);

        for (int i = 0; i < attempts && plazasCreated < plazaCount; i++)
        {
            if (candidates.Count == 0)
            {
                return;
            }

            Vector2Int plazaCenter = candidates[Random.Range(0, candidates.Count)];

            if (IsTooCloseToBase(plazaCenter))
            {
                candidates.Remove(plazaCenter);
                continue;
            }

            if (IsTooCloseToExistingPlaza(plazaCenter))
            {
                candidates.Remove(plazaCenter);
                continue;
            }

            int radius = Random.Range(minPlazaRadius, maxPlazaRadius + 1);

            if (WouldPlazaTouchBaseExclusion(plazaCenter, radius))
            {
                candidates.Remove(plazaCenter);
                continue;
            }

            CarvePlaza(plazaCenter, radius, false);
            plazaCenters.Add(plazaCenter);

            candidates.Remove(plazaCenter);
            plazasCreated++;
        }
    }

    List<Vector2Int> GetPlazaCandidates()
    {
        List<Vector2Int> strongCandidates = new List<Vector2Int>();
        List<Vector2Int> fallbackCandidates = new List<Vector2Int>();

        for (int i = 0; i < savedWalkableCells.Count; i++)
        {
            Vector2Int cell = savedWalkableCells[i];

            if (IsTooCloseToBase(cell))
            {
                continue;
            }

            if (IsCellMainStreet(cell.x, cell.y))
            {
                continue;
            }

            int connectionCount = CountCardinalWalkableNeighbors(cell);
            bool touchesMainStreet = TouchesMainStreet(cell);
            bool hasSideOrAlleyNearby = HasSideStreetOrAlleyNearby(cell);

            if (connectionCount >= 3 && hasSideOrAlleyNearby && !touchesMainStreet)
            {
                strongCandidates.Add(cell);
            }
            else if (connectionCount >= 2 && hasSideOrAlleyNearby)
            {
                fallbackCandidates.Add(cell);
            }
        }

        for (int i = 0; i < fallbackCandidates.Count; i++)
        {
            if (!strongCandidates.Contains(fallbackCandidates[i]))
            {
                strongCandidates.Add(fallbackCandidates[i]);
            }
        }

        if (strongCandidates.Count > 0)
        {
            return strongCandidates;
        }

        return new List<Vector2Int>(savedWalkableCells);
    }

    void CarveOpeningRelayCourtyards()
    {
        if (!carveOpeningSignalCourtyard)
        {
            return;
        }

        if (useIntentionalDistrictSkeleton)
        {
            CarveSectorRelayCourtyards();
            return;
        }

        float arcDirection = Random.value < 0.5f ? -1f : 1f;

        savedOpeningSignalCourtyardCell = CarveDesignedRelayCourtyard(
            openingSignalCourtyardRadius,
            openingSignalCourtyardMinDistance,
            openingSignalCourtyardMaxDistance,
            GetBaseCell(),
            false,
            0f
        );

        if (!carveOpeningChainCourtyards || !IsSavedOpeningCourtyardCell(savedOpeningSignalCourtyardCell))
        {
            return;
        }

        savedOpeningPowerCourtyardCell = CarveDesignedRelayCourtyard(
            openingPowerCourtyardRadius,
            openingPowerCourtyardMinDistance,
            openingPowerCourtyardMaxDistance,
            savedOpeningSignalCourtyardCell,
            true,
            openingRelayRouteArcDegrees * arcDirection
        );

        if (!IsSavedOpeningCourtyardCell(savedOpeningPowerCourtyardCell))
        {
            return;
        }

        savedOpeningTransitCourtyardCell = CarveDesignedRelayCourtyard(
            openingTransitCourtyardRadius,
            openingTransitCourtyardMinDistance,
            openingTransitCourtyardMaxDistance,
            savedOpeningPowerCourtyardCell,
            true,
            openingRelayRouteArcDegrees * 0.7f * arcDirection
        );
    }

    void CarveSectorRelayCourtyards()
    {
        if (plannedRelaySectorCells.Count < 3)
        {
            PlanRelaySectorCells(GetBaseCell());
        }

        savedOpeningSignalCourtyardCell = CarveRelayCourtyardNearSectorTarget(
            plannedRelaySectorCells[0],
            openingSignalCourtyardRadius
        );

        savedOpeningPowerCourtyardCell = CarveRelayCourtyardNearSectorTarget(
            plannedRelaySectorCells[1],
            openingPowerCourtyardRadius
        );

        savedOpeningTransitCourtyardCell = CarveRelayCourtyardNearSectorTarget(
            plannedRelaySectorCells[2],
            openingTransitCourtyardRadius
        );
    }

    Vector2Int CarveRelayCourtyardNearSectorTarget(Vector2Int target, int radius)
    {
        Vector2Int selected = new Vector2Int(-1, -1);
        float bestScore = float.MaxValue;
        int searchRadius = Mathf.Max(radius + 1, 3);

        for (int x = -searchRadius; x <= searchRadius; x++)
        {
            for (int y = -searchRadius; y <= searchRadius; y++)
            {
                Vector2Int cell = target + new Vector2Int(x, y);

                if (!HasOpeningCourtyardClearance(cell, radius))
                {
                    continue;
                }

                float score = Vector2Int.Distance(cell, target) * 12f;
                score += Mathf.Abs(GetNearestServiceRouteDistance(cell) - (radius + 2f)) * 1.25f;
                score += CountWalkableCellsInEnvelope(cell, radius) * 0.75f;
                score += Random.Range(0f, 0.6f);

                if (score < bestScore)
                {
                    bestScore = score;
                    selected = cell;
                }
            }
        }

        if (!IsSavedOpeningCourtyardCell(selected))
        {
            selected = ClampToMapMargin(target, radius + openingRelayMainStreetClearance + 2);
        }

        CarveConnectorPath(FindNearestServiceRouteCell(selected), selected);
        CarvePlaza(selected, radius, false);
        plazaCenters.Add(selected);
        return selected;
    }

    Vector2Int CarveDesignedRelayCourtyard(
        int radius,
        int minDistance,
        int maxDistance,
        Vector2Int previousAnchor,
        bool extendRoute,
        float arcDegrees)
    {
        Vector2Int baseCell = GetBaseCell();
        float targetDistance = (minDistance + maxDistance) * 0.5f;
        Vector2 routeDirection = new Vector2(previousAnchor.x - baseCell.x, previousAnchor.y - baseCell.y).normalized;

        if (extendRoute && routeDirection.sqrMagnitude > 0.01f)
        {
            routeDirection = RotateDirection(routeDirection, arcDegrees);
        }

        Vector2 desiredPosition = new Vector2(baseCell.x, baseCell.y) + routeDirection * targetDistance;
        Vector2Int selected = new Vector2Int(-1, -1);
        float bestScore = float.MaxValue;
        int margin = radius + openingRelayMainStreetClearance + 1;

        for (int x = margin; x < width - margin; x++)
        {
            for (int y = margin; y < height - margin; y++)
            {
                Vector2Int cell = new Vector2Int(x, y);
                float distance = Vector2Int.Distance(baseCell, cell);

                if (distance < minDistance || distance > maxDistance ||
                    !HasOpeningCourtyardClearance(cell, radius))
                {
                    continue;
                }

                if (extendRoute && routeDirection.sqrMagnitude > 0.01f)
                {
                    Vector2 candidateDirection = new Vector2(cell.x - baseCell.x, cell.y - baseCell.y).normalized;

                    if (Vector2.Angle(candidateDirection, routeDirection) > openingRelayMaxArcDeviationDegrees)
                    {
                        continue;
                    }
                }

                float score = Mathf.Abs(distance - targetDistance) * 10f;
                score += CountWalkableCellsInEnvelope(cell, radius) * 7f;
                float serviceRouteDistance = GetNearestServiceRouteDistance(cell);
                float desiredServiceDistance = radius + openingRelayPreferredServiceSpurLength;
                score += Mathf.Abs(serviceRouteDistance - desiredServiceDistance) * 6f;

                if (serviceRouteDistance <= radius + 1f)
                {
                    score += 55f;
                }

                if (extendRoute && routeDirection.sqrMagnitude > 0.01f)
                {
                    score += Vector2.Distance(new Vector2(cell.x, cell.y), desiredPosition) * 4f;
                }

                score += Random.Range(0f, 1f);

                if (score < bestScore)
                {
                    bestScore = score;
                    selected = cell;
                }
            }
        }

        if (selected.x < 0)
        {
            if (!extendRoute || routeDirection.sqrMagnitude < 0.01f)
            {
                Vector2Int nearestAvenue = FindNearestCell(baseCell, mainStreetCells);
                Vector2Int approach = GetDominantCardinalDirection(nearestAvenue - baseCell);
                selected = baseCell + approach * Mathf.RoundToInt(targetDistance);
            }
            else
            {
                selected = Vector2Int.RoundToInt(desiredPosition);
            }

            if (!HasOpeningCourtyardClearance(selected, radius))
            {
                return new Vector2Int(-1, -1);
            }
        }

        CarveConnectorPath(FindNearestServiceRouteCell(selected), selected);
        CarvePlaza(selected, radius, false);
        plazaCenters.Add(selected);
        return selected;
    }

    Vector2 RotateDirection(Vector2 direction, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);
        return new Vector2(
            direction.x * cos - direction.y * sin,
            direction.x * sin + direction.y * cos
        ).normalized;
    }

    Vector2Int FindNearestServiceRouteCell(Vector2Int cell)
    {
        if (sideStreetCells.Count > 0)
        {
            return FindNearestCell(cell, sideStreetCells);
        }

        return FindNearestCell(cell, savedWalkableCells);
    }

    float GetNearestServiceRouteDistance(Vector2Int cell)
    {
        return Vector2Int.Distance(cell, FindNearestServiceRouteCell(cell));
    }

    bool HasOpeningCourtyardClearance(Vector2Int cell, int radius)
    {
        int margin = radius + 1;

        if (cell.x < margin || cell.x >= width - margin ||
            cell.y < margin || cell.y >= height - margin)
        {
            return false;
        }

        if (useIntentionalDistrictSkeleton && !IsCellInsideStarterArea(cell.x, cell.y))
        {
            return false;
        }

        if (Vector2Int.Distance(cell, GetBaseCell()) < baseCampRadius + radius + openingRelayCourtyardGap)
        {
            return false;
        }

        if (!useIntentionalDistrictSkeleton && IsCourtyardNearMainStreet(cell, radius))
        {
            return false;
        }

        for (int i = 0; i < plazaCenters.Count; i++)
        {
            int requiredSeparation = radius * 2 + openingRelayCourtyardGap;
            Vector2Int offset = cell - plazaCenters[i];

            if (Mathf.Abs(offset.x) < requiredSeparation && Mathf.Abs(offset.y) < requiredSeparation)
            {
                return false;
            }
        }

        return true;
    }

    bool IsCourtyardNearMainStreet(Vector2Int center, int radius)
    {
        int protectedRadius = radius + openingRelayMainStreetClearance;

        for (int x = -protectedRadius; x <= protectedRadius; x++)
        {
            for (int y = -protectedRadius; y <= protectedRadius; y++)
            {
                if (mainStreetCells.Contains(center + new Vector2Int(x, y)))
                {
                    return true;
                }
            }
        }

        return false;
    }

    bool HasEstablishedRouteNearDistrict(RectInt district, int margin)
    {
        int minX = Mathf.Max(1, district.xMin - margin);
        int maxX = Mathf.Min(width - 2, district.xMax + margin);
        int minY = Mathf.Max(1, district.yMin - margin);
        int maxY = Mathf.Min(height - 2, district.yMax + margin);

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                Vector2Int cell = new Vector2Int(x, y);

                if (mainStreetCells.Contains(cell) || sideStreetCells.Contains(cell) || alleyCells.Contains(cell))
                {
                    return true;
                }
            }
        }

        return false;
    }

    int CountWalkableCellsInEnvelope(Vector2Int center, int radius)
    {
        int count = 0;
        int envelopeRadius = radius + 1;

        for (int x = -envelopeRadius; x <= envelopeRadius; x++)
        {
            for (int y = -envelopeRadius; y <= envelopeRadius; y++)
            {
                Vector2Int cell = center + new Vector2Int(x, y);

                if (IsInsideBounds(cell) && walkableCells[cell.x, cell.y])
                {
                    count++;
                }
            }
        }

        return count;
    }

    bool HasNonWalkableNeighbor(Vector2Int cell)
    {
        Vector2Int[] neighbors =
        {
            cell + Vector2Int.up,
            cell + Vector2Int.down,
            cell + Vector2Int.left,
            cell + Vector2Int.right
        };

        for (int i = 0; i < neighbors.Length; i++)
        {
            Vector2Int neighbor = neighbors[i];

            if (!IsInsideBounds(neighbor))
            {
                continue;
            }

            if (!IsCellWalkable(neighbor.x, neighbor.y))
            {
                return true;
            }
        }

        return false;
    }

    bool IsFarEnoughFromCells(Vector2Int cell, List<Vector2Int> existingCells, float minimumDistance)
    {
        for (int i = 0; i < existingCells.Count; i++)
        {
            if (Vector2Int.Distance(cell, existingCells[i]) < minimumDistance)
            {
                return false;
            }
        }

        return true;
    }

    bool IsSavedOpeningCourtyardCell(Vector2Int cell)
    {
        return cell.x >= 0 && cell.x < width && cell.y >= 0 && cell.y < height;
    }

    Vector2Int PickBaseCampServiceSite(Vector2Int networkCenter)
    {
        Vector2Int bestCell = networkCenter;
        float bestScore = float.MaxValue;
        HashSet<Vector2Int> assessedCells = new HashSet<Vector2Int>();

        ScoreBaseCampCandidates(sideStreetCells, networkCenter, assessedCells, ref bestCell, ref bestScore, 0f);
        ScoreBaseCampCandidates(alleyCells, networkCenter, assessedCells, ref bestCell, ref bestScore, 14f);
        ScoreBaseCampCandidates(savedWalkableCells, networkCenter, assessedCells, ref bestCell, ref bestScore, 30f);

        return bestCell;
    }

    void CarveBaseCampServiceAccesses(Vector2Int center)
    {
        if (mainStreetCells.Count == 0)
        {
            return;
        }

        if (useIntentionalDistrictSkeleton)
        {
            Vector2Int[] accessDirections =
            {
                Vector2Int.up,
                Vector2Int.down,
                Vector2Int.left,
                Vector2Int.right
            };

            ShuffleDirections(accessDirections);
            int accessLength = nearZoneLoopRadius + nearZoneLoopVariation + 1;

            for (int i = 0; i < 3; i++)
            {
                CarveBaseCampAccess(center, accessDirections[i], accessLength, sideStreetCells);
            }

            return;
        }

        Vector2Int nearestAvenue = FindNearestCell(center, mainStreetCells);
        Vector2Int approachDirection = GetDominantCardinalDirection(nearestAvenue - center);
        Vector2Int lateralDirection = new Vector2Int(-approachDirection.y, approachDirection.x);

        CarveBaseCampAccess(center, approachDirection, baseCampRadius + 3, sideStreetCells);
        CarveBaseCampAccess(center, lateralDirection, baseCampRadius + 2, alleyCells);
        CarveBaseCampAccess(center, -lateralDirection, baseCampRadius + 2, alleyCells);
    }

    void CarveBaseCampAccess(Vector2Int center, Vector2Int direction, int length, List<Vector2Int> category)
    {
        for (int distance = baseCampRadius + 1; distance <= length; distance++)
        {
            Vector2Int cell = center + direction * distance;

            if (!IsInsideBounds(cell))
            {
                break;
            }

            CarveCorridor(cell, 0, category, true);
        }
    }

    Vector2Int GetDominantCardinalDirection(Vector2Int offset)
    {
        if (Mathf.Abs(offset.x) >= Mathf.Abs(offset.y))
        {
            return offset.x >= 0 ? Vector2Int.right : Vector2Int.left;
        }

        return offset.y >= 0 ? Vector2Int.up : Vector2Int.down;
    }

    void ScoreBaseCampCandidates(
        List<Vector2Int> candidates,
        Vector2Int networkCenter,
        HashSet<Vector2Int> assessedCells,
        ref Vector2Int bestCell,
        ref float bestScore,
        float categoryPenalty
    )
    {
        if (candidates == null || candidates.Count == 0 || mainStreetCells.Count == 0)
        {
            return;
        }

        float desiredCenterOffset = (baseCampMinimumCenterOffset + baseCampMaximumCenterOffset) * 0.5f;

        for (int i = 0; i < candidates.Count; i++)
        {
            Vector2Int candidate = candidates[i];

            if (!assessedCells.Add(candidate) || mainStreetCells.Contains(candidate))
            {
                continue;
            }

            int requiredMargin = baseCampRadius + 2;

            if (candidate.x < requiredMargin || candidate.x >= width - requiredMargin ||
                candidate.y < requiredMargin || candidate.y >= height - requiredMargin)
            {
                continue;
            }

            float centerOffset = Vector2Int.Distance(candidate, networkCenter);
            float avenueOffset = Vector2Int.Distance(candidate, FindNearestCell(candidate, mainStreetCells));
            float score = categoryPenalty;
            score += Mathf.Abs(centerOffset - desiredCenterOffset) * 2.5f;
            score += Mathf.Abs(avenueOffset - baseCampPreferredAvenueOffset) * 8f;

            if (centerOffset < baseCampMinimumCenterOffset)
            {
                score += (baseCampMinimumCenterOffset - centerOffset) * 28f;
            }
            else if (centerOffset > baseCampMaximumCenterOffset)
            {
                score += (centerOffset - baseCampMaximumCenterOffset) * 16f;
            }

            if (avenueOffset <= baseCampRadius + 1)
            {
                score += 180f;
            }

            int connectedNeighbors = CountCardinalWalkableNeighbors(candidate);
            score += connectedNeighbors == 2 ? 0f : Mathf.Abs(connectedNeighbors - 2) * 12f;
            score += Random.value * 2f;

            if (score < bestScore)
            {
                bestScore = score;
                bestCell = candidate;
            }
        }
    }

    Vector2Int FindRegionCenterCell(List<Vector2Int> region)
    {
        if (region == null || region.Count == 0)
        {
            return GetBaseCell();
        }

        Vector2 average = Vector2.zero;

        for (int i = 0; i < region.Count; i++)
        {
            average += new Vector2(region[i].x, region[i].y);
        }

        average /= region.Count;
        Vector2Int bestCell = region[0];
        float bestDistance = float.MaxValue;

        for (int i = 0; i < region.Count; i++)
        {
            float distance = Vector2.SqrMagnitude(new Vector2(region[i].x, region[i].y) - average);

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestCell = region[i];
            }
        }

        return bestCell;
    }

    int CountCardinalWalkableNeighbors(Vector2Int cell)
    {
        int count = 0;

        Vector2Int[] neighbors =
        {
        cell + Vector2Int.up,
        cell + Vector2Int.down,
        cell + Vector2Int.left,
        cell + Vector2Int.right
    };

        for (int i = 0; i < neighbors.Length; i++)
        {
            Vector2Int neighbor = neighbors[i];

            if (!IsInsideBounds(neighbor))
            {
                continue;
            }

            if (walkableCells[neighbor.x, neighbor.y])
            {
                count++;
            }
        }

        return count;
    }

    bool TouchesMainStreet(Vector2Int cell)
    {
        Vector2Int[] neighbors =
        {
        cell + Vector2Int.up,
        cell + Vector2Int.down,
        cell + Vector2Int.left,
        cell + Vector2Int.right
    };

        for (int i = 0; i < neighbors.Length; i++)
        {
            Vector2Int neighbor = neighbors[i];

            if (!IsInsideBounds(neighbor))
            {
                continue;
            }

            if (IsCellMainStreet(neighbor.x, neighbor.y))
            {
                return true;
            }
        }

        return false;
    }

    bool HasSideStreetOrAlleyNearby(Vector2Int cell)
    {
        for (int x = -2; x <= 2; x++)
        {
            for (int y = -2; y <= 2; y++)
            {
                Vector2Int checkCell = new Vector2Int(cell.x + x, cell.y + y);

                if (!IsInsideBounds(checkCell))
                {
                    continue;
                }

                if (IsCellSideStreet(checkCell.x, checkCell.y) || IsCellAlley(checkCell.x, checkCell.y))
                {
                    return true;
                }
            }
        }

        return false;
    }

    bool IsTooCloseToExistingPlaza(Vector2Int cell)
    {
        for (int i = 0; i < plazaCenters.Count; i++)
        {
            if (Vector2Int.Distance(cell, plazaCenters[i]) < minimumPlazaSpacing)
            {
                return true;
            }
        }

        return false;
    }

    bool IsTooCloseToBase(Vector2Int cell)
    {
        return Vector2Int.Distance(cell, GetBaseCell()) <= baseExclusionRadius;
    }

    bool WouldPlazaTouchBaseExclusion(Vector2Int plazaCenter, int radius)
    {
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                Vector2Int cell = new Vector2Int(plazaCenter.x + x, plazaCenter.y + y);

                if (Vector2Int.Distance(cell, GetBaseCell()) <= baseExclusionRadius)
                {
                    return true;
                }
            }
        }

        return false;
    }

    void CarveLocalConnections()
    {
        int attempts = useIntentionalDistrictSkeleton
            ? ShouldUseTemplateGuidedLayout() ? 1 : 3
            : Mathf.RoundToInt((sideStreetCount + alleyCount) * loopConnectionChance);

        for (int i = 0; i < attempts; i++)
        {
            if (carvedCells.Count < 2) return;

            Vector2Int start = carvedCells[Random.Range(0, carvedCells.Count)];
            Vector2Int target = FindNearbyWalkableCell(start, 4, 10); // shorter distance

            if (target == start) continue;

            CarveConnectorPath(start, target);
        }
    }


    Vector2Int FindNearestCell(Vector2Int start, List<Vector2Int> cells)
    {
        if (cells == null || cells.Count == 0)
        {
            return start;
        }

        Vector2Int bestCell = cells[0];
        float bestDistance = float.MaxValue;

        for (int i = 0; i < cells.Count; i++)
        {
            float distance = Vector2Int.Distance(start, cells[i]);

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestCell = cells[i];
            }
        }

        return bestCell;
    }

    Vector2Int FindNearbyWalkableCell(Vector2Int start, int minDistance, int maxDistance)
    {
        Vector2Int bestCell = start;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < 25; i++)
        {
            Vector2Int candidate = carvedCells[Random.Range(0, carvedCells.Count)];
            float distance = Vector2Int.Distance(start, candidate);

            if (distance >= minDistance && distance <= maxDistance && distance < bestDistance)
            {
                bestDistance = distance;
                bestCell = candidate;
            }
        }

        return bestCell;
    }

    void CarveConnectorPath(Vector2Int start, Vector2Int target)
    {
        Vector2Int current = start;

        int safety = width + height;

        while (current != target && safety > 0)
        {
            safety--;

            CarveCorridor(current, alleyRadius, alleyCells, true);

            Vector2Int delta = target - current;

            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                current += delta.x > 0 ? Vector2Int.right : Vector2Int.left;
            }
            else
            {
                current += delta.y > 0 ? Vector2Int.up : Vector2Int.down;
            }

            if (!IsInsideBounds(current))
            {
                break;
            }
        }
    }

    void CarvePlaza(Vector2Int center, int radius, bool isBaseCamp)
    {
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                Vector2Int cell = new Vector2Int(center.x + x, center.y + y);

                if (!IsInsideBounds(cell)) continue;
                if (useIntentionalDistrictSkeleton && !IsCellInsideStarterArea(cell.x, cell.y)) continue;

                MarkWalkable(cell);

                if (!isBaseCamp)
                {
                    plazaCells[cell.x, cell.y] = true;

                    if (!savedPlazaCells.Contains(cell))
                    {
                        savedPlazaCells.Add(cell);
                    }

                    savedMainStreetCells.Remove(cell);
                    savedSideStreetCells.Remove(cell);
                    savedAlleyCells.Remove(cell);
                }
            }
        }
    }

    void SaveCellCategory(Vector2Int cell, List<Vector2Int> categoryList)
    {
        if (categoryList == mainStreetCells)
        {
            if (!savedMainStreetCells.Contains(cell))
            {
                savedMainStreetCells.Add(cell);
            }

            return;
        }

        if (categoryList == sideStreetCells)
        {
            if (!savedSideStreetCells.Contains(cell))
            {
                savedSideStreetCells.Add(cell);
            }

            return;
        }

        if (categoryList == alleyCells)
        {
            if (!savedAlleyCells.Contains(cell))
            {
                savedAlleyCells.Add(cell);
            }
        }
    }

    void MarkWalkable(Vector2Int cell)
    {
        if (!IsInsideBounds(cell)) return;

        if (useIntentionalDistrictSkeleton && !IsCellInsideStarterArea(cell.x, cell.y))
        {
            return;
        }

        if (!walkableCells[cell.x, cell.y])
        {
            walkableCells[cell.x, cell.y] = true;
            carvedCells.Add(cell);

            if (!savedWalkableCells.Contains(cell))
            {
                savedWalkableCells.Add(cell);
            }
        }
    }

    void CreateFloor()
    {
        if (!useIntentionalDistrictSkeleton)
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Generated_Floor";
            floor.transform.SetParent(transform);

            floor.transform.position = transform.position + new Vector3(0f, -floorThickness * 0.5f, 0f);
            floor.transform.localScale = new Vector3(width * cellSize, floorThickness, height * cellSize);
            floor.isStatic = true;

            ApplyMaterial(floor, floorMaterial);
            return;
        }

        CreateStarterAreaFloorChunks();
    }

    void CreateStarterAreaFloorChunks()
    {
        bool[,] visited = new bool[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!IsCellInsideStarterArea(x, y) || visited[x, y])
                {
                    continue;
                }

                RectInt chunk = FindStarterAreaChunk(x, y, visited);
                CreateFloorChunk(chunk);
                MarkChunkVisited(chunk, visited);
            }
        }
    }

    RectInt FindStarterAreaChunk(int startX, int startY, bool[,] visited)
    {
        int chunkWidth = 0;
        int maxWidth = Mathf.Min(maxBuildingChunkWidth, width - startX);

        for (int x = startX; x < startX + maxWidth; x++)
        {
            if (!IsCellInsideStarterArea(x, startY) || visited[x, startY])
            {
                break;
            }

            chunkWidth++;
        }

        chunkWidth = Mathf.Max(1, chunkWidth);
        int chunkDepth = 1;
        int maxDepth = Mathf.Min(maxBuildingChunkDepth, height - startY);

        for (int y = startY + 1; y < startY + maxDepth; y++)
        {
            bool rowClear = true;

            for (int x = startX; x < startX + chunkWidth; x++)
            {
                if (!IsCellInsideStarterArea(x, y) || visited[x, y])
                {
                    rowClear = false;
                    break;
                }
            }

            if (!rowClear)
            {
                break;
            }

            chunkDepth++;
        }

        return new RectInt(startX, startY, chunkWidth, chunkDepth);
    }

    void CreateFloorChunk(RectInt chunk)
    {
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Generated_Floor_Chunk";
        floor.transform.SetParent(transform);

        float centerCellX = chunk.x + chunk.width * 0.5f;
        float centerCellY = chunk.y + chunk.height * 0.5f;
        floor.transform.position = GridPositionToWorld(centerCellX, centerCellY, -floorThickness * 0.5f);
        floor.transform.localScale = new Vector3(chunk.width * cellSize, floorThickness, chunk.height * cellSize);
        floor.isStatic = true;

        ApplyMaterial(floor, floorMaterial);
    }

    void CreateBuildings()
    {
        bool[,] visited = new bool[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (useIntentionalDistrictSkeleton && !IsCellInsideStarterArea(x, y))
                {
                    visited[x, y] = true;
                    continue;
                }

                if (walkableCells[x, y])
                {
                    if (showWalkableMarkers)
                    {
                        CreateWalkableMarker(x, y);
                    }

                    continue;
                }

                if (visited[x, y])
                {
                    continue;
                }

                RectInt chunk = FindBuildingChunk(x, y, visited);
                CreateBuildingChunk(chunk);
                MarkChunkVisited(chunk, visited);
            }
        }

        if (useIntentionalDistrictSkeleton)
        {
            CreateStarterAreaBoundaryBuildings();
        }

        CreatePlazaRegionMarkers();
    }

    void CreateStarterAreaBoundaryBuildings()
    {
        bool[,] visited = new bool[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!IsStarterAreaBoundaryBuildingCell(x, y) || visited[x, y])
                {
                    continue;
                }

                RectInt chunk = FindBoundaryBuildingChunk(x, y, visited);
                CreateBoundaryBuildingChunk(chunk);
                MarkChunkVisited(chunk, visited);
            }
        }
    }

    bool IsStarterAreaBoundaryBuildingCell(int x, int y)
    {
        if (!IsInsideBounds(new Vector2Int(x, y)) || IsCellInsideStarterArea(x, y))
        {
            return false;
        }

        for (int dx = -2; dx <= 2; dx++)
        {
            for (int dy = -2; dy <= 2; dy++)
            {
                if (dx == 0 && dy == 0)
                {
                    continue;
                }

                int checkX = x + dx;
                int checkY = y + dy;

                if (checkX < 0 || checkX >= width || checkY < 0 || checkY >= height)
                {
                    continue;
                }

                if (IsCellInsideStarterArea(checkX, checkY))
                {
                    return true;
                }
            }
        }

        return false;
    }

    RectInt FindBoundaryBuildingChunk(int startX, int startY, bool[,] visited)
    {
        int chunkWidth = 0;
        int maxWidth = Mathf.Min(maxBuildingChunkWidth, width - startX);

        for (int x = startX; x < startX + maxWidth; x++)
        {
            if (!IsStarterAreaBoundaryBuildingCell(x, startY) || visited[x, startY])
            {
                break;
            }

            chunkWidth++;
        }

        chunkWidth = Mathf.Max(1, chunkWidth);
        int chunkDepth = 1;
        int maxDepth = Mathf.Min(maxBuildingChunkDepth, height - startY);

        for (int y = startY + 1; y < startY + maxDepth; y++)
        {
            bool rowClear = true;

            for (int x = startX; x < startX + chunkWidth; x++)
            {
                if (!IsStarterAreaBoundaryBuildingCell(x, y) || visited[x, y])
                {
                    rowClear = false;
                    break;
                }
            }

            if (!rowClear)
            {
                break;
            }

            chunkDepth++;
        }

        return new RectInt(startX, startY, chunkWidth, chunkDepth);
    }

    void CreateBoundaryBuildingChunk(RectInt chunk)
    {
        GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
        block.name = "StarterArea_Boundary_Building";
        block.transform.SetParent(transform);

        float randomHeight = Random.Range(minBuildingHeight * 1.15f, maxBuildingHeight * 1.2f);
        float centerCellX = chunk.x + chunk.width * 0.5f;
        float centerCellY = chunk.y + chunk.height * 0.5f;

        block.transform.position = GridPositionToWorld(centerCellX, centerCellY, randomHeight * 0.5f);
        block.transform.localScale = new Vector3(
            chunk.width * cellSize - buildingChunkGap,
            randomHeight,
            chunk.height * cellSize - buildingChunkGap
        );
        block.isStatic = true;

        ApplyMaterial(block, buildingMaterial);
    }

    RectInt FindBuildingChunk(int startX, int startY, bool[,] visited)
    {
        int chunkWidth = 0;
        int maxWidth = Mathf.Min(maxBuildingChunkWidth, width - startX);

        for (int x = startX; x < startX + maxWidth; x++)
        {
            if ((useIntentionalDistrictSkeleton && !IsCellInsideStarterArea(x, startY)) ||
                walkableCells[x, startY] ||
                visited[x, startY])
            {
                break;
            }

            chunkWidth++;
        }

        chunkWidth = Mathf.Max(1, chunkWidth);

        int chunkDepth = 1;
        int maxDepth = Mathf.Min(maxBuildingChunkDepth, height - startY);

        for (int y = startY + 1; y < startY + maxDepth; y++)
        {
            bool rowClear = true;

            for (int x = startX; x < startX + chunkWidth; x++)
            {
                if ((useIntentionalDistrictSkeleton && !IsCellInsideStarterArea(x, y)) ||
                    walkableCells[x, y] ||
                    visited[x, y])
                {
                    rowClear = false;
                    break;
                }
            }

            if (!rowClear)
            {
                break;
            }

            chunkDepth++;
        }

        return new RectInt(startX, startY, chunkWidth, chunkDepth);
    }

    void MarkChunkVisited(RectInt chunk, bool[,] visited)
    {
        for (int x = chunk.xMin; x < chunk.xMax; x++)
        {
            for (int y = chunk.yMin; y < chunk.yMax; y++)
            {
                visited[x, y] = true;
            }
        }
    }

    void CreateBuildingChunk(RectInt chunk)
    {
        GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
        block.name = "Building_Block";
        block.transform.SetParent(transform);

        float randomHeight = Random.Range(minBuildingHeight, maxBuildingHeight);

        float centerCellX = chunk.x + chunk.width * 0.5f;
        float centerCellY = chunk.y + chunk.height * 0.5f;

        Vector3 worldPosition = GridPositionToWorld(centerCellX, centerCellY, randomHeight * 0.5f);

        block.transform.position = worldPosition;
        block.transform.localScale = new Vector3(
            chunk.width * cellSize - buildingChunkGap,
            randomHeight,
            chunk.height * cellSize - buildingChunkGap
        );
        block.isStatic = true;

        ApplyMaterial(block, buildingMaterial);
    }

    void CreateWalkableMarker(int x, int y)
    {
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        marker.name = "Walkable_Debug_Marker";
        marker.transform.SetParent(transform);

        marker.transform.position = CellToWorld(x, y, 0.03f);
        marker.transform.localScale = new Vector3(cellSize * 0.85f, 0.05f, cellSize * 0.85f);

        ApplyMaterial(marker, walkableMaterial);
    }

    void CreatePlazaRegionMarkers()
    {
        bool[,] visited = new bool[width, height];
        Vector2Int[] neighbors =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!plazaCells[x, y] || visited[x, y])
                {
                    continue;
                }

                Queue<Vector2Int> queue = new Queue<Vector2Int>();
                Vector2Int start = new Vector2Int(x, y);
                int minX = x;
                int maxX = x;
                int minY = y;
                int maxY = y;
                queue.Enqueue(start);
                visited[x, y] = true;

                while (queue.Count > 0)
                {
                    Vector2Int cell = queue.Dequeue();
                    minX = Mathf.Min(minX, cell.x);
                    maxX = Mathf.Max(maxX, cell.x);
                    minY = Mathf.Min(minY, cell.y);
                    maxY = Mathf.Max(maxY, cell.y);

                    for (int i = 0; i < neighbors.Length; i++)
                    {
                        Vector2Int neighbor = cell + neighbors[i];

                        if (!IsInsideBounds(neighbor) || visited[neighbor.x, neighbor.y] || !plazaCells[neighbor.x, neighbor.y])
                        {
                            continue;
                        }

                        visited[neighbor.x, neighbor.y] = true;
                        queue.Enqueue(neighbor);
                    }
                }

                CreatePlazaRegionMarker(minX, maxX, minY, maxY);
            }
        }
    }

    void CreatePlazaRegionMarker(int minX, int maxX, int minY, int maxY)
    {
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        marker.name = "Plaza_Region_Marker";
        marker.transform.SetParent(transform);
        int regionWidth = maxX - minX + 1;
        int regionHeight = maxY - minY + 1;
        marker.transform.position = GridPositionToWorld(minX + regionWidth * 0.5f, minY + regionHeight * 0.5f, 0.04f);
        marker.transform.localScale = new Vector3(regionWidth * cellSize * 0.985f, 0.06f, regionHeight * cellSize * 0.985f);
        marker.isStatic = true;

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

        ApplyMaterial(marker, plazaMaterial);
    }

    void CreateBaseMarker(Vector2Int center)
    {
        GameObject baseMarker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Collider col = baseMarker.GetComponent<Collider>();

        if (col != null)
        {
            col.isTrigger = true;
        }
        baseMarker.layer = LayerMask.NameToLayer("Default");
        baseMarker.name = "Generated_Base_Camp_Marker";
        baseMarker.transform.SetParent(transform);

        baseMarker.transform.position = CellToWorld(center.x, center.y, 0.15f);
        baseMarker.transform.localScale = new Vector3(cellSize * 2f, 0.15f, cellSize * 2f);

        ApplyMaterial(baseMarker, baseMaterial);
    }

    public Vector3 CellToWorld(int x, int y, float worldY)
    {
        return GridPositionToWorld(x + 0.5f, y + 0.5f, worldY);
    }

    Vector3 GridPositionToWorld(float gridX, float gridY, float worldY)
    {
        float worldX = gridX * cellSize - width * cellSize * 0.5f;
        float worldZ = gridY * cellSize - height * cellSize * 0.5f;

        return transform.position + new Vector3(worldX, worldY, worldZ);
    }

    bool IsInsideBounds(Vector2Int cell)
    {
        return cell.x >= 1 && cell.x < width - 1 && cell.y >= 1 && cell.y < height - 1;
    }
    void ShuffleDirections(Vector2Int[] directions)
    {
        for (int i = 0; i < directions.Length; i++)
        {
            int randomIndex = Random.Range(i, directions.Length);

            Vector2Int temp = directions[i];
            directions[i] = directions[randomIndex];
            directions[randomIndex] = temp;
        }
    }
    void CarveMainStreetLine(Vector2Int start, Vector2Int direction)
    {
        Vector2Int current = start;

        for (int i = 0; i < mainStreetLength; i++)
        {
            if (!IsInsideBounds(current))
            {
                break;
            }

            // allow main streets to overwrite spacing rules
            CarveCorridor(current, mainStreetRadius, mainStreetCells, true);

            // VERY slight chance to bend
            if (Random.value < 0.03f)
            {
                direction = PickNewDirection(direction);
            }

            current += direction;
        }
    }
    Vector2Int RandomDirection()
    {
        int choice = Random.Range(0, 4);

        if (choice == 0) return Vector2Int.up;
        if (choice == 1) return Vector2Int.down;
        if (choice == 2) return Vector2Int.left;

        return Vector2Int.right;
    }

    void ApplyMaterial(GameObject obj, Material material)
    {
        if (material == null) return;

        Renderer renderer = obj.GetComponent<Renderer>();

        if (renderer != null)
        {
            renderer.sharedMaterial = material;
        }
    }
}
