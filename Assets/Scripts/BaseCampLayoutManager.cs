using UnityEngine;

public class BaseCampLayoutManager : MonoBehaviour
{
    public CityBlockoutGenerator city;
    public BaseCampZone baseCampZone;
    public WorkbenchStation workbenchStation;
    public RechargeStation rechargeStation;

    [Header("Layout")]
    public Vector3 baseZoneSize = new Vector3(18f, 4f, 17f);
    public Vector3 workbenchOffset = new Vector3(-4.55f, 0.5f, 1.65f);
    public Vector3 rechargeOffset = new Vector3(0f, 0.5f, -0.35f);
    public bool positionPlayerAtWorkshop = true;
    public Vector3 playerSpawnOffset = new Vector3(0f, 1.05f, 4.25f);

    [Header("Workshop Blockout")]
    public float workshopWidth = 15.5f;
    public float workshopDepth = 13f;
    public float workshopWallHeight = 4.2f;
    public float workshopMinimumClearanceHeight = 5.25f;
    public bool workshopWallsBlockMovement = true;

    [Header("Cleanup")]
    public bool cleanupOldTestObjectsOnStart = true;

    void Start()
    {
        ApplyLayout();

        if (cleanupOldTestObjectsOnStart)
        {
            CleanupOldTestObjects();
        }
    }

    [ContextMenu("Apply Base Camp Layout")]
    public void ApplyLayout()
    {
        Vector3 center = GetBaseCampWorldPosition();
        Quaternion orientation = GetWorkshopOrientation();

        if (baseCampZone != null)
        {
            baseCampZone.transform.position = center + Vector3.up;
            baseCampZone.transform.localScale = baseZoneSize;
            baseCampZone.transform.rotation = orientation;
        }

        if (workbenchStation != null)
        {
            workbenchStation.transform.position = center + orientation * workbenchOffset;
            workbenchStation.transform.rotation = orientation;
        }

        if (rechargeStation != null)
        {
            rechargeStation.transform.position = center + orientation * rechargeOffset;
            rechargeStation.transform.rotation = orientation;
        }

        PlacePlayerAtWorkshop(center, orientation);
        BuildBaseCampVisuals(center, orientation);
    }

    [ContextMenu("Cleanup Old Test Objects")]
    public void CleanupOldTestObjects()
    {
        DeleteIfExists("CoreHazardZone");
        DeleteIfExists("MobilityHazardZone");
        DeleteIfExists("PerceptionHazardZone");
        DeleteIfExists("MetalScrapPickup");
        DeleteIfExists("WiringPickup");
        DeleteIfExists("CoreFragmentPickup");
    }

    Vector3 GetBaseCampWorldPosition()
    {
        if (city != null && city.HasGeneratedMap())
        {
            return city.CellToWorldPosition(city.GetBaseCell(), 0f);
        }

        GameObject marker = GameObject.Find("Generated_Base_Camp_Marker");

        if (marker != null)
        {
            return marker.transform.position;
        }

        return Vector3.zero;
    }

    Quaternion GetWorkshopOrientation()
    {
        if (city == null || !city.HasGeneratedMap())
        {
            return Quaternion.identity;
        }

        Vector3 approach = city.GetBaseCampApproachDirection();
        return approach.sqrMagnitude > 0.01f
            ? Quaternion.LookRotation(approach, Vector3.up)
            : Quaternion.identity;
    }

    void PlacePlayerAtWorkshop(Vector3 center, Quaternion orientation)
    {
        if (!positionPlayerAtWorkshop)
        {
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            return;
        }

        player.transform.position = center + orientation * playerSpawnOffset;
        player.transform.rotation = orientation;

        Rigidbody body = player.GetComponent<Rigidbody>();

        if (body != null && Application.isPlaying)
        {
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }
    }

