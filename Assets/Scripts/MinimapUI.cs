using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinimapUI : MonoBehaviour
{
    public static bool IsLargeMapOpen { get; private set; } = false;

    private Vector3 mapWorldOrigin = Vector3.zero;

    [Header("Map Zoom")]
    public float smallMapZoom = 1.75f;
    public float largeMapZoom = 1f;
    public float largeMapMinZoom = 0.75f;
    public float largeMapMaxZoom = 3f;
    public float zoomSpeed = 0.15f;

    private Vector2 largeMapPanOffset = Vector2.zero;
    private bool isDraggingMap = false;
    private Vector2 lastMousePosition;

    public GameObject mapPanel;
    public RectTransform mapPanelRect;
    public RectTransform mapContent;
    public RectTransform playerMarker;

    public InputSettings inputSettings;
    public Transform playerTransform;
    public PlayerCondition playerCondition;
    public UIStateManager uiStateManager;

    [Header("Map Settings")]
    public float worldSizeX = 165f;
    public float worldSizeZ = 120f;
    public float revealDistance = 8f;
    public float updateInterval = 0.15f;

    [Header("Perception")]
    public bool perceptionAffectsRevealDistance = true;
    public float minimumRevealDistance = 3f;
    public bool showGeneratedSignals = true;
    public bool showSignalsOnlyNearPlayer = true;
    public float signalDetectionDistance = 12f;

    [Header("Scan Pulse")]
    public bool scanEnabled = true;
    public float scanRadius = 24f;
    public float minimumScanRadius = 9f;
    public float scanCooldown = 4f;
    public bool showWorldScanPulse = true;
    public Color scanPulseColor = new Color(0.25f, 0.9f, 1f, 0.9f);
    public bool showMapScanPulse = true;
    public float mapScanPulseDuration = 0.85f;
    public Color mapScanPulseColor = new Color(0.25f, 0.9f, 1f, 0.8f);

    [Header("Minimap Layout")]
    public Vector2 smallSize = new Vector2(250f, 250f);
    public Vector2 smallPosition = new Vector2(-20f, -20f);
    public Vector2 largeSize = new Vector2(700f, 500f);
    public Vector2 largePosition = new Vector2(0f, 0f);

    [Header("Cells")]
    public GameObject exploredCellPrefab;
    public int gridWidth = 55;
    public int gridHeight = 40;

    [Header("Map Performance")]
    public bool revealOnlyGeneratedStarterArea = true;

    [Header("Cell Colors")]
    public Color walkableCellColor = new Color(0.7f, 0.7f, 0.7f, 0.75f);
    public Color buildingCellColor = new Color(0.15f, 0.18f, 0.25f, 0.9f);
    public Color outsideStarterAreaCellColor = new Color(0.025f, 0.03f, 0.045f, 0.2f);

    [Header("Debug Map Colors")]
    public bool useDebugMapColors = true;
    public Color mainStreetCellColor = new Color(0.95f, 0.85f, 0.45f, 0.9f);
    public Color sideStreetCellColor = new Color(0.75f, 0.75f, 0.75f, 0.85f);
    public Color alleyCellColor = new Color(0.45f, 0.45f, 0.45f, 0.85f);
    public Color plazaCellColor = new Color(0.65f, 0.45f, 0.9f, 0.9f);
    public Color resourceCellColor = new Color(0.2f, 0.9f, 0.25f, 0.95f);
    public Color hazardCellColor = new Color(1f, 0.15f, 0.15f, 0.95f);
    public Color infrastructureNodeCellColor = new Color(0.25f, 0.95f, 0.95f, 0.95f);
    public Color unresolvedInfrastructureHintColor = new Color(0.25f, 0.7f, 1f, 0.22f);
    public Color knownInfrastructureHintColor = new Color(0.25f, 0.95f, 0.95f, 0.95f);
    public Color restoredInfrastructureHintColor = new Color(0.42f, 1f, 0.62f, 0.9f);
    public Color restoredDistrictCellColor = new Color(0.33f, 0.9f, 0.68f, 0.82f);
    public Color signalRelayHintColor = new Color(0.2f, 0.92f, 0.92f, 0.98f);
    public Color powerJunctionHintColor = new Color(1f, 0.68f, 0.2f, 0.98f);
    public Color transitLiftHintColor = new Color(0.72f, 0.44f, 1f, 0.98f);
    public Color deepCitySignalHintColor = new Color(0.76f, 0.48f, 1f, 0.98f);
    public Color baseCampHintColor = new Color(0.34f, 0.9f, 0.54f, 0.98f);

    [Header("Building Detection")]
    public LayerMask buildingDetectionLayers;

    private bool[,] exploredCells;
    private bool[,] scannedSignalCells;
    private Dictionary<Vector2Int, GameObject> cellObjects = new Dictionary<Vector2Int, GameObject>();
    private Dictionary<InfrastructureNode, GameObject> infrastructureHintObjects = new Dictionary<InfrastructureNode, GameObject>();
    private GameObject deepCitySignalHintObject;
    private GameObject baseCampHintObject;
    private float timer = 0f;
    private float lastScanTime = -999f;
    private int lastScreenWidth = -1;
    private int lastScreenHeight = -1;
    private RectTransform mapScanPulseRect;
    private Image mapScanPulseImage;
    private Sprite mapScanPulseSprite;
    private Texture2D mapScanPulseTexture;
    private Vector3 mapScanPulseWorldOrigin;
    private float mapScanPulseWorldRadius;
    private float mapScanPulseStartTime = -999f;

    [Header("Follow Player")]
    public bool smallMapFollowsPlayer = true;

    public CityBlockoutGenerator cityGenerator;
    public GeneratedWorldSpawner worldSpawner;
    public InfrastructureNetworkManager infrastructureNetworkManager;

    void Start()
    {
        IsLargeMapOpen = false;

        if (playerCondition == null && playerTransform != null)
        {
            playerCondition = playerTransform.GetComponent<PlayerCondition>();
        }

        if (cityGenerator != null && cityGenerator.HasGeneratedMap())
        {
            gridWidth = cityGenerator.GetGridWidth();
            gridHeight = cityGenerator.GetGridHeight();
            worldSizeX = cityGenerator.GetWorldSizeX();
            worldSizeZ = cityGenerator.GetWorldSizeZ();
            mapWorldOrigin = cityGenerator.GetWorldOrigin();
        }
        exploredCells = new bool[gridWidth, gridHeight];
        scannedSignalCells = new bool[gridWidth, gridHeight];

        if (infrastructureNetworkManager == null)
        {
            infrastructureNetworkManager = FindFirstObjectByType<InfrastructureNetworkManager>();
        }

        SetSmallMap();
    }

    void Update()
    {
        HandleScreenResize();

        if (inputSettings != null && UIStateManager.CurrentState == UIState.Gameplay && Input.GetKeyDown(inputSettings.scanKey))
        {
            TryScanFromPlayer();
        }

        if (inputSettings != null && UIStateManager.CurrentState == UIState.Gameplay && Input.GetKeyDown(inputSettings.mapKey))
        {
            OpenLargeMap();
            UpdateMapScanPulse();
            return;
        }

        if (IsLargeMapOpen)
        {
            HandleLargeMapPanAndZoom();
        }

        if (IsLargeMapOpen && Input.GetKeyDown(inputSettings.mapKey))
        {
            CloseLargeMap();
            UpdateMapScanPulse();
            return;
        }

        timer += Time.deltaTime;

        if (timer >= updateInterval)
        {
            timer = 0f;
            RevealAroundPlayer();
            UpdatePlayerMarker();
        }

        UpdateMapScanPulse();
    }

    void HandleLargeMapPanAndZoom()
    {
        float scroll = Input.mouseScrollDelta.y;

        if (Mathf.Abs(scroll) > 0.01f)
        {
            largeMapZoom = Mathf.Clamp(
                largeMapZoom + scroll * zoomSpeed,
                largeMapMinZoom,
                largeMapMaxZoom
            );

            RelayoutAllRevealedCells();
            UpdatePlayerMarker();
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            largeMapZoom = Mathf.Clamp(largeMapZoom + zoomSpeed, largeMapMinZoom, largeMapMaxZoom);
            RelayoutAllRevealedCells();
            UpdatePlayerMarker();
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            largeMapZoom = Mathf.Clamp(largeMapZoom - zoomSpeed, largeMapMinZoom, largeMapMaxZoom);
            RelayoutAllRevealedCells();
            UpdatePlayerMarker();
        }

        if (Input.GetMouseButtonDown(0))
        {
            isDraggingMap = true;
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDraggingMap = false;
        }

        if (isDraggingMap)
        {
            Vector2 currentMousePosition = Input.mousePosition;
            Vector2 delta = currentMousePosition - lastMousePosition;
            lastMousePosition = currentMousePosition;

            largeMapPanOffset += delta;

            if (mapContent != null)
            {
                mapContent.anchoredPosition = largeMapPanOffset;
            }

            UpdatePlayerMarker();
        }
    }

    public void OpenLargeMap()
    {
        IsLargeMapOpen = true;
        SetLargeMap();

        if (uiStateManager != null)
        {
            uiStateManager.SetState(UIState.Map);
        }
    }

    public void CloseLargeMap()
    {
        IsLargeMapOpen = false;
        SetSmallMap();

        if (uiStateManager != null)
        {
            uiStateManager.ReturnToGameplay();
        }
    }

    void HandleScreenResize()
    {
        if (Screen.width == lastScreenWidth && Screen.height == lastScreenHeight)
        {
            return;
        }

        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;

        if (IsLargeMapOpen)
        {
            SetLargeMap();
        }
        else
        {
            SetSmallMap();
        }
    }

    public void ForceCloseMap()
    {
        if (!IsLargeMapOpen)
        {
            return;
        }

        IsLargeMapOpen = false;
        SetSmallMap();
    }

    void SetSmallMap()
    {
        if (mapPanelRect == null) return;

        mapPanelRect.anchorMin = new Vector2(1f, 1f);
        mapPanelRect.anchorMax = new Vector2(1f, 1f);
        mapPanelRect.pivot = new Vector2(1f, 1f);
        mapPanelRect.sizeDelta = GetResponsiveSmallMapSize();
        mapPanelRect.anchoredPosition = GetResponsiveSmallMapPosition();

        largeMapPanOffset = Vector2.zero;

        ForceRelayoutMapNow();
    }

    void SetLargeMap()
    {
        if (mapPanelRect == null) return;

        mapPanelRect.anchorMin = new Vector2(0.5f, 0.5f);
        mapPanelRect.anchorMax = new Vector2(0.5f, 0.5f);
        mapPanelRect.pivot = new Vector2(0.5f, 0.5f);
        mapPanelRect.sizeDelta = GetResponsiveLargeMapSize();
        mapPanelRect.anchoredPosition = largePosition;

        largeMapZoom = 1f;
        largeMapPanOffset = Vector2.zero;

        ForceRelayoutMapNow();
    }

    void ForceRelayoutMapNow()
    {
        Canvas.ForceUpdateCanvases();
        RelayoutAllRevealedCells();
        UpdateInfrastructureNodeHints();
        UpdatePlayerMarker();
    }

    void RelayoutAllRevealedCells()
    {
        foreach (KeyValuePair<Vector2Int, GameObject> pair in cellObjects)
        {
            if (pair.Value == null) continue;

            RectTransform rect = pair.Value.GetComponent<RectTransform>();

            if (rect != null)
            {
                rect.anchoredPosition = CellToUIPosition(pair.Key);
                rect.sizeDelta = GetUICellSize();
            }

            Image image = pair.Value.GetComponent<Image>();

            if (image != null)
            {
                image.color = GetCellColor(pair.Key);
            }
        }

        UpdateInfrastructureNodeHints();
        UpdateDeepCitySignalHint();
        UpdateBaseCampHint();
    }

    void RevealAroundPlayer()
    {
        if (playerTransform == null || exploredCells == null) return;

        Vector2Int playerCell = WorldToMapCell(playerTransform.position);
        int revealRadiusInCells = Mathf.CeilToInt(GetEffectiveRevealDistance() / GetCellWorldSize());
        bool revealedAnyCell = false;

        for (int x = -revealRadiusInCells; x <= revealRadiusInCells; x++)
        {
            for (int y = -revealRadiusInCells; y <= revealRadiusInCells; y++)
            {
                Vector2Int cell = new Vector2Int(playerCell.x + x, playerCell.y + y);

                if (!IsInsideMap(cell)) continue;

                float distance = Vector2Int.Distance(playerCell, cell);

                if (distance <= revealRadiusInCells)
                {
                    revealedAnyCell |= RevealCell(cell);
                }
            }
        }

        if (revealedAnyCell)
        {
            UpdateInfrastructureNodeHints();
            UpdateBaseCampHint();
        }
    }

    public bool TryScanFromPlayer()
    {
        if (!scanEnabled || playerTransform == null || scannedSignalCells == null)
        {
            return false;
        }

        if (Time.time - lastScanTime < scanCooldown)
        {
            return false;
        }

        lastScanTime = Time.time;
        float effectiveRadius = GetEffectiveScanRadius();
        ScanAroundWorldPosition(playerTransform.position, effectiveRadius);

        if (showWorldScanPulse)
        {
            ScanPulseEffect.Spawn(playerTransform.position, effectiveRadius, scanPulseColor);
        }

        if (showMapScanPulse)
        {
            BeginMapScanPulse(playerTransform.position, effectiveRadius);
        }

        return true;
    }

    public void ScanAroundWorldPosition(Vector3 worldPosition, float radius)
    {
        if (scannedSignalCells == null)
        {
            scannedSignalCells = new bool[gridWidth, gridHeight];
        }

        Vector2Int centerCell = WorldToMapCell(worldPosition);
        int radiusInCells = Mathf.CeilToInt(radius / GetCellWorldSize());

        for (int x = -radiusInCells; x <= radiusInCells; x++)
        {
            for (int y = -radiusInCells; y <= radiusInCells; y++)
            {
                Vector2Int cell = new Vector2Int(centerCell.x + x, centerCell.y + y);

                if (!IsInsideMap(cell)) continue;
                if (Vector2Int.Distance(centerCell, cell) > radiusInCells) continue;
                if (!HasGeneratedSignalAtCell(cell)) continue;

                scannedSignalCells[cell.x, cell.y] = true;
                RevealCell(cell);
            }
        }

        RelayoutAllRevealedCells();
        UpdateInfrastructureNodeHints();
        UpdatePlayerMarker();
    }

    bool RevealCell(Vector2Int cell)
    {
        if (!IsInsideMap(cell) || exploredCells[cell.x, cell.y])
        {
            return false;
        }

        exploredCells[cell.x, cell.y] = true;

        if (!ShouldCreateExploredCellObject(cell) || exploredCellPrefab == null || mapContent == null)
        {
            return false;
        }

        GameObject cellObject = Instantiate(exploredCellPrefab, mapContent);
        cellObjects[cell] = cellObject;

        RectTransform rect = cellObject.GetComponent<RectTransform>();

        if (rect != null)
        {
            rect.anchoredPosition = CellToUIPosition(cell);
            rect.sizeDelta = GetUICellSize();
        }

        Image image = cellObject.GetComponent<Image>();

        if (image != null)
        {
            image.color = GetCellColor(cell);
        }

        return true;
    }

    bool ShouldCreateExploredCellObject(Vector2Int cell)
    {
        if (!revealOnlyGeneratedStarterArea || cityGenerator == null || !cityGenerator.HasGeneratedMap())
        {
            return true;
        }

        return cityGenerator.IsCellInsideStarterArea(cell.x, cell.y);
    }

    void BeginMapScanPulse(Vector3 worldPosition, float radius)
    {
        EnsureMapScanPulseVisual();

        if (mapScanPulseRect == null || mapScanPulseImage == null)
        {
            return;
        }

        mapScanPulseWorldOrigin = worldPosition;
        mapScanPulseWorldRadius = radius;
        mapScanPulseStartTime = Time.time;
        mapScanPulseRect.gameObject.SetActive(true);
        UpdateMapScanPulse();
    }

    void EnsureMapScanPulseVisual()
    {
        if (mapScanPulseRect != null || mapContent == null)
        {
            return;
        }

        GameObject pulseObject = new GameObject("MapScanPulse", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        pulseObject.transform.SetParent(mapContent, false);
        mapScanPulseRect = pulseObject.GetComponent<RectTransform>();
        mapScanPulseRect.anchorMin = new Vector2(0.5f, 0.5f);
        mapScanPulseRect.anchorMax = new Vector2(0.5f, 0.5f);
        mapScanPulseRect.pivot = new Vector2(0.5f, 0.5f);

        mapScanPulseImage = pulseObject.GetComponent<Image>();
        mapScanPulseImage.raycastTarget = false;
        mapScanPulseImage.sprite = CreateMapScanPulseSprite();
        mapScanPulseImage.color = mapScanPulseColor;
        pulseObject.SetActive(false);
    }

    Sprite CreateMapScanPulseSprite()
    {
        const int textureSize = 64;
        mapScanPulseTexture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        mapScanPulseTexture.name = "Runtime_MapScanPulseRing";
        mapScanPulseTexture.wrapMode = TextureWrapMode.Clamp;
        mapScanPulseTexture.filterMode = FilterMode.Bilinear;

        Color[] pixels = new Color[textureSize * textureSize];
        Vector2 center = new Vector2((textureSize - 1) * 0.5f, (textureSize - 1) * 0.5f);
        float outerRadius = textureSize * 0.49f;
        float innerRadius = textureSize * 0.425f;

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float outerAlpha = Mathf.Clamp01(outerRadius - distance);
                float innerAlpha = Mathf.Clamp01(distance - innerRadius);
                float alpha = Mathf.Min(outerAlpha, innerAlpha);
                pixels[y * textureSize + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        mapScanPulseTexture.SetPixels(pixels);
        mapScanPulseTexture.Apply();
        mapScanPulseSprite = Sprite.Create(
            mapScanPulseTexture,
            new Rect(0f, 0f, textureSize, textureSize),
            new Vector2(0.5f, 0.5f),
            textureSize
        );
        mapScanPulseSprite.name = "Runtime_MapScanPulseRing";
        return mapScanPulseSprite;
    }

    void UpdateMapScanPulse()
    {
        if (mapScanPulseRect == null || !mapScanPulseRect.gameObject.activeSelf)
        {
            return;
        }

        float t = Mathf.Clamp01((Time.time - mapScanPulseStartTime) / mapScanPulseDuration);

        if (t >= 1f)
        {
            mapScanPulseRect.gameObject.SetActive(false);
            return;
        }

        float easedProgress = 1f - Mathf.Pow(1f - t, 2f);
        Vector2 fullSize = WorldRadiusToUISize(mapScanPulseWorldRadius);
        mapScanPulseRect.anchoredPosition = CellToUIPosition(WorldToMapCell(mapScanPulseWorldOrigin));
        mapScanPulseRect.sizeDelta = Vector2.Lerp(Vector2.one * 6f, fullSize, easedProgress);
        mapScanPulseRect.SetAsLastSibling();

        Color pulseColor = mapScanPulseColor;
        pulseColor.a *= 1f - t;
        mapScanPulseImage.color = pulseColor;
    }

    Vector2 WorldRadiusToUISize(float radius)
    {
        Vector2 drawSize = GetAspectCorrectMapDrawSize();
        float zoom = IsLargeMapOpen ? largeMapZoom : smallMapZoom;
        return new Vector2(
            radius * 2f / worldSizeX * drawSize.x * zoom,
            radius * 2f / worldSizeZ * drawSize.y * zoom
        );
    }

    Color GetCellColor(Vector2Int cell)
    {
        if (cityGenerator != null &&
            cityGenerator.HasGeneratedMap() &&
            !cityGenerator.IsCellInsideStarterArea(cell.x, cell.y))
        {
            return outsideStarterAreaCellColor;
        }

        bool signalsVisible = useDebugMapColors || ShouldShowGeneratedSignals(cell);

        if (showGeneratedSignals && worldSpawner != null && signalsVisible && worldSpawner.HasHazardAtCell(cell.x, cell.y))
        {
            return hazardCellColor;
        }

        if (showGeneratedSignals && worldSpawner != null && signalsVisible && worldSpawner.HasInfrastructureNodeAtCell(cell.x, cell.y))
        {
            return infrastructureNodeCellColor;
        }

        if (showGeneratedSignals && worldSpawner != null && signalsVisible && worldSpawner.HasResourceAtCell(cell.x, cell.y))
        {
            return resourceCellColor;
        }

        if (worldSpawner != null && worldSpawner.IsRestoredDistrictCell(cell.x, cell.y))
        {
            return restoredDistrictCellColor;
        }

        if (IsBuildingCell(cell))
        {
            return buildingCellColor;
        }

        if (!useDebugMapColors || cityGenerator == null || !cityGenerator.HasGeneratedMap())
        {
            return walkableCellColor;
        }

        if (cityGenerator.IsCellPlaza(cell.x, cell.y))
        {
            return plazaCellColor;
        }

        if (cityGenerator.IsCellMainStreet(cell.x, cell.y))
        {
            return mainStreetCellColor;
        }

        if (cityGenerator.IsCellSideStreet(cell.x, cell.y))
        {
            return sideStreetCellColor;
        }

        if (cityGenerator.IsCellAlley(cell.x, cell.y))
        {
            return alleyCellColor;
        }

        return walkableCellColor;
    }

    bool ShouldShowGeneratedSignals(Vector2Int cell)
    {
        if (!showGeneratedSignals || worldSpawner == null)
        {
            return false;
        }

        if (scannedSignalCells != null && scannedSignalCells[cell.x, cell.y])
        {
            return true;
        }

        if (!showSignalsOnlyNearPlayer || playerTransform == null)
        {
            return true;
        }

        float detectionDistance = GetEffectiveSignalDetectionDistance();
        float distance = Vector3.Distance(playerTransform.position, MapCellToWorld(cell));

        return distance <= detectionDistance;
    }

    void UpdateInfrastructureNodeHints()
    {
        if (mapContent == null)
        {
            return;
        }

        if (infrastructureNetworkManager == null)
        {
            infrastructureNetworkManager = FindFirstObjectByType<InfrastructureNetworkManager>();
        }

        if (infrastructureNetworkManager == null)
        {
            ClearInfrastructureHints();
            return;
        }

        List<InfrastructureNode> activeNodes = new List<InfrastructureNode>();

        for (int i = 0; i < infrastructureNetworkManager.GetNodeCount(); i++)
        {
            InfrastructureNode node = infrastructureNetworkManager.GetNode(i);

            if (node == null)
            {
                continue;
            }

            activeNodes.Add(node);
            GameObject hintObject = GetOrCreateInfrastructureHint(node);
            ApplyInfrastructureHintPresentation(node, hintObject);
        }

        RemoveStaleInfrastructureHints(activeNodes);
        UpdateDeepCitySignalHint();
    }

    public void RefreshNetworkMarkers()
    {
        RefreshRevealedCellColors();
        UpdateInfrastructureNodeHints();
        UpdateDeepCitySignalHint();
        UpdateBaseCampHint();
    }

    void RefreshRevealedCellColors()
    {
        foreach (KeyValuePair<Vector2Int, GameObject> pair in cellObjects)
        {
            if (pair.Value == null)
            {
                continue;
            }

            Image image = pair.Value.GetComponent<Image>();

            if (image != null)
            {
                image.color = GetCellColor(pair.Key);
            }
        }
    }

    void UpdateDeepCitySignalHint()
    {
        if (mapContent == null)
        {
            return;
        }

        if (infrastructureNetworkManager == null)
        {
            infrastructureNetworkManager = FindFirstObjectByType<InfrastructureNetworkManager>();
        }

        bool showMarker = infrastructureNetworkManager != null && infrastructureNetworkManager.HasDeepCitySignal();

        if (!showMarker)
        {
            if (deepCitySignalHintObject != null)
            {
                deepCitySignalHintObject.SetActive(false);
            }

            return;
        }

        if (deepCitySignalHintObject == null)
        {
            deepCitySignalHintObject = new GameObject("DeepCitySignalHint", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            deepCitySignalHintObject.transform.SetParent(mapContent, false);
        }

        deepCitySignalHintObject.SetActive(true);

        RectTransform rect = deepCitySignalHintObject.GetComponent<RectTransform>();
        Image image = deepCitySignalHintObject.GetComponent<Image>();
        Vector2 cellSize = GetUICellSize();

        if (rect != null)
        {
            rect.anchoredPosition = CellToUIPosition(WorldToMapCell(infrastructureNetworkManager.GetDeepCitySignalPosition()));
            rect.sizeDelta = new Vector2(Mathf.Max(cellSize.x * 2.6f, 16f), Mathf.Max(cellSize.y * 2.6f, 16f));
            rect.localRotation = Quaternion.Euler(0f, 0f, 45f);
            rect.SetAsLastSibling();
        }

        if (image != null)
        {
            image.color = deepCitySignalHintColor;
            image.raycastTarget = false;
        }
    }

    void UpdateBaseCampHint()
    {
        if (mapContent == null || cityGenerator == null || !cityGenerator.HasGeneratedMap())
        {
            if (baseCampHintObject != null)
            {
                baseCampHintObject.SetActive(false);
            }

            return;
        }

        if (baseCampHintObject == null)
        {
            baseCampHintObject = new GameObject("BaseCampWorkshopHint", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            baseCampHintObject.transform.SetParent(mapContent, false);

            GameObject labelObject = new GameObject("WorkshopGlyph", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            labelObject.transform.SetParent(baseCampHintObject.transform, false);
            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            Text label = labelObject.GetComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.text = "B";
            label.alignment = TextAnchor.MiddleCenter;
            label.fontStyle = FontStyle.Bold;
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 7;
            label.resizeTextMaxSize = 18;
            label.color = new Color(0.04f, 0.085f, 0.07f, 1f);
            label.raycastTarget = false;
        }

        baseCampHintObject.SetActive(true);
        RectTransform rect = baseCampHintObject.GetComponent<RectTransform>();
        Image image = baseCampHintObject.GetComponent<Image>();
        Vector2 cellSize = GetUICellSize();

        if (rect != null)
        {
            rect.anchoredPosition = CellToUIPosition(cityGenerator.GetBaseCell());
            rect.sizeDelta = new Vector2(Mathf.Max(cellSize.x * 2.3f, 13f), Mathf.Max(cellSize.y * 2.3f, 13f));
            rect.SetAsLastSibling();
        }

        if (image != null)
        {
            image.color = baseCampHintColor;
            image.raycastTarget = false;
        }
    }

    GameObject GetOrCreateInfrastructureHint(InfrastructureNode node)
    {
        if (infrastructureHintObjects.TryGetValue(node, out GameObject hintObject) && hintObject != null)
        {
            return hintObject;
        }

        hintObject = new GameObject("InfrastructureHint_" + node.GetDisplayName(), typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        hintObject.transform.SetParent(mapContent, false);

        GameObject labelObject = new GameObject("RelayType", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        labelObject.transform.SetParent(hintObject.transform, false);
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        Text label = labelObject.GetComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.alignment = TextAnchor.MiddleCenter;
        label.fontStyle = FontStyle.Bold;
        label.resizeTextForBestFit = true;
        label.resizeTextMinSize = 7;
        label.resizeTextMaxSize = 18;
        label.raycastTarget = false;

        infrastructureHintObjects[node] = hintObject;
        return hintObject;
    }

    void ApplyInfrastructureHintPresentation(InfrastructureNode node, GameObject hintObject)
    {
        if (node == null || hintObject == null)
        {
            return;
        }

        RectTransform rect = hintObject.GetComponent<RectTransform>();
        Image image = hintObject.GetComponent<Image>();
        Text label = hintObject.GetComponentInChildren<Text>();
        Vector2Int cell = node.cell;
        bool known = IsInfrastructureNodeKnown(cell) || node.restored;
        Vector2 cellSize = GetUICellSize();
        float sizeMultiplier = known ? 1.75f : 5.2f;

        if (rect != null)
        {
            rect.anchoredPosition = CellToUIPosition(cell);
            rect.sizeDelta = new Vector2(
                Mathf.Max(cellSize.x * sizeMultiplier, known ? 10f : 22f),
                Mathf.Max(cellSize.y * sizeMultiplier, known ? 10f : 22f)
            );
            rect.SetAsLastSibling();
        }

        if (image != null)
        {
            image.color = GetInfrastructureHintColor(node, known);
            image.raycastTarget = false;
        }

        if (label != null)
        {
            label.text = GetInfrastructureHintGlyph(node);
            label.color = known || node.restored
                ? new Color(0.04f, 0.075f, 0.09f, 1f)
                : new Color(0.85f, 0.95f, 1f, 0.8f);
        }
    }

    Color GetInfrastructureHintColor(InfrastructureNode node, bool known)
    {
        Color typeColor = signalRelayHintColor;

        if (node.nodeType == InfrastructureNodeType.PowerJunction)
        {
            typeColor = powerJunctionHintColor;
        }
        else if (node.nodeType == InfrastructureNodeType.TransitLift)
        {
            typeColor = transitLiftHintColor;
        }

        if (node.restored)
        {
            return Color.Lerp(typeColor, restoredInfrastructureHintColor, 0.42f);
        }

        if (!known)
        {
            typeColor.a = unresolvedInfrastructureHintColor.a;
        }

        return typeColor;
    }

    string GetInfrastructureHintGlyph(InfrastructureNode node)
    {
        switch (node.nodeType)
        {
            case InfrastructureNodeType.PowerJunction:
                return "P";

            case InfrastructureNodeType.TransitLift:
                return "T";

            default:
                return "S";
        }
    }

    bool IsInfrastructureNodeKnown(Vector2Int cell)
    {
        if (!IsInsideMap(cell))
        {
            return false;
        }

        if (exploredCells != null && exploredCells[cell.x, cell.y])
        {
            return true;
        }

        if (scannedSignalCells != null && scannedSignalCells[cell.x, cell.y])
        {
            return true;
        }

        if (playerTransform == null)
        {
            return false;
        }

        return Vector3.Distance(playerTransform.position, MapCellToWorld(cell)) <= GetEffectiveSignalDetectionDistance();
    }

    void RemoveStaleInfrastructureHints(List<InfrastructureNode> activeNodes)
    {
        List<InfrastructureNode> staleNodes = new List<InfrastructureNode>();

        foreach (KeyValuePair<InfrastructureNode, GameObject> pair in infrastructureHintObjects)
        {
            if (pair.Key == null || !activeNodes.Contains(pair.Key))
            {
                staleNodes.Add(pair.Key);
            }
        }

        for (int i = 0; i < staleNodes.Count; i++)
        {
            InfrastructureNode node = staleNodes[i];

            if (infrastructureHintObjects.TryGetValue(node, out GameObject hintObject) && hintObject != null)
            {
                Destroy(hintObject);
            }

            infrastructureHintObjects.Remove(node);
        }
    }

    void ClearInfrastructureHints()
    {
        foreach (KeyValuePair<InfrastructureNode, GameObject> pair in infrastructureHintObjects)
        {
            if (pair.Value != null)
            {
                Destroy(pair.Value);
            }
        }

        infrastructureHintObjects.Clear();
    }

    float GetEffectiveRevealDistance()
    {
        float networkMultiplier = GetNetworkRevealMultiplier();

        if (!perceptionAffectsRevealDistance || playerCondition == null)
        {
            return revealDistance * networkMultiplier;
        }

        float perceptionPercent = playerCondition.GetEffectivePerceptionPercent();
        return Mathf.Lerp(minimumRevealDistance, revealDistance, perceptionPercent) * networkMultiplier;
    }

    float GetEffectiveSignalDetectionDistance()
    {
        float networkMultiplier = GetNetworkSignalMultiplier();

        if (playerCondition == null)
        {
            return signalDetectionDistance * networkMultiplier;
        }

        float perceptionPercent = playerCondition.GetEffectivePerceptionPercent();
        return Mathf.Lerp(signalDetectionDistance * 0.45f, signalDetectionDistance, perceptionPercent) * networkMultiplier;
    }

    float GetEffectiveScanRadius()
    {
        float networkMultiplier = GetNetworkSignalMultiplier();

        if (playerCondition == null)
        {
            return scanRadius * networkMultiplier;
        }

        float perceptionPercent = playerCondition.GetEffectivePerceptionPercent();
        return Mathf.Lerp(minimumScanRadius, scanRadius, perceptionPercent) * networkMultiplier;
    }

    float GetNetworkRevealMultiplier()
    {
        if (infrastructureNetworkManager == null)
        {
            infrastructureNetworkManager = FindFirstObjectByType<InfrastructureNetworkManager>();
        }

        if (infrastructureNetworkManager == null)
        {
            return 1f;
        }

        return infrastructureNetworkManager.GetRevealRangeMultiplier();
    }

    float GetNetworkSignalMultiplier()
    {
        if (infrastructureNetworkManager == null)
        {
            infrastructureNetworkManager = FindFirstObjectByType<InfrastructureNetworkManager>();
        }

        if (infrastructureNetworkManager == null)
        {
            return 1f;
        }

        return infrastructureNetworkManager.GetSignalRangeMultiplier();
    }

    bool HasGeneratedSignalAtCell(Vector2Int cell)
    {
        if (worldSpawner == null)
        {
            return false;
        }

        return
            worldSpawner.HasHazardAtCell(cell.x, cell.y) ||
            worldSpawner.HasInfrastructureNodeAtCell(cell.x, cell.y) ||
            worldSpawner.HasResourceAtCell(cell.x, cell.y);
    }

    bool IsBuildingCell(Vector2Int cell)
    {
        if (cityGenerator != null && cityGenerator.HasGeneratedMap())
        {
            return cityGenerator.IsCellInsideStarterArea(cell.x, cell.y) &&
                   !cityGenerator.IsCellWalkable(cell.x, cell.y);
        }

        return false;
    }

    void UpdatePlayerMarker()
    {
        if (playerTransform == null || playerMarker == null) return;

        Vector2Int playerCell = WorldToMapCell(playerTransform.position);
        Vector2 playerMapPosition = CellToUIPosition(playerCell);

        if (IsLargeMapOpen)
        {
            if (mapContent != null)
            {
                mapContent.anchoredPosition = largeMapPanOffset;
            }

            playerMarker.anchoredPosition = playerMapPosition + largeMapPanOffset;
        }
        else
        {
            if (smallMapFollowsPlayer && mapContent != null)
            {
                mapContent.anchoredPosition = -playerMapPosition;
            }

            playerMarker.anchoredPosition = Vector2.zero;
        }

        UpdatePlayerMarkerRotation();
    }

    void UpdatePlayerMarkerRotation()
    {
        if (playerTransform == null || playerMarker == null) return;

        float yRotation = playerTransform.eulerAngles.y;
        playerMarker.localRotation = Quaternion.Euler(0f, 0f, -yRotation);
    }

    Vector2Int WorldToMapCell(Vector3 worldPosition)
    {
        Vector3 localPosition = worldPosition - mapWorldOrigin;

        float normalizedX = (localPosition.x + worldSizeX * 0.5f) / worldSizeX;
        float normalizedZ = (localPosition.z + worldSizeZ * 0.5f) / worldSizeZ;

        int x = Mathf.FloorToInt(normalizedX * gridWidth);
        int y = Mathf.FloorToInt(normalizedZ * gridHeight);

        return new Vector2Int(
            Mathf.Clamp(x, 0, gridWidth - 1),
            Mathf.Clamp(y, 0, gridHeight - 1)
        );
    }

    Vector3 MapCellToWorld(Vector2Int cell)
    {
        float x = ((cell.x + 0.5f) / gridWidth - 0.5f) * worldSizeX;
        float z = ((cell.y + 0.5f) / gridHeight - 0.5f) * worldSizeZ;

        return mapWorldOrigin + new Vector3(x, 0f, z);
    }

    Vector2 CellToUIPosition(Vector2Int cell)
    {
        if (mapContent == null) return Vector2.zero;

        Vector2 drawSize = GetAspectCorrectMapDrawSize();
        float zoom = IsLargeMapOpen ? largeMapZoom : smallMapZoom;

        float x = ((cell.x + 0.5f) / gridWidth - 0.5f) * drawSize.x * zoom;
        float y = ((cell.y + 0.5f) / gridHeight - 0.5f) * drawSize.y * zoom;

        return new Vector2(x, y);
    }

    Vector2 GetUICellSize()
    {
        if (mapContent == null) return Vector2.one * 5f;

        Vector2 drawSize = GetAspectCorrectMapDrawSize();
        float zoom = IsLargeMapOpen ? largeMapZoom : smallMapZoom;

        return new Vector2(
            drawSize.x / gridWidth * zoom,
            drawSize.y / gridHeight * zoom
        );
    }

    Vector2 GetResponsiveSmallMapSize()
    {
        float shortestSide = Mathf.Min(Screen.width, Screen.height);
        float size = Mathf.Clamp(shortestSide * 0.62f, 340f, 430f);
        return new Vector2(size, size);
    }

    Vector2 GetResponsiveSmallMapPosition()
    {
        float margin = Mathf.Clamp(Mathf.Min(Screen.width, Screen.height) * 0.022f, 12f, 24f);
        return new Vector2(-margin, -margin);
    }

    Vector2 GetResponsiveLargeMapSize()
    {
        return new Vector2(
            Mathf.Clamp(Screen.width * 0.72f, 520f, largeSize.x),
            Mathf.Clamp(Screen.height * 0.72f, 360f, largeSize.y)
        );
    }

    Vector2 GetAspectCorrectMapDrawSize()
    {
        if (mapContent == null)
        {
            return Vector2.zero;
        }

        float contentWidth = mapContent.rect.width;
        float contentHeight = mapContent.rect.height;

        if (contentWidth <= 0f || contentHeight <= 0f || worldSizeZ <= 0f)
        {
            return new Vector2(contentWidth, contentHeight);
        }

        float worldAspect = worldSizeX / worldSizeZ;
        float contentAspect = contentWidth / contentHeight;

        if (contentAspect > worldAspect)
        {
            float height = contentHeight;
            float width = height * worldAspect;
            return new Vector2(width, height);
        }
        else
        {
            float width = contentWidth;
            float height = width / worldAspect;
            return new Vector2(width, height);
        }
    }
    float GetCellWorldSize()
    {
        return Mathf.Max(worldSizeX / gridWidth, worldSizeZ / gridHeight);
    }

    bool IsInsideMap(Vector2Int cell)
    {
        return cell.x >= 0 && cell.x < gridWidth && cell.y >= 0 && cell.y < gridHeight;
    }

    public void RevealEntireMap()
    {
        if (exploredCells == null)
        {
            exploredCells = new bool[gridWidth, gridHeight];
        }

        bool revealedAnyCell = false;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                revealedAnyCell |= RevealCell(new Vector2Int(x, y));
            }
        }

        if (revealedAnyCell)
        {
            RelayoutAllRevealedCells();
        }
        else
        {
            UpdateInfrastructureNodeHints();
            UpdateDeepCitySignalHint();
            UpdateBaseCampHint();
        }

        UpdatePlayerMarker();
    }

    public void RevealAroundWorldPosition(Vector3 worldPosition, float radius)
    {
        if (exploredCells == null)
        {
            exploredCells = new bool[gridWidth, gridHeight];
        }

        Vector2Int centerCell = WorldToMapCell(worldPosition);
        int radiusInCells = Mathf.CeilToInt(radius / GetCellWorldSize());
        bool revealedAnyCell = false;

        for (int x = -radiusInCells; x <= radiusInCells; x++)
        {
            for (int y = -radiusInCells; y <= radiusInCells; y++)
            {
                Vector2Int cell = new Vector2Int(centerCell.x + x, centerCell.y + y);

                if (!IsInsideMap(cell)) continue;

                if (Vector2Int.Distance(centerCell, cell) <= radiusInCells)
                {
                    revealedAnyCell |= RevealCell(cell);
                }
            }
        }

        if (revealedAnyCell)
        {
            RelayoutAllRevealedCells();
        }

        UpdatePlayerMarker();
    }

    public void SetDebugMapColors(bool enabled)
    {
        useDebugMapColors = enabled;

        if (enabled)
        {
            RevealEntireMap();
        }

        RelayoutAllRevealedCells();
    }

    void OnDestroy()
    {
        if (mapScanPulseSprite != null)
        {
            Destroy(mapScanPulseSprite);
        }

        if (mapScanPulseTexture != null)
        {
            Destroy(mapScanPulseTexture);
        }
    }
}
