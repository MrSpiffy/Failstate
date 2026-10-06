using UnityEngine;

public class InfrastructureNode : MonoBehaviour
{
    public InfrastructureNodeType nodeType = InfrastructureNodeType.SignalRelay;
    public Vector2Int cell;
    public bool requiredChainLandmark = false;

    [Header("State")]
    public bool restored = false;
    public float interactionDistance = 3.5f;

    [Header("Repair Cost")]
    public ItemCost[] repairCosts;

    [Header("Effects")]
    public float localRevealRadius = 24f;
    public float localStabilityRadius = 14f;

    [Header("Presentation")]
    public bool tintByState = true;
    public Color damagedColor = new Color(0.2f, 0.45f, 0.55f, 1f);
    public Color restoredColor = new Color(0.45f, 0.95f, 0.75f, 1f);

    private Renderer nodeRenderer;
    private MaterialPropertyBlock propertyBlock;
    private InfrastructureNetworkManager networkManager;
    private GameObject restoredRing;
    private GameObject effectHalo;
    private GameObject lockIndicator;
    private GameObject restoredRangeIndicator;
    private Material restoredRangeMaterial;

    void Awake()
    {
        nodeRenderer = GetComponent<Renderer>();
        networkManager = FindFirstObjectByType<InfrastructureNetworkManager>();
        ApplyPresentation();
    }

    void Start()
    {
        if (networkManager == null)
        {
            networkManager = FindFirstObjectByType<InfrastructureNetworkManager>();
        }

        if (networkManager != null)
        {
            networkManager.RegisterNode(this);
        }

        ApplyPresentation();
    }

    void OnDestroy()
    {
        if (networkManager != null)
        {
            networkManager.UnregisterNode(this);
        }

        if (restoredRangeMaterial != null)
        {
            Destroy(restoredRangeMaterial);
        }
    }

    void Update()
    {
        if (GetComponent<RelayRestorationController>() != null)
        {
            return;
        }

        GameReferences refs = GameReferences.Instance;

        if (refs == null || refs.playerTransform == null || refs.inputSettings == null)
        {
            return;
        }

        if (lockIndicator != null)
        {
            lockIndicator.SetActive(IsLockedBySequence());
        }

        float distance = Vector3.Distance(transform.position, refs.playerTransform.position);
        bool canInteract = distance <= interactionDistance && UIStateManager.CanInteract();

        if (!canInteract)
        {
            if (refs.interactionPromptUI != null)
            {
                refs.interactionPromptUI.HidePrompt(gameObject);
            }

            return;
        }

        if (refs.interactionPromptUI != null)
        {
            refs.interactionPromptUI.ShowPrompt(GetPromptText(refs.playerInventory, refs.inputSettings.interactKey), gameObject, 10);
        }

        if (Input.GetKeyDown(refs.inputSettings.interactKey))
        {
            TryRestore(refs);
        }
    }

    public void Configure(InfrastructureNodeType type, Vector2Int gridCell, ItemCost[] costs, bool isRequiredChainLandmark = false)
    {
        nodeType = type;
        cell = gridCell;
        repairCosts = costs;
        requiredChainLandmark = isRequiredChainLandmark;
        ConfigureNodeEffects();
        gameObject.name = requiredChainLandmark
            ? "LocalGridLandmark_" + nodeType
            : "InfrastructureNode_" + nodeType;
        BuildPlaceholderVisuals();
        ApplyPresentation();
    }

    public bool TryRestore(GameReferences refs)
    {
        if (restored)
        {
            return false;
        }

        if (networkManager == null)
        {
            networkManager = FindFirstObjectByType<InfrastructureNetworkManager>();
        }

        if (networkManager != null && !networkManager.CanRestoreNode(this))
        {
            string reason = networkManager.GetNodeLockReason(this);

            if (SystemMessageUI.Instance != null)
            {
                SystemMessageUI.Instance.ShowMessage("NETWORK HANDSHAKE REJECTED\n" + reason, 3.5f);
            }

            return false;
        }

        if (refs == null || refs.playerInventory == null)
        {
            return false;
        }

        if (!refs.playerInventory.SpendItems(repairCosts))
        {
            return false;
        }

        return CompleteRestoration(refs);
    }

    public bool DebugRestore(GameReferences refs)
    {
        if (restored)
        {
            return false;
        }

        return CompleteRestoration(refs);
    }

    public bool CompleteRestoration(GameReferences refs)
    {
        restored = true;
        ApplyRestoredEffects(refs);
        ApplyPresentation();

        if (refs != null && refs.interactionPromptUI != null)
        {
            refs.interactionPromptUI.HidePrompt(gameObject);
        }

        return true;
    }