    void DeleteIfExists(string objectName)
    {
        GameObject obj = GameObject.Find(objectName);

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

    void BuildBaseCampVisuals(Vector3 center, Quaternion orientation)
    {
        GameObject existing = GameObject.Find("BaseCampVisuals");

        if (existing != null)
        {
            if (Application.isPlaying)
            {
                Destroy(existing);
            }
            else
            {
                DestroyImmediate(existing);
            }
        }

        GameObject root = new GameObject("BaseCampVisuals");
        root.transform.position = center;
        root.transform.rotation = orientation;

        Color floorColor = new Color(0.12f, 0.17f, 0.17f, 1f);
        Color structureColor = new Color(0.17f, 0.21f, 0.21f, 1f);
        Color beamColor = new Color(0.25f, 0.32f, 0.31f, 1f);
        Color patchColor = new Color(0.29f, 0.25f, 0.21f, 1f);
        Color cableColor = new Color(0.08f, 0.11f, 0.12f, 1f);
        Color moistureColor = new Color(0.08f, 0.18f, 0.2f, 1f);
        Color plantColor = new Color(0.12f, 0.28f, 0.19f, 1f);
        float structureHeight = Mathf.Max(workshopWallHeight, workshopMinimumClearanceHeight);

        CreateVisualCube(root.transform, "BaseCampVisual_WorkshopDeck", new Vector3(0f, 0.035f, 0f), new Vector3(workshopWidth, 0.07f, workshopDepth), floorColor);
        CreateVisualCube(root.transform, "BaseCampVisual_RearBulkhead", new Vector3(0f, structureHeight * 0.5f, -workshopDepth * 0.5f), new Vector3(workshopWidth, structureHeight, 0.24f), structureColor, workshopWallsBlockMovement);
        CreateVisualCube(root.transform, "BaseCampVisual_LeftRearBulkhead", new Vector3(-workshopWidth * 0.5f, structureHeight * 0.5f, -4.35f), new Vector3(0.24f, structureHeight, 4.1f), structureColor, workshopWallsBlockMovement);
        CreateVisualCube(root.transform, "BaseCampVisual_LeftFrontBulkhead", new Vector3(-workshopWidth * 0.5f, structureHeight * 0.5f, 3.7f), new Vector3(0.24f, structureHeight, 3.15f), structureColor, workshopWallsBlockMovement);
        CreateVisualCube(root.transform, "BaseCampVisual_RightRearBulkhead", new Vector3(workshopWidth * 0.5f, structureHeight * 0.5f, -3.7f), new Vector3(0.24f, structureHeight, 5.35f), structureColor, workshopWallsBlockMovement);
        CreateVisualCube(root.transform, "BaseCampVisual_RightFrontBulkhead", new Vector3(workshopWidth * 0.5f, structureHeight * 0.5f, 4.15f), new Vector3(0.24f, structureHeight, 2.25f), structureColor, workshopWallsBlockMovement);

        CreateVisualCube(root.transform, "BaseCampVisual_FrontHeaderBeam", new Vector3(0f, structureHeight, workshopDepth * 0.5f - 0.35f), new Vector3(workshopWidth, 0.22f, 0.22f), beamColor);
        CreateVisualCube(root.transform, "BaseCampVisual_RearRoofPlate", new Vector3(0f, structureHeight + 0.04f, -4.55f), new Vector3(workshopWidth, 0.13f, 3.75f), structureColor);
        CreateVisualCube(root.transform, "BaseCampVisual_LeftRoofPlate", new Vector3(-5.8f, structureHeight + 0.04f, 0.05f), new Vector3(3.8f, 0.13f, 5.4f), patchColor);
        CreateVisualCube(root.transform, "BaseCampVisual_RightRoofPlate", new Vector3(5.45f, structureHeight + 0.04f, 0.05f), new Vector3(4.45f, 0.13f, 5.4f), structureColor);
        CreateVisualCube(root.transform, "BaseCampVisual_FrontCanopyLeft", new Vector3(-5.15f, structureHeight + 0.04f, 5.45f), new Vector3(4.85f, 0.13f, 1.55f), patchColor);
        CreateVisualCube(root.transform, "BaseCampVisual_FrontCanopyRight", new Vector3(5.15f, structureHeight + 0.04f, 5.45f), new Vector3(4.85f, 0.13f, 1.55f), patchColor);

        CreateVisualCube(root.transform, "BaseCampVisual_SolarApertureLeft", new Vector3(-2.45f, structureHeight + 0.14f, -0.35f), new Vector3(0.16f, 0.22f, 4.4f), beamColor);
        CreateVisualCube(root.transform, "BaseCampVisual_SolarApertureRight", new Vector3(2.45f, structureHeight + 0.14f, -0.35f), new Vector3(0.16f, 0.22f, 4.4f), beamColor);
        CreateVisualCube(root.transform, "BaseCampVisual_SolarApertureRear", new Vector3(0f, structureHeight + 0.14f, -2.48f), new Vector3(5.05f, 0.22f, 0.16f), beamColor);
        CreateVisualLabel(root.transform, "BaseCampVisual_SolarLabel", "SOLAR INTAKE", new Vector3(0f, structureHeight - 0.2f, -2.55f), new Color(0.85f, 0.78f, 0.52f, 1f));

        CreateVisualCube(root.transform, "BaseCampVisual_DrainageChannel", new Vector3(0f, 0.065f, 4.55f), new Vector3(1.15f, 0.04f, 3.8f), moistureColor);
        CreateVisualCube(root.transform, "BaseCampVisual_LeftServiceThreshold", new Vector3(-7.25f, 0.065f, 0.15f), new Vector3(1.35f, 0.05f, 1.8f), moistureColor);
        CreateVisualCube(root.transform, "BaseCampVisual_RightServiceThreshold", new Vector3(7.25f, 0.065f, 0.75f), new Vector3(1.35f, 0.05f, 1.65f), moistureColor);

        CreateVisualCube(root.transform, "BaseCampVisual_RootRelay", new Vector3(0f, 0.65f, -3.65f), new Vector3(1.1f, 1.3f, 1.1f), new Color(0.18f, 0.45f, 0.38f, 1f));
        GameObject rootConsole = CreateVisualCube(root.transform, "BaseCampVisual_RootConsole", new Vector3(3.85f, 0.62f, -5.95f), new Vector3(2.45f, 1.24f, 0.35f), new Color(0.14f, 0.29f, 0.25f, 1f));
        BaseCampTerminalStation terminalStation = rootConsole.AddComponent<BaseCampTerminalStation>();
        terminalStation.baseCampZone = baseCampZone;

        GameObject storageShelf = CreateVisualCube(root.transform, "BaseCampVisual_StorageShelf", new Vector3(5.9f, 1.1f, -2.6f), new Vector3(0.38f, 2.2f, 3.1f), beamColor);
        BaseCampStorageStation storageStation = storageShelf.AddComponent<BaseCampStorageStation>();
        storageStation.baseCampZone = baseCampZone;
        CreateVisualCube(root.transform, "BaseCampVisual_StorageA", new Vector3(5.35f, 0.38f, -3.3f), new Vector3(1.1f, 0.75f, 0.95f), new Color(0.32f, 0.38f, 0.34f, 1f));
        CreateVisualCube(root.transform, "BaseCampVisual_StorageB", new Vector3(5.3f, 0.38f, -2.1f), new Vector3(1.15f, 0.75f, 0.92f), new Color(0.28f, 0.34f, 0.31f, 1f));

        CreateVisualCube(root.transform, "BaseCampVisual_WorkbenchFrame", workbenchOffset + new Vector3(0f, 0.48f, 0f), new Vector3(2.4f, 0.9f, 1.15f), new Color(0.22f, 0.2f, 0.17f, 1f));
        CreateVisualCube(root.transform, "BaseCampVisual_WorkbenchMarker", workbenchOffset + new Vector3(0f, 1.02f, 0f), new Vector3(2.45f, 0.12f, 1.2f), new Color(0.38f, 0.28f, 0.18f, 1f));

        CreateVisualCube(root.transform, "BaseCampVisual_RechargePlinth", rechargeOffset + new Vector3(0f, 0.12f, 0f), new Vector3(2.2f, 0.24f, 2.2f), new Color(0.12f, 0.23f, 0.3f, 1f));
        CreateVisualCube(root.transform, "BaseCampVisual_RechargeMarker", rechargeOffset + new Vector3(0f, 0.3f, 0f), new Vector3(1.45f, 0.08f, 1.45f), new Color(0.18f, 0.45f, 0.62f, 1f));

        CreateVisualCube(root.transform, "BaseCampVisual_FrontEntryPostLeft", new Vector3(-6.5f, structureHeight * 0.5f, 6.1f), new Vector3(0.26f, structureHeight, 0.26f), beamColor, workshopWallsBlockMovement);
        CreateVisualCube(root.transform, "BaseCampVisual_FrontEntryPostRight", new Vector3(6.5f, structureHeight * 0.5f, 6.1f), new Vector3(0.26f, structureHeight, 0.26f), beamColor, workshopWallsBlockMovement);

        CreateVisualCube(root.transform, "BaseCampVisual_PatchedPanelLeft", new Vector3(-7.63f, 2.55f, -4.15f), new Vector3(0.03f, 1.55f, 1.75f), patchColor);
        CreateVisualCube(root.transform, "BaseCampVisual_PatchedPanelRear", new Vector3(-4.65f, 2.15f, -6.63f), new Vector3(2.1f, 1.6f, 0.03f), patchColor);
        CreateVisualCube(root.transform, "BaseCampVisual_ReinforcementBraceLeft", new Vector3(-7.42f, 1.5f, 3.2f), new Vector3(0.12f, 2.8f, 0.12f), beamColor);
        CreateVisualCube(root.transform, "BaseCampVisual_ReinforcementBraceRight", new Vector3(7.42f, 1.55f, -3.55f), new Vector3(0.12f, 2.9f, 0.12f), beamColor);
        CreateVisualCube(root.transform, "BaseCampVisual_CableRunRear", new Vector3(-1.95f, 3.55f, -6.28f), new Vector3(7.8f, 0.07f, 0.07f), cableColor);
        CreateVisualCube(root.transform, "BaseCampVisual_CableDrop", new Vector3(-4.9f, 2.25f, -6.27f), new Vector3(0.07f, 2.55f, 0.07f), cableColor);
        CreateVisualCube(root.transform, "BaseCampVisual_MossStrip", new Vector3(2.95f, 0.075f, 2.25f), new Vector3(2.4f, 0.03f, 0.42f), plantColor);
        CreateVisualCube(root.transform, "BaseCampVisual_MossStripDrain", new Vector3(-0.7f, 0.076f, 5.45f), new Vector3(1.8f, 0.03f, 0.24f), plantColor);
    }

    GameObject CreateVisualCube(Transform parent, string name, Vector3 localPosition, Vector3 scale, Color color, bool keepCollider = false)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = name;
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = localPosition;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = scale;
        obj.isStatic = true;

        Collider collider = obj.GetComponent<Collider>();

        if (collider != null && !keepCollider)
        {
            if (Application.isPlaying)
            {
                Destroy(collider);
            }
            else
            {
                DestroyImmediate(collider);
            }
        }

        Renderer renderer = obj.GetComponent<Renderer>();

        if (renderer != null)
        {
            ApplyRendererColor(renderer, color);
        }

        return obj;
    }

    void CreateVisualLabel(Transform parent, string name, string text, Vector3 localPosition, Color color)
    {
        GameObject labelObject = new GameObject(name);
        labelObject.transform.SetParent(parent, false);
        labelObject.transform.localPosition = localPosition;
        labelObject.transform.localRotation = Quaternion.identity;

        TextMesh label = labelObject.AddComponent<TextMesh>();
        label.text = text;
        label.fontSize = 42;
        label.characterSize = 0.055f;
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.color = color;
    }

    void ApplyRendererColor(Renderer renderer, Color color)
    {
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(block);
        block.SetColor("_BaseColor", color);
        block.SetColor("_Color", color);
        renderer.SetPropertyBlock(block);
    }
}
