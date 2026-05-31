using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class UpperCityLayerGenerator : MonoBehaviour
{
    public CityBlockoutGenerator city;

    [Header("Generation")]
    public bool generateOnStart = false;
    public bool useRecommendedAtmosphericDefaults = true;
    public int seedOffset = 9107;
    [Range(0.1f, 1f)] public float coverage = 1f;
    public int chunkSizeInCells = 28;
    public int playableCeilingChunkSizeInCells = 4;
    public int playableCeilingTransitionCells = 16;
    public int overhangCells = 110;

    [Header("Height")]
    public float minLayerHeight = 130f;
    public float maxLayerHeight = 162f;
    public float minSlabThickness = 22f;
    public float maxSlabThickness = 34f;
    public float chunkGap = -0.5f;

    [Header("Sunlight Holes")]
    public int minimumHoleCount = 2;
    public float holesPerThousandCells = 0.45f;
    public int minHoleRadiusCells = 2;
    public int maxHoleRadiusCells = 3;
    public bool createSunlightShafts = true;
    public bool createShaftForEveryHole = true;
    public int maxShaftLights = 12;
    public bool reserveOpeningRelaySunlightShafts = true;

    [Header("Support Pillars")]
    public int minimumPillarCount = 2;
    public float pillarsPerThousandCells = 0.75f;
    public float minPillarRadius = 5.5f;
    public float maxPillarRadius = 10f;
    public bool createOuterSupportPillars = true;
    public int outerSupportPillarCount = 12;
    public int outerSupportPillarMinOffsetCells = 22;
    public int outerSupportPillarMaxOffsetCells = 120;

    [Header("Underside Detail")]
    public bool createUndersideRibs = true;
    public int ribSpacingInCells = 18;
    [Range(0f, 1f)] public float ribCoverage = 0.7f;
    public float ribThickness = 1.25f;
    public float ribDrop = 3f;
    public bool createHangingMasses = true;
    public int hangingMassCount = 22;
    public float minHangingMassDrop = 3f;
    public float maxHangingMassDrop = 10f;

    [Header("Far City Silhouette")]
    public bool createFarCitySilhouette = true;
    public int farSilhouetteCount = 70;
    public int farSilhouetteDistanceCells = 145;
    public float minFarSilhouetteHeight = 34f;
    public float maxFarSilhouetteHeight = 115f;
    public float farSilhouetteDepth = 22f;

    [Header("Atmosphere Placeholder")]
    public bool applyAtmosphereSettings = true;
    public Color fogColor = new Color(0.05f, 0.07f, 0.075f, 1f);
    public float fogDensity = 0.006f;
    public Color ambientColor = new Color(0.05f, 0.055f, 0.06f, 1f);
    public bool tuneDirectionalLight = true;
    public Color directionalLightColor = new Color(0.74f, 0.82f, 0.88f, 1f);
    public float directionalLightIntensity = 0.45f;
    public bool applyCameraBackgroundColor = true;
    public Color cameraBackgroundColor = new Color(0.33f, 0.48f, 0.62f, 1f);
    public bool createDistanceHaze = true;
    public int hazeWallDistanceCells = 118;
    public float hazeWallHeight = 82f;
    public float hazeWallThickness = 8f;
    public Color distanceHazeColor = new Color(0.055f, 0.105f, 0.12f, 0.18f);

    [Header("Collision")]
    public bool addSlabColliders = false;
    public bool addPillarColliders = false;

    [Header("Materials")]
    public Material upperLayerMaterial;
    public Material pillarMaterial;
    public Material sunlightShaftMaterial;
    public Material farSilhouetteMaterial;
    public Material distanceHazeMaterial;
    public Material sunlightGroundPatchMaterial;
    public Color upperLayerColor = new Color(0.025f, 0.035f, 0.055f, 1f);
    public Color pillarColor = new Color(0.035f, 0.04f, 0.05f, 1f);
    public Color sunlightShaftColor = new Color(1f, 0.84f, 0.5f, 0.12f);
    public Color sunlightGroundPatchColor = new Color(1f, 0.78f, 0.35f, 0.28f);
    public Color farSilhouetteColor = new Color(0.018f, 0.022f, 0.03f, 1f);

    private readonly List<Vector2Int> holeCenters = new List<Vector2Int>();
    private readonly List<int> holeRadii = new List<int>();

    private Material generatedUpperLayerMaterial;
    private Material generatedPillarMaterial;
    private Material generatedSunlightShaftMaterial;
    private Material generatedFarSilhouetteMaterial;
    private Material generatedDistanceHazeMaterial;
    private Material generatedSunlightGroundPatchMaterial;

    void Start()
    {
        if (generateOnStart)
        {
            GenerateUpperCityLayer();
        }
    }

    [ContextMenu("Generate Upper City Layer")]
    public void GenerateUpperCityLayer()
    {
        ResolveCity();

        if (city == null || !city.HasGeneratedMap())
        {
            Debug.LogWarning("UpperCityLayerGenerator: No generated city data found.");
            return;
        }

        Random.State previousState = Random.state;
        Random.InitState(city.seed + seedOffset);

        if (useRecommendedAtmosphericDefaults)
        {
            ApplyRecommendedAtmosphericDefaults();
        }

        ClearUpperCityLayer();
        BuildSunlightHoles();
        GenerateSlabChunks();

        if (createUndersideRibs)
        {
            GenerateUndersideRibs();
        }

        if (createHangingMasses)
        {
            GenerateHangingMasses();
        }

        GenerateSupportPillars();

        if (createOuterSupportPillars)
        {
            GenerateOuterSupportPillars();
        }

        if (createFarCitySilhouette)
        {
            GenerateFarCitySilhouette();
        }

        if (createDistanceHaze)
        {
            GenerateDistanceHaze();
        }

        if (createSunlightShafts)
        {
            GenerateSunlightShafts();
        }

        if (applyAtmosphereSettings)
        {
            ApplyAtmosphereSettings();
        }

        Random.state = previousState;
    }

    [ContextMenu("Clear Upper City Layer")]
    public void ClearUpperCityLayer()
    {
        List<GameObject> childrenToDelete = new List<GameObject>();

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            childrenToDelete.Add(transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < childrenToDelete.Count; i++)
        {
            DestroyGeneratedObject(childrenToDelete[i]);
        }

        holeCenters.Clear();
        holeRadii.Clear();
    }

    void ResolveCity()
    {
        if (city == null)
        {
            city = FindFirstObjectByType<CityBlockoutGenerator>();
        }
    }

    void ApplyRecommendedAtmosphericDefaults()
    {
        coverage = 1f;
        chunkSizeInCells = 28;
        playableCeilingChunkSizeInCells = 4;
        playableCeilingTransitionCells = 16;
        overhangCells = 110;

        minLayerHeight = 130f;
        maxLayerHeight = 162f;
        minSlabThickness = 22f;
        maxSlabThickness = 34f;
        chunkGap = -0.5f;

        minimumHoleCount = 2;
        holesPerThousandCells = 0.45f;
        minHoleRadiusCells = 2;
        maxHoleRadiusCells = 3;
        createShaftForEveryHole = true;
        maxShaftLights = 12;

        minimumPillarCount = 2;
        pillarsPerThousandCells = 0.75f;
        minPillarRadius = 5.5f;
        maxPillarRadius = 10f;
        createOuterSupportPillars = true;
        outerSupportPillarCount = 12;
        outerSupportPillarMinOffsetCells = 22;
        outerSupportPillarMaxOffsetCells = 120;

        createUndersideRibs = true;
        ribSpacingInCells = 18;
        ribCoverage = 0.7f;
        ribThickness = 1.25f;
        ribDrop = 3f;

        createHangingMasses = true;
        hangingMassCount = 22;
        minHangingMassDrop = 3f;
        maxHangingMassDrop = 10f;

        createFarCitySilhouette = true;
        farSilhouetteCount = 80;
        farSilhouetteDistanceCells = 145;
        minFarSilhouetteHeight = 34f;
        maxFarSilhouetteHeight = 115f;
        farSilhouetteDepth = 30f;

        tuneDirectionalLight = true;
        directionalLightColor = new Color(0.74f, 0.82f, 0.88f, 1f);
        directionalLightIntensity = 0.45f;
        applyCameraBackgroundColor = true;
        cameraBackgroundColor = new Color(0.33f, 0.48f, 0.62f, 1f);
        createDistanceHaze = true;
        hazeWallDistanceCells = 118;
        hazeWallHeight = 118f;
        hazeWallThickness = 8f;
        distanceHazeColor = new Color(0.055f, 0.105f, 0.12f, 0.18f);

        upperLayerColor = new Color(0.025f, 0.035f, 0.055f, 1f);
        pillarColor = new Color(0.035f, 0.04f, 0.05f, 1f);
        sunlightShaftColor = new Color(1f, 0.84f, 0.5f, 0.12f);
        sunlightGroundPatchColor = new Color(1f, 0.78f, 0.35f, 0.28f);
        farSilhouetteColor = new Color(0.018f, 0.022f, 0.03f, 1f);
        fogColor = new Color(0.05f, 0.07f, 0.075f, 1f);
        fogDensity = 0.006f;
        ambientColor = new Color(0.05f, 0.055f, 0.06f, 1f);
    }

    void BuildSunlightHoles()
    {
        int mapArea = city.GetGridWidth() * city.GetGridHeight();
        int holeCount = Mathf.Max(minimumHoleCount, Mathf.RoundToInt(mapArea * holesPerThousandCells / 1000f));

        AddHole(city.GetBaseCell(), maxHoleRadiusCells);
        AddOpeningRelaySunlightHoles();

        List<Vector2Int> plazas = city.GetSavedPlazaCells();
        int attempts = 0;

        while (holeCenters.Count < holeCount && attempts < holeCount * 80)
        {
            attempts++;
            int radius = Random.Range(minHoleRadiusCells, maxHoleRadiusCells + 1);
            Vector2Int candidate = PickHoleCandidate(plazas, radius);
            AddHole(candidate, radius);
        }

        attempts = 0;

        while (holeCenters.Count < holeCount && attempts < holeCount * 120)
        {
            attempts++;
            Vector2Int candidate = PickHoleCandidate(plazas, 1);
            AddHole(candidate, 1);
        }
    }

    void AddOpeningRelaySunlightHoles()
    {
        if (!reserveOpeningRelaySunlightShafts)
        {
            return;
        }

        AddOpeningRelaySunlightHole(InfrastructureNodeType.SignalRelay);
        AddOpeningRelaySunlightHole(InfrastructureNodeType.PowerJunction);
        AddOpeningRelaySunlightHole(InfrastructureNodeType.TransitLift);
    }

    void AddOpeningRelaySunlightHole(InfrastructureNodeType nodeType)
    {
        Vector2Int cell;

        if (city.TryGetOpeningRelayCourtyardCell(nodeType, out cell) &&
            CanPlaceSunlightHole(cell, minHoleRadiusCells))
        {
            AddHole(cell, minHoleRadiusCells);
        }
    }

    Vector2Int PickHoleCandidate(List<Vector2Int> plazas, int radius)
    {
        if (plazas != null && plazas.Count > 0 && Random.value < 0.55f)
        {
            for (int i = 0; i < 30; i++)
            {
                Vector2Int plazaCandidate = plazas[Random.Range(0, plazas.Count)];

                if (CanPlaceSunlightHole(plazaCandidate, radius))
                {
                    return plazaCandidate;
                }
            }
        }

        List<Vector2Int> walkableCells = city.GetSavedWalkableCells();

        if (walkableCells.Count == 0)
        {
            return city.GetBaseCell();
        }

        for (int i = 0; i < 30; i++)
        {
            Vector2Int candidate = walkableCells[Random.Range(0, walkableCells.Count)];

            if (!CanPlaceSunlightHole(candidate, radius))
            {
                continue;
            }

            if (city.IsCellPlaza(candidate.x, candidate.y) || city.IsCellMainStreet(candidate.x, candidate.y) || city.IsCellSideStreet(candidate.x, candidate.y))
            {
                return candidate;
            }
        }

        return city.GetBaseCell();
    }

    bool CanPlaceSunlightHole(Vector2Int cell, int radius)
    {
        if (IsInsideSunlightHole(cell))
        {
            return false;
        }

        int clearRadius = Mathf.Max(1, radius - 1);

        for (int x = -clearRadius; x <= clearRadius; x++)
        {
            for (int y = -clearRadius; y <= clearRadius; y++)
            {
                Vector2Int checkCell = new Vector2Int(cell.x + x, cell.y + y);

                if (Vector2Int.Distance(cell, checkCell) > clearRadius)
                {
                    continue;
                }

                if (checkCell.x < 0 || checkCell.x >= city.GetGridWidth() || checkCell.y < 0 || checkCell.y >= city.GetGridHeight())
                {
                    return false;
                }

                if (!city.IsCellWalkable(checkCell.x, checkCell.y))
                {
                    return false;
                }
            }
        }

        return true;
    }

    void AddHole(Vector2Int center, int radius)
    {
        if (IsInsideSunlightHole(center))
        {
            return;
        }

        holeCenters.Add(center);
        holeRadii.Add(Mathf.Max(1, radius));
    }

    void GenerateSlabChunks()
    {
        int width = city.GetGridWidth();
        int height = city.GetGridHeight();
        int startX = -overhangCells;
        int startY = -overhangCells;
        int endX = width + overhangCells;
        int endY = height + overhangCells;

        for (int x = startX; x < endX; x += chunkSizeInCells)
        {
            for (int y = startY; y < endY; y += chunkSizeInCells)
            {
                int chunkWidth = Mathf.Min(chunkSizeInCells, endX - x);
                int chunkHeight = Mathf.Min(chunkSizeInCells, endY - y);
                Vector2Int centerCell = new Vector2Int(x + chunkWidth / 2, y + chunkHeight / 2);

                if (IsChunkCenterInsideFineCeilingArea(centerCell))
                {
                    continue;
                }

                CreateSlabChunk(centerCell, chunkWidth, chunkHeight);
            }
        }

        GeneratePlayableCeilingChunks(width, height);
    }

    void GeneratePlayableCeilingChunks(int width, int height)
    {
        int chunkSize = Mathf.Max(1, playableCeilingChunkSizeInCells);
        int transition = Mathf.Max(0, playableCeilingTransitionCells);
        int startX = -transition;
        int startY = -transition;
        int endX = width + transition;
        int endY = height + transition;

        for (int x = startX; x < endX; x += chunkSize)
        {
            for (int y = startY; y < endY; y += chunkSize)
            {
                int chunkWidth = Mathf.Min(chunkSize, endX - x);
                int chunkHeight = Mathf.Min(chunkSize, endY - y);

                bool insidePlayableMap = x < width && x + chunkWidth > 0 && y < height && y + chunkHeight > 0;
                bool holeCutout = insidePlayableMap && DoesChunkOverlapSunlightHole(x, y, chunkWidth, chunkHeight);

                if (holeCutout || Random.value > coverage)
                {
                    continue;
                }

                Vector2Int centerCell = new Vector2Int(x + chunkWidth / 2, y + chunkHeight / 2);
                CreateSlabChunk(centerCell, chunkWidth, chunkHeight);
            }
        }
    }

    bool IsChunkCenterInsideFineCeilingArea(Vector2Int centerCell)
    {
        int transition = Mathf.Max(0, playableCeilingTransitionCells);

        return
            centerCell.x >= -transition &&
            centerCell.x < city.GetGridWidth() + transition &&
            centerCell.y >= -transition &&
            centerCell.y < city.GetGridHeight() + transition;
    }

    bool DoesChunkOverlapSunlightHole(int startX, int startY, int chunkWidth, int chunkHeight)
    {
        float chunkMinX = startX;
        float chunkMaxX = startX + chunkWidth;
        float chunkMinY = startY;
        float chunkMaxY = startY + chunkHeight;

        for (int i = 0; i < holeCenters.Count; i++)
        {
            Vector2Int hole = holeCenters[i];
            float closestX = Mathf.Clamp(hole.x, chunkMinX, chunkMaxX);
            float closestY = Mathf.Clamp(hole.y, chunkMinY, chunkMaxY);
            float distance = Vector2.Distance(new Vector2(hole.x, hole.y), new Vector2(closestX, closestY));

            if (distance <= holeRadii[i] + 0.5f)
            {
                return true;
            }
        }

        return false;
    }

    void CreateSlabChunk(Vector2Int centerCell, int chunkWidth, int chunkHeight)
    {
        float cellSize = city.GetCellSize();
        float layerHeight = Random.Range(minLayerHeight, maxLayerHeight);
        float thickness = Random.Range(minSlabThickness, maxSlabThickness);
        Vector3 position = city.CellToWorldPosition(centerCell, layerHeight);

        GameObject slab = GameObject.CreatePrimitive(PrimitiveType.Cube);
        slab.name = "Upper_Slab";
        slab.transform.SetParent(transform);
        slab.transform.position = position;
        slab.transform.localScale = new Vector3(
            Mathf.Max(cellSize, chunkWidth * cellSize - chunkGap),
            thickness,
            Mathf.Max(cellSize, chunkHeight * cellSize - chunkGap)
        );
        slab.isStatic = true;

        ConfigureCollider(slab, addSlabColliders);
        ApplyMaterial(slab, GetUpperLayerMaterial());
    }

    void GenerateUndersideRibs()
    {
        int width = city.GetGridWidth();
        int height = city.GetGridHeight();
        int startX = -overhangCells;
        int startY = -overhangCells;
        int endX = width + overhangCells;
        int endY = height + overhangCells;
        float cellSize = city.GetCellSize();
        float undersideY = minLayerHeight - maxSlabThickness * 0.5f - ribDrop;
        float totalWidth = (endX - startX) * cellSize;
        float totalDepth = (endY - startY) * cellSize;

        for (int x = startX; x < endX; x += ribSpacingInCells)
        {
            if (Random.value > ribCoverage) continue;

            Vector2Int centerCell = new Vector2Int(x, (startY + endY) / 2);
            Vector3 position = city.CellToWorldPosition(centerCell, undersideY);
            CreateUndersideRib("Upper_Underside_Rib_X", position, new Vector3(ribThickness, ribThickness, totalDepth));
        }

        for (int y = startY; y < endY; y += ribSpacingInCells)
        {
            if (Random.value > ribCoverage * 0.75f) continue;

            Vector2Int centerCell = new Vector2Int((startX + endX) / 2, y);
            Vector3 position = city.CellToWorldPosition(centerCell, undersideY - ribThickness * 0.8f);
            CreateUndersideRib("Upper_Underside_Rib_Z", position, new Vector3(totalWidth, ribThickness, ribThickness));
        }
    }

    void CreateUndersideRib(string objectName, Vector3 position, Vector3 scale)
    {
        GameObject rib = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rib.name = objectName;
        rib.transform.SetParent(transform);
        rib.transform.position = position;
        rib.transform.localScale = scale;
        rib.isStatic = true;

        ConfigureCollider(rib, false);
        ApplyMaterial(rib, GetUpperLayerMaterial());
    }

    void GenerateHangingMasses()
    {
        int startX = -overhangCells;
        int startY = -overhangCells;
        int endX = city.GetGridWidth() + overhangCells;
        int endY = city.GetGridHeight() + overhangCells;
        float cellSize = city.GetCellSize();

        for (int i = 0; i < hangingMassCount; i++)
        {
            Vector2Int cell = new Vector2Int(Random.Range(startX, endX), Random.Range(startY, endY));

            if (IsInsideSunlightHole(cell))
            {
                continue;
            }

            float width = Random.Range(2f, 6f) * cellSize;
            float depth = Random.Range(2f, 7f) * cellSize;
            float drop = Random.Range(minHangingMassDrop, maxHangingMassDrop);
            float y = Random.Range(minLayerHeight, maxLayerHeight) - maxSlabThickness * 0.5f - drop * 0.5f;
            Vector3 position = city.CellToWorldPosition(cell, y);

            GameObject mass = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mass.name = "Upper_Hanging_Mass";
            mass.transform.SetParent(transform);
            mass.transform.position = position;
            mass.transform.localScale = new Vector3(width, drop, depth);
            mass.isStatic = true;

            ConfigureCollider(mass, false);
            ApplyMaterial(mass, GetUpperLayerMaterial());
        }
    }

    void GenerateSupportPillars()
    {
        int mapArea = city.GetGridWidth() * city.GetGridHeight();
        int pillarCount = Mathf.Max(minimumPillarCount, Mathf.RoundToInt(mapArea * pillarsPerThousandCells / 1000f));
        List<Vector2Int> candidates = GetBuildingCellCandidates();

        if (candidates.Count == 0)
        {
            return;
        }

        for (int i = 0; i < pillarCount; i++)
        {
            Vector2Int cell = PickPillarCell(candidates);
            CreateSupportPillar(cell, i);
        }
    }

    void GenerateOuterSupportPillars()
    {
        int count = Mathf.Max(0, outerSupportPillarCount);

        for (int i = 0; i < count; i++)
        {
            Vector2Int cell = PickOuterSupportPillarCell();
            CreateSupportPillar(cell, i, "Outer_Support_Pillar");
        }
    }

    Vector2Int PickOuterSupportPillarCell()
    {
        int minOffset = Mathf.Max(1, outerSupportPillarMinOffsetCells);
        int maxOffset = Mathf.Max(minOffset + 1, outerSupportPillarMaxOffsetCells);
        int side = Random.Range(0, 4);
        int x;
        int y;

        if (side == 0)
        {
            x = Random.Range(-maxOffset, city.GetGridWidth() + maxOffset);
            y = -Random.Range(minOffset, maxOffset);
        }
        else if (side == 1)
        {
            x = Random.Range(-maxOffset, city.GetGridWidth() + maxOffset);
            y = city.GetGridHeight() + Random.Range(minOffset, maxOffset);
        }
        else if (side == 2)
        {
            x = -Random.Range(minOffset, maxOffset);
            y = Random.Range(-maxOffset, city.GetGridHeight() + maxOffset);
        }
        else
        {
            x = city.GetGridWidth() + Random.Range(minOffset, maxOffset);
            y = Random.Range(-maxOffset, city.GetGridHeight() + maxOffset);
        }

        return new Vector2Int(x, y);
    }

    Vector2Int PickPillarCell(List<Vector2Int> candidates)
    {
        for (int i = 0; i < 35; i++)
        {
            Vector2Int candidate = candidates[Random.Range(0, candidates.Count)];

            if (city.IsCellNearBase(candidate, 7f)) continue;
            if (city.IsCellPlaza(candidate.x, candidate.y)) continue;
            if (IsInsideSunlightHole(candidate)) continue;
            if (HasWalkableCellNearby(candidate, 2)) continue;

            return candidate;
        }

        return candidates[Random.Range(0, candidates.Count)];
    }

    bool HasWalkableCellNearby(Vector2Int cell, int radius)
    {
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                int checkX = cell.x + x;
                int checkY = cell.y + y;

                if (checkX < 0 || checkX >= city.GetGridWidth() || checkY < 0 || checkY >= city.GetGridHeight())
                {
                    continue;
                }

                if (city.IsCellWalkable(checkX, checkY))
                {
                    return true;
                }
            }
        }

        return false;
    }

    List<Vector2Int> GetBuildingCellCandidates()
    {
        List<Vector2Int> candidates = new List<Vector2Int>();
        int width = city.GetGridWidth();
        int height = city.GetGridHeight();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!city.IsCellWalkable(x, y))
                {
                    candidates.Add(new Vector2Int(x, y));
                }
            }
        }

        return candidates;
    }

    void CreateSupportPillar(Vector2Int cell, int index)
    {
        CreateSupportPillar(cell, index, "Support_Pillar");
    }

    void CreateSupportPillar(Vector2Int cell, int index, string objectNamePrefix)
    {
        float layerHeight = Random.Range(minLayerHeight, maxLayerHeight);
        float radius = Random.Range(minPillarRadius, maxPillarRadius);
        Vector3 position = city.CellToWorldPosition(cell, layerHeight * 0.5f);

        GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pillar.name = objectNamePrefix + "_" + index.ToString("00");
        pillar.transform.SetParent(transform);
        pillar.transform.position = position;
        pillar.transform.localScale = new Vector3(radius * 2f, layerHeight * 0.5f, radius * 2f);
        pillar.isStatic = true;

        ConfigureCollider(pillar, addPillarColliders);
        ApplyMaterial(pillar, GetPillarMaterial());

        CreatePillarCapital(cell, layerHeight, radius, index, objectNamePrefix);
    }

    void CreatePillarCapital(Vector2Int cell, float layerHeight, float pillarRadius, int index, string objectNamePrefix)
    {
        Vector3 topPosition = city.CellToWorldPosition(cell, layerHeight - 2f);
        float capitalWidth = pillarRadius * Random.Range(3.4f, 4.8f);

        GameObject capital = GameObject.CreatePrimitive(PrimitiveType.Cube);
        capital.name = objectNamePrefix + "_Capital_" + index.ToString("00");
        capital.transform.SetParent(transform);
        capital.transform.position = topPosition;
        capital.transform.localScale = new Vector3(capitalWidth, 4f, capitalWidth);
        capital.isStatic = true;

        ConfigureCollider(capital, false);
        ApplyMaterial(capital, GetPillarMaterial());
    }

    void GenerateFarCitySilhouette()
    {
        int halfWidth = city.GetGridWidth() / 2;
        int halfHeight = city.GetGridHeight() / 2;

        for (int i = 0; i < farSilhouetteCount; i++)
        {
            Vector2Int cell = PickFarSilhouetteCell(halfWidth, halfHeight);
            CreateFarSilhouetteBlock(cell, i);
        }
    }

    Vector2Int PickFarSilhouetteCell(int halfWidth, int halfHeight)
    {
        int side = Random.Range(0, 4);

        if (side == 0)
        {
            return new Vector2Int(Random.Range(-halfWidth - farSilhouetteDistanceCells, halfWidth + farSilhouetteDistanceCells), -farSilhouetteDistanceCells);
        }

        if (side == 1)
        {
            return new Vector2Int(Random.Range(-halfWidth - farSilhouetteDistanceCells, halfWidth + farSilhouetteDistanceCells), city.GetGridHeight() + farSilhouetteDistanceCells);
        }

        if (side == 2)
        {
            return new Vector2Int(-farSilhouetteDistanceCells, Random.Range(-halfHeight - farSilhouetteDistanceCells, halfHeight + farSilhouetteDistanceCells));
        }

        return new Vector2Int(city.GetGridWidth() + farSilhouetteDistanceCells, Random.Range(-halfHeight - farSilhouetteDistanceCells, halfHeight + farSilhouetteDistanceCells));
    }

    void CreateFarSilhouetteBlock(Vector2Int cell, int index)
    {
        float cellSize = city.GetCellSize();
        float height = Random.Range(minFarSilhouetteHeight, maxFarSilhouetteHeight);
        float width = Random.Range(3f, 9f) * cellSize;
        float depth = Random.Range(2f, 6f) * cellSize + farSilhouetteDepth;
        Vector3 position = city.CellToWorldPosition(cell, height * 0.5f);

        GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
        block.name = "FarCity_Silhouette_" + index.ToString("00");
        block.transform.SetParent(transform);
        block.transform.position = position;
        block.transform.localScale = new Vector3(width, height, depth);
        block.isStatic = true;

        ConfigureCollider(block, false);
        ApplyMaterial(block, GetFarSilhouetteMaterial());
    }

    void GenerateDistanceHaze()
    {
        float cellSize = city.GetCellSize();
        int width = city.GetGridWidth();
        int height = city.GetGridHeight();
        float worldWidth = (width + hazeWallDistanceCells * 2f) * cellSize;
        float worldDepth = (height + hazeWallDistanceCells * 2f) * cellSize;
        float y = hazeWallHeight * 0.5f;

        CreateHazeWall(
            "Distance_Haze_North",
            city.CellToWorldPosition(new Vector2Int(width / 2, height + hazeWallDistanceCells), y),
            new Vector3(worldWidth, hazeWallHeight, hazeWallThickness)
        );

        CreateHazeWall(
            "Distance_Haze_South",
            city.CellToWorldPosition(new Vector2Int(width / 2, -hazeWallDistanceCells), y),
            new Vector3(worldWidth, hazeWallHeight, hazeWallThickness)
        );

        CreateHazeWall(
            "Distance_Haze_East",
            city.CellToWorldPosition(new Vector2Int(width + hazeWallDistanceCells, height / 2), y),
            new Vector3(hazeWallThickness, hazeWallHeight, worldDepth)
        );

        CreateHazeWall(
            "Distance_Haze_West",
            city.CellToWorldPosition(new Vector2Int(-hazeWallDistanceCells, height / 2), y),
            new Vector3(hazeWallThickness, hazeWallHeight, worldDepth)
        );
    }

    void CreateHazeWall(string objectName, Vector3 position, Vector3 scale)
    {
        GameObject haze = GameObject.CreatePrimitive(PrimitiveType.Cube);
        haze.name = objectName;
        haze.transform.SetParent(transform);
        haze.transform.position = position;
        haze.transform.localScale = scale;
        haze.isStatic = true;

        ConfigureCollider(haze, false);
        ApplyMaterial(haze, GetDistanceHazeMaterial());
    }

    void GenerateSunlightShafts()
    {
        int shaftCount = createShaftForEveryHole
            ? holeCenters.Count
            : Mathf.Min(holeCenters.Count, maxShaftLights);

        for (int i = 0; i < shaftCount; i++)
        {
            CreateSunlightShaft(holeCenters[i], holeRadii[i], i);
        }
    }

    void CreateSunlightShaft(Vector2Int cell, int radiusCells, int index)
    {
        float cellSize = city.GetCellSize();
        float height = minLayerHeight;
        float radius = radiusCells * cellSize * 0.85f;
        Vector3 position = city.CellToWorldPosition(cell, height * 0.5f);

        GameObject shaft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        shaft.name = "Sunlight_Placeholder_Beam_" + index.ToString("00");
        shaft.transform.SetParent(transform);
        shaft.transform.position = position;
        shaft.transform.localScale = new Vector3(radius * 2f, height * 0.5f, radius * 2f);
        shaft.isStatic = true;

        ConfigureCollider(shaft, false);
        ApplyMaterial(shaft, GetSunlightShaftMaterial());

        CreateSunlightGroundPatch(cell, radius * 1.2f, index);

        GameObject lightObject = new GameObject("Sunlight_Shaft_Light_" + index.ToString("00"));
        lightObject.transform.SetParent(transform);
        lightObject.transform.position = city.CellToWorldPosition(cell, height - 2f);
        lightObject.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Spot;
        light.color = new Color(1f, 0.86f, 0.58f, 1f);
        light.intensity = 1.1f;
        light.range = height + 8f;
        light.spotAngle = Mathf.Clamp(radiusCells * 16f, 36f, 64f);
        light.shadows = LightShadows.None;
    }

    void CreateSunlightGroundPatch(Vector2Int cell, float radius, int index)
    {
        Vector3 position = city.CellToWorldPosition(cell, 0.035f);

        GameObject patch = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        patch.name = "Sunlight_Ground_Patch_" + index.ToString("00");
        patch.transform.SetParent(transform);
        patch.transform.position = position;
        patch.transform.localScale = new Vector3(radius * 2f, 0.02f, radius * 2f);
        patch.isStatic = true;

        ConfigureCollider(patch, false);
        ApplyMaterial(patch, GetSunlightGroundPatchMaterial());

        SunlightZone zone = patch.AddComponent<SunlightZone>();
        zone.cell = cell;
        zone.zoneRadius = radius;
        zone.revealRadius = Mathf.Max(radius * 1.5f, city.GetCellSize() * 4f);
        zone.displayName = index == 0 ? "Base Camp Sunlight" : "Sunlight Breach";
    }

    void ApplyAtmosphereSettings()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogDensity = fogDensity;
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = ambientColor;

        if (tuneDirectionalLight)
        {
            TuneDirectionalLights();
        }

        if (applyCameraBackgroundColor)
        {
            ApplyCameraBackgroundColor();
        }
    }

    void TuneDirectionalLights()
    {
        Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);

        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i] == null || lights[i].type != LightType.Directional)
            {
                continue;
            }

            lights[i].color = directionalLightColor;
            lights[i].intensity = directionalLightIntensity;
            lights[i].shadows = LightShadows.Soft;
            lights[i].transform.rotation = Quaternion.Euler(52f, -35f, 0f);
        }
    }

    void ApplyCameraBackgroundColor()
    {
        Camera mainCamera = Camera.main;

        if (mainCamera == null)
        {
            return;
        }

        mainCamera.clearFlags = CameraClearFlags.SolidColor;
        mainCamera.backgroundColor = cameraBackgroundColor;
    }

    bool IsInsideSunlightHole(Vector2Int cell)
    {
        for (int i = 0; i < holeCenters.Count; i++)
        {
            if (Vector2Int.Distance(cell, holeCenters[i]) <= holeRadii[i])
            {
                return true;
            }
        }

        return false;
    }

    void ConfigureCollider(GameObject obj, bool keepCollider)
    {
        Collider collider = obj.GetComponent<Collider>();

        if (collider == null || keepCollider)
        {
            return;
        }

        DestroyGeneratedObject(collider);
    }

    void ApplyMaterial(GameObject obj, Material material)
    {
        Renderer renderer = obj.GetComponent<Renderer>();

        if (renderer != null && material != null)
        {
            renderer.sharedMaterial = material;
        }
    }

    Material GetUpperLayerMaterial()
    {
        if (upperLayerMaterial != null) return upperLayerMaterial;
        if (generatedUpperLayerMaterial == null) generatedUpperLayerMaterial = CreateGeneratedMaterial("Generated_UpperLayer_Material", upperLayerColor, false);
        return generatedUpperLayerMaterial;
    }

    Material GetPillarMaterial()
    {
        if (pillarMaterial != null) return pillarMaterial;
        if (generatedPillarMaterial == null) generatedPillarMaterial = CreateGeneratedMaterial("Generated_UpperPillar_Material", pillarColor, false);
        return generatedPillarMaterial;
    }

    Material GetSunlightShaftMaterial()
    {
        if (sunlightShaftMaterial != null) return sunlightShaftMaterial;
        if (generatedSunlightShaftMaterial == null) generatedSunlightShaftMaterial = CreateGeneratedMaterial("Generated_SunlightShaft_Material", sunlightShaftColor, true);
        return generatedSunlightShaftMaterial;
    }

    Material GetFarSilhouetteMaterial()
    {
        if (farSilhouetteMaterial != null) return farSilhouetteMaterial;
        if (generatedFarSilhouetteMaterial == null) generatedFarSilhouetteMaterial = CreateGeneratedMaterial("Generated_FarCitySilhouette_Material", farSilhouetteColor, false);
        return generatedFarSilhouetteMaterial;
    }

    Material GetDistanceHazeMaterial()
    {
        if (distanceHazeMaterial != null) return distanceHazeMaterial;
        if (generatedDistanceHazeMaterial == null) generatedDistanceHazeMaterial = CreateGeneratedMaterial("Generated_DistanceHaze_Material", distanceHazeColor, true);
        return generatedDistanceHazeMaterial;
    }

    Material GetSunlightGroundPatchMaterial()
    {
        if (sunlightGroundPatchMaterial != null) return sunlightGroundPatchMaterial;
        if (generatedSunlightGroundPatchMaterial == null) generatedSunlightGroundPatchMaterial = CreateGeneratedMaterial("Generated_SunlightGroundPatch_Material", sunlightGroundPatchColor, true);
        return generatedSunlightGroundPatchMaterial;
    }

    Material CreateGeneratedMaterial(string materialName, Color color, bool transparent)
    {
        Shader shader = transparent
            ? Shader.Find("Universal Render Pipeline/Unlit")
            : Shader.Find("Universal Render Pipeline/Lit");

        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        Material material = new Material(shader);
        material.name = materialName;
        material.color = color;

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }

        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }

        if (transparent)
        {
            material.SetOverrideTag("RenderType", "Transparent");
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Mode", 3f);
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.SetFloat("_Cull", 0f);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.renderQueue = (int)RenderQueue.Transparent;
        }

        return material;
    }

    void DestroyGeneratedObject(Object obj)
    {
        if (obj == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(obj);
        }
        else
        {
            DestroyImmediate(obj);
        }
    }
}