    void ApplyRestoredEffects(GameReferences refs)
    {
        if (networkManager == null)
        {
            networkManager = FindFirstObjectByType<InfrastructureNetworkManager>();
        }

        if (networkManager != null)
        {
            networkManager.HandleNodeRestored(this);
        }
        else
        {
            Debug.Log(GetDisplayName() + " restored. " + GetSystemMessage());
        }

        if (refs != null && refs.minimapUI != null)
        {
            refs.minimapUI.RevealAroundWorldPosition(transform.position, localRevealRadius);

            if (nodeType == InfrastructureNodeType.SignalRelay)
            {
                refs.minimapUI.ScanAroundWorldPosition(transform.position, localRevealRadius * 2.25f);
            }
            else if (nodeType == InfrastructureNodeType.PowerJunction)
            {
                refs.minimapUI.RevealAroundWorldPosition(transform.position, localStabilityRadius * 1.6f);
            }
            else if (nodeType == InfrastructureNodeType.TransitLift)
            {
                refs.minimapUI.ScanAroundWorldPosition(transform.position, localRevealRadius * 1.7f);
            }
        }

        BuildRestoredEffectFeedback();
    }

    string GetPromptText(PlayerInventory inventory, KeyCode interactKey)
    {
        if (restored)
        {
            return GetDisplayName() + " restored - " + GetEffectSummary();
        }

        if (networkManager == null)
        {
            networkManager = FindFirstObjectByType<InfrastructureNetworkManager>();
        }

        if (networkManager != null && !networkManager.CanRestoreNode(this))
        {
            return networkManager.GetNodeLockReason(this);
        }

        string affordability = inventory != null && inventory.CanAfford(repairCosts)
            ? "repair"
            : "missing: " + GetCostText();

        return "Press " + interactKey + " to " + affordability + " " + GetDisplayName() + " - " + GetEffectSummary();
    }

    public string GetDisplayName()
    {
        switch (nodeType)
        {
            case InfrastructureNodeType.BaseCampRoot:
                return "Base Camp Root Relay";

            case InfrastructureNodeType.PowerJunction:
                return "Power Junction";

            case InfrastructureNodeType.TransitLift:
                return "Transit Lift";

            default:
                return "Signal Relay";
        }
    }

    public string GetSystemMessage()
    {
        switch (nodeType)
        {
            case InfrastructureNodeType.BaseCampRoot:
                return "Root relay accepted local identity token. Recovery graph is incomplete.";

            case InfrastructureNodeType.PowerJunction:
                return "Auxiliary grid handshake restored. Nearby hazards dim; local decay pressure falls.";

            case InfrastructureNodeType.TransitLift:
                return "Transit spine ping received. A deeper city signal is now visible beyond this district.";

            default:
                return "Local relay synchronized. Nearby resources, hazards, and dark relays are easier to read.";
        }
    }

    public string GetEffectSummary()
    {
        switch (nodeType)
        {
            case InfrastructureNodeType.BaseCampRoot:
                return "anchors base camp network functions";

            case InfrastructureNodeType.PowerJunction:
                return "creates a strong local stability pocket and suppresses nearby hazards";

            case InfrastructureNodeType.TransitLift:
                return "reduces global passive decay and exposes the deeper-city route signal";

            default:
                return "reveals local map data, resource traces, and relay signals";
        }
    }

    public string GetCostText()
    {
        if (repairCosts == null || repairCosts.Length == 0)
        {
            return "no resources";
        }

        string result = "";

        for (int i = 0; i < repairCosts.Length; i++)
        {
            result += repairCosts[i].amount + " " + ItemDatabase.GetDisplayName(repairCosts[i].itemType);

            if (i < repairCosts.Length - 1)
            {
                result += " + ";
            }
        }

        return result;
    }

    void ApplyPresentation()
    {
        if (nodeRenderer == null)
        {
            nodeRenderer = GetComponent<Renderer>();
        }

        if (!tintByState || nodeRenderer == null)
        {
            return;
        }

        ApplyRendererColor(nodeRenderer, restored ? restoredColor : damagedColor);

        if (restoredRing != null)
        {
            restoredRing.SetActive(restored);
        }

        if (effectHalo != null)
        {
            effectHalo.SetActive(restored);
        }

        if (lockIndicator != null)
        {
            lockIndicator.SetActive(IsLockedBySequence());
        }
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

    void BuildPlaceholderVisuals()
    {
        RemoveGeneratedVisualChildren();

        transform.localScale = GetNodeScale();
        damagedColor = GetDamagedColorForType();
        restoredColor = GetRestoredColorForType();

        CreateMarkerChild();
        CreateLabelChild();
        CreateRestoredRing();
        CreateEffectHalo();
        CreateLockIndicator();
    }

    void RemoveGeneratedVisualChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);

            if (!child.name.StartsWith("NodeVisual_"))
            {
                continue;
            }

            if (Application.isPlaying)
            {
                Destroy(child.gameObject);
            }
            else
            {
                DestroyImmediate(child.gameObject);
            }
        }
    }

    void CreateMarkerChild()
    {
        PrimitiveType markerShape = nodeType == InfrastructureNodeType.PowerJunction
            ? PrimitiveType.Cube
            : PrimitiveType.Sphere;

        GameObject marker = GameObject.CreatePrimitive(markerShape);
        marker.name = "NodeVisual_TypeMarker";
        marker.transform.SetParent(transform);
        marker.transform.localPosition = new Vector3(0f, 1.25f, 0f);
        marker.transform.localScale = nodeType == InfrastructureNodeType.TransitLift
            ? new Vector3(0.42f, 1.15f, 0.42f)
            : new Vector3(0.62f, 0.62f, 0.62f);

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
            markerRenderer.sharedMaterial = nodeRenderer != null ? nodeRenderer.sharedMaterial : markerRenderer.sharedMaterial;
            ApplyRendererColor(markerRenderer, GetRestoredColorForType());
        }
    }

    void CreateRestoredRing()
    {
        restoredRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        restoredRing.name = "NodeVisual_RestoredRing";
        restoredRing.transform.SetParent(transform);
        restoredRing.transform.localPosition = new Vector3(0f, -0.45f, 0f);
        restoredRing.transform.localScale = new Vector3(1.18f, 0.04f, 1.18f);

        Collider ringCollider = restoredRing.GetComponent<Collider>();

        if (ringCollider != null)
        {
            if (Application.isPlaying)
            {
                Destroy(ringCollider);
            }
            else
            {
                DestroyImmediate(ringCollider);
            }
        }

        Renderer ringRenderer = restoredRing.GetComponent<Renderer>();

        if (ringRenderer != null)
        {
            ringRenderer.sharedMaterial = nodeRenderer != null ? nodeRenderer.sharedMaterial : ringRenderer.sharedMaterial;
            ApplyRendererColor(ringRenderer, restoredColor);
        }

        restoredRing.SetActive(restored);
    }

    void CreateEffectHalo()
    {
        effectHalo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        effectHalo.name = "NodeVisual_EffectHalo";
        effectHalo.transform.SetParent(transform);
        effectHalo.transform.localPosition = new Vector3(0f, -0.48f, 0f);
        effectHalo.transform.localScale = GetEffectHaloScale();

        Collider haloCollider = effectHalo.GetComponent<Collider>();

        if (haloCollider != null)
        {
            if (Application.isPlaying)
            {
                Destroy(haloCollider);
            }
            else
            {
                DestroyImmediate(haloCollider);
            }
        }

        Renderer haloRenderer = effectHalo.GetComponent<Renderer>();

        if (haloRenderer != null)
        {
            haloRenderer.sharedMaterial = nodeRenderer != null ? nodeRenderer.sharedMaterial : haloRenderer.sharedMaterial;
            Color haloColor = restoredColor;
            haloColor.a = 0.35f;
            ApplyRendererColor(haloRenderer, haloColor);
        }

        effectHalo.SetActive(restored);
    }

    void CreateLockIndicator()
    {
        lockIndicator = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lockIndicator.name = "NodeVisual_LockedState";
        lockIndicator.transform.SetParent(transform);
        lockIndicator.transform.localPosition = new Vector3(0f, 2.7f, 0f);
        lockIndicator.transform.localScale = new Vector3(0.42f, 0.14f, 0.42f);

        Collider lockCollider = lockIndicator.GetComponent<Collider>();

        if (lockCollider != null)
        {
            if (Application.isPlaying)
            {
                Destroy(lockCollider);
            }
            else
            {
                DestroyImmediate(lockCollider);
            }
        }

        Renderer lockRenderer = lockIndicator.GetComponent<Renderer>();

        if (lockRenderer != null)
        {
            lockRenderer.sharedMaterial = nodeRenderer != null ? nodeRenderer.sharedMaterial : lockRenderer.sharedMaterial;
            ApplyRendererColor(lockRenderer, new Color(0.95f, 0.18f, 0.12f, 1f));
        }

        lockIndicator.SetActive(IsLockedBySequence());
    }

    void CreateLabelChild()
    {
        GameObject labelObject = new GameObject("NodeVisual_Label");
        labelObject.transform.SetParent(transform);
        labelObject.transform.localPosition = new Vector3(0f, 2.15f, 0f);

        TextMesh label = labelObject.AddComponent<TextMesh>();
        label.text = GetShortLabel();
        label.fontSize = 42;
        label.characterSize = 0.06f;
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.color = new Color(0.7f, 0.95f, 0.9f, 1f);
    }

    Vector3 GetNodeScale()
    {
        switch (nodeType)
        {
            case InfrastructureNodeType.PowerJunction:
                return new Vector3(1.75f, 1.2f, 1.75f);

            case InfrastructureNodeType.TransitLift:
                return new Vector3(1.25f, 2.15f, 1.25f);

            default:
                return new Vector3(1.35f, 1.55f, 1.35f);
        }
    }

    Vector3 GetEffectHaloScale()
    {
        switch (nodeType)
        {
            case InfrastructureNodeType.PowerJunction:
                return new Vector3(2.6f, 0.025f, 2.6f);

            case InfrastructureNodeType.TransitLift:
                return new Vector3(2.25f, 0.025f, 2.25f);

            default:
                return new Vector3(1.9f, 0.025f, 1.9f);
        }
    }

    void ConfigureNodeEffects()
    {
        switch (nodeType)
        {
            case InfrastructureNodeType.PowerJunction:
                localRevealRadius = 24f;
                localStabilityRadius = 22f;
                break;

            case InfrastructureNodeType.TransitLift:
                localRevealRadius = 34f;
                localStabilityRadius = 16f;
                break;

            default:
                localRevealRadius = 32f;
                localStabilityRadius = 14f;
                break;
        }
    }

    string GetShortLabel()
    {
        switch (nodeType)
        {
            case InfrastructureNodeType.PowerJunction:
                return "PWR";

            case InfrastructureNodeType.TransitLift:
                return "LIFT";

            default:
                return "SIG";
        }
    }

    Color GetDamagedColorForType()
    {
        switch (nodeType)
        {
            case InfrastructureNodeType.PowerJunction:
                return new Color(0.55f, 0.34f, 0.14f, 1f);

            case InfrastructureNodeType.TransitLift:
                return new Color(0.38f, 0.24f, 0.55f, 1f);

            default:
                return new Color(0.2f, 0.45f, 0.55f, 1f);
        }
    }

    Color GetRestoredColorForType()
    {
        switch (nodeType)
        {
            case InfrastructureNodeType.PowerJunction:
                return new Color(1f, 0.72f, 0.25f, 1f);

            case InfrastructureNodeType.TransitLift:
                return new Color(0.72f, 0.48f, 1f, 1f);

            default:
                return new Color(0.45f, 0.95f, 0.75f, 1f);
        }
    }

    bool IsLockedBySequence()
    {
        if (restored)
        {
            return false;
        }

        if (networkManager == null)
        {
            networkManager = FindFirstObjectByType<InfrastructureNetworkManager>();
        }

        return networkManager != null && !networkManager.CanRestoreNode(this);
    }

    void BuildRestoredEffectFeedback()
    {
        Color pulseColor = GetRestoredColorForType();
        float pulseRadius = localRevealRadius;

        if (nodeType == InfrastructureNodeType.PowerJunction)
        {
            pulseRadius = localStabilityRadius;
            CreateRestoredRangeIndicator(localStabilityRadius, pulseColor);
        }
        else if (nodeType == InfrastructureNodeType.TransitLift)
        {
            pulseRadius = localRevealRadius * 1.3f;
        }

        ScanPulseEffect.Spawn(transform.position, pulseRadius, pulseColor);
    }

    void CreateRestoredRangeIndicator(float radius, Color color)
    {
        if (restoredRangeIndicator != null)
        {
            Destroy(restoredRangeIndicator);
        }

        restoredRangeIndicator = new GameObject("NodeVisual_StabilityBoundary");
        restoredRangeIndicator.transform.position = transform.position + Vector3.up * 0.09f;
        restoredRangeIndicator.transform.SetParent(transform, true);

        LineRenderer line = restoredRangeIndicator.AddComponent<LineRenderer>();
        line.loop = true;
        line.useWorldSpace = true;
        line.positionCount = 72;
        line.startWidth = 0.09f;
        line.endWidth = 0.09f;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;

        Shader shader = Shader.Find("Sprites/Default");

        if (shader != null)
        {
            restoredRangeMaterial = new Material(shader);
            line.material = restoredRangeMaterial;
        }

        Color boundaryColor = color;
        boundaryColor.a = 0.65f;
        line.startColor = boundaryColor;
        line.endColor = boundaryColor;

        for (int i = 0; i < line.positionCount; i++)
        {
            float angle = Mathf.PI * 2f * i / line.positionCount;
            line.SetPosition(i, transform.position + new Vector3(Mathf.Cos(angle) * radius, 0.09f, Mathf.Sin(angle) * radius));
        }
    }

}
