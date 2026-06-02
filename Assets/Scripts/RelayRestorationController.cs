using UnityEngine;

public class RelayRestorationController : MonoBehaviour
{
    public InfrastructureNode node;
    public RelayRestorationState state = RelayRestorationState.Dormant;
    public float interactionDistance = 3.2f;

    public Color dormantColor = new Color(0.15f, 0.28f, 0.32f, 1f);
    public Color restoringColor = new Color(0.75f, 0.62f, 0.22f, 1f);
    public Color restoredColor = new Color(0.26f, 0.95f, 1f, 1f);
    public Color missingColor = new Color(0.85f, 0.18f, 0.12f, 1f);

    private bool diagnosticsRun = false;
    private int installStepIndex = 0;
    private int nodeActivationIndex = 0;
    private bool sharedNodeItemConsumed = false;
    private Transform[] installPanels = new Transform[0];
    private Transform[] activationNodes = new Transform[0];
    private bool[] activationNodeComplete = new bool[0];
    private Light restoredLight;
    private Transform rotatingRing;
    private Transform antenna;
    private float antennaExtension = 0f;

    void Awake()
    {
        if (node == null)
        {
            node = GetComponent<InfrastructureNode>();
        }

        BuildRepairVisuals();
        ApplyStateVisuals();
    }

    void Update()
    {
        AnimateRestoredVisuals();

        if (node == null)
        {
            return;
        }

        if (node.restored || state == RelayRestorationState.Restored)
        {
            state = RelayRestorationState.Restored;
            UpdateRestoredPrompt();
            return;
        }

        GameReferences refs = GameReferences.Instance;

        if (refs == null || refs.playerTransform == null || refs.inputSettings == null || refs.playerInventory == null)
        {
            return;
        }

        if (!CanUseRelay(refs, out string lockReason))
        {
            UpdateLockedPrompt(refs, lockReason);
            return;
        }

        if (!diagnosticsRun)
        {
            UpdateDiagnosticsInteraction(refs);
            return;
        }

        if (installStepIndex < GetInstallStepCount())
        {
            UpdateInstallInteraction(refs);
            return;
        }

        UpdateActivationInteraction(refs);
    }

    public void Configure(InfrastructureNode configuredNode)
    {
        node = configuredNode;
        BuildRepairVisuals();
        ApplyStateVisuals();
    }

    public string GetObjectiveText(Vector3 playerPosition, PlayerInventory inventory, int restoredCount, int chainLength)
    {
        if (node == null)
        {
            return "OBJECTIVE: inspect damaged relay";
        }

        int distance = Mathf.RoundToInt(Vector3.Distance(playerPosition, node.transform.position));

        if (!diagnosticsRun)
        {
            return
                "OBJECTIVE: diagnose " + node.GetDisplayName() + " (" + restoredCount + "/" + chainLength + ")\n" +
                distance + "m away - run diagnostics at the relay plaza.";
        }

        ItemCost[] installItems = GetInstallItems();

        for (int i = installStepIndex; i < installItems.Length; i++)
        {
            ItemType itemType = installItems[i].itemType;

            if (inventory == null || inventory.GetItemCount(itemType) < installItems[i].amount)
            {
                return
                    "OBJECTIVE: prepare " + node.GetDisplayName() + "\n" +
                    "Find or craft " + ItemDatabase.GetDisplayName(itemType) + ". " + GetPreparationHint();
            }
        }

        if (installStepIndex < installItems.Length)
        {
            return
                "OBJECTIVE: repair " + node.GetDisplayName() + "\n" +
                "Install " + ItemDatabase.GetDisplayName(installItems[installStepIndex].itemType) + " at the relay panel.";
        }

        return
            "OBJECTIVE: activate " + node.GetDisplayName() + "\n" +
            GetActivationObjectiveText();
    }

    void UpdateDiagnosticsInteraction(GameReferences refs)
    {
        bool inRange = IsInRange(refs.playerTransform.position, transform);

        if (inRange && UIStateManager.CanInteract())
        {
            refs.interactionPromptUI?.ShowPrompt(
                "Press " + refs.inputSettings.interactKey + " to run " + node.GetDisplayName() + " diagnostics",
                gameObject,
                22
            );

            if (Input.GetKeyDown(refs.inputSettings.interactKey))
            {
                diagnosticsRun = true;
                state = RelayRestorationState.Restoring;
                refs.interactionPromptUI?.HidePrompt(gameObject);
                ApplyStateVisuals();
                ShowStepMessage(GetDiagnosticsMessage(), 6f);
            }
        }
        else
        {
            refs.interactionPromptUI?.HidePrompt(gameObject);
        }
    }

    void UpdateInstallInteraction(GameReferences refs)
    {
        ItemCost[] installItems = GetInstallItems();

        if (installStepIndex < 0 || installStepIndex >= installItems.Length)
        {
            return;
        }

        Transform target = installStepIndex < installPanels.Length ? installPanels[installStepIndex] : transform;
        ItemCost requirement = installItems[installStepIndex];
        bool inRange = target != null && IsInRange(refs.playerTransform.position, target);

        if (inRange && UIStateManager.CanInteract())
        {
            refs.interactionPromptUI?.ShowPrompt(
                "Press " + refs.inputSettings.interactKey + " to install " + ItemDatabase.GetDisplayName(requirement.itemType),
                target.gameObject,
                23
            );

            if (Input.GetKeyDown(refs.inputSettings.interactKey))
            {
                if (!refs.playerInventory.TryRemoveItem(requirement.itemType, requirement.amount))
                {
                    ShowMissingItem(ItemDatabase.GetDisplayName(requirement.itemType));
                    FlashTarget(target, missingColor);
                    return;
                }

                ApplyRendererColor(target, restoredColor);
                installStepIndex++;
                refs.interactionPromptUI?.HidePrompt(target.gameObject);
                ShowStepMessage(GetInstallMessage(requirement.itemType), 4.5f);
            }
        }
        else if (target != null)
        {
            refs.interactionPromptUI?.HidePrompt(target.gameObject);
        }
    }

    void UpdateActivationInteraction(GameReferences refs)
    {
        if (activationNodes.Length == 0)
        {
            CompleteRelay(refs);
            return;
        }

        int targetIndex = GetTargetActivationNodeIndex(refs.playerTransform.position);

        if (targetIndex < 0)
        {
            refs.interactionPromptUI?.HidePrompt(gameObject);
            return;
        }

        Transform target = activationNodes[targetIndex];
        bool inRange = target != null && IsInRange(refs.playerTransform.position, target);

        if (inRange && UIStateManager.CanInteract())
        {
            refs.interactionPromptUI?.ShowPrompt(
                "Press " + refs.inputSettings.interactKey + " to " + GetActivationPrompt(targetIndex),
                target.gameObject,
                23
            );

            if (Input.GetKeyDown(refs.inputSettings.interactKey))
            {
                if (!TryConsumeSharedActivationItem(refs, target))
                {
                    return;
                }

                activationNodeComplete[targetIndex] = true;
                nodeActivationIndex = Mathf.Max(nodeActivationIndex, GetCompletedActivationNodeCount());
                ApplyRendererColor(target, restoredColor);
                ShowStepMessage(GetActivationMessage(targetIndex), 3.4f);

                if (GetCompletedActivationNodeCount() >= activationNodes.Length)
                {
                    CompleteRelay(refs);
                }
            }
        }
        else if (target != null)
        {
            refs.interactionPromptUI?.HidePrompt(target.gameObject);
        }
    }

    bool TryConsumeSharedActivationItem(GameReferences refs, Transform target)
    {
        ItemType? sharedItem = GetSharedActivationItem();

        if (!sharedItem.HasValue || sharedNodeItemConsumed)
        {
            return true;
        }

        if (!refs.playerInventory.TryRemoveItem(sharedItem.Value, 1))
        {
            ShowMissingItem(ItemDatabase.GetDisplayName(sharedItem.Value));
            FlashTarget(target, missingColor);
            return false;
        }

        sharedNodeItemConsumed = true;
        return true;
    }

    void CompleteRelay(GameReferences refs)
    {
        state = RelayRestorationState.Restored;
        refs.interactionPromptUI?.HideAllPrompts();

        if (node != null)
        {
            node.CompleteRestoration(refs);
        }

        ApplyStateVisuals();
        ShowStepMessage(GetCompletionMessage(), 6f);
    }

    void UpdateLockedPrompt(GameReferences refs, string lockReason)
    {
        bool inRange = IsInRange(refs.playerTransform.position, transform);

        if (inRange && UIStateManager.CanInteract())
        {
            refs.interactionPromptUI?.ShowPrompt(lockReason, gameObject, 22);
        }
        else
        {
            refs.interactionPromptUI?.HidePrompt(gameObject);
        }
    }

    void UpdateRestoredPrompt()
    {
        GameReferences refs = GameReferences.Instance;

        if (refs == null || refs.playerTransform == null || refs.interactionPromptUI == null)
        {
            return;
        }

        if (IsInRange(refs.playerTransform.position, transform) && UIStateManager.CanInteract())
        {
            refs.interactionPromptUI.ShowPrompt(node.GetDisplayName() + " restored - district systems online", gameObject, 10);
        }
        else
        {
            refs.interactionPromptUI.HidePrompt(gameObject);
        }
    }

    bool CanUseRelay(GameReferences refs, out string lockReason)
    {
        InfrastructureNetworkManager network = refs.infrastructureNetworkManager != null
            ? refs.infrastructureNetworkManager
            : FindFirstObjectByType<InfrastructureNetworkManager>();

        if (network != null && !network.CanRestoreNode(node))
        {
            lockReason = network.GetNodeLockReason(node);
            return false;
        }

        lockReason = "";
        return true;
    }

    int GetTargetActivationNodeIndex(Vector3 playerPosition)
    {
        if (UsesOrderedActivation())
        {
            if (nodeActivationIndex >= activationNodes.Length || activationNodeComplete[nodeActivationIndex])
            {
                return -1;
            }

            Transform target = activationNodes[nodeActivationIndex];
            return target != null && IsInRange(playerPosition, target) ? nodeActivationIndex : -1;
        }

        int nearestIndex = -1;
        float nearestDistance = float.MaxValue;

        for (int i = 0; i < activationNodes.Length; i++)
        {
            if (activationNodeComplete[i] || activationNodes[i] == null)
            {
                continue;
            }

            float distance = Vector3.Distance(playerPosition, activationNodes[i].position);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestIndex = i;
            }
        }

        return nearestDistance <= interactionDistance ? nearestIndex : -1;
    }

    int GetCompletedActivationNodeCount()
    {
        int count = 0;

        for (int i = 0; i < activationNodeComplete.Length; i++)
        {
            if (activationNodeComplete[i])
            {
                count++;
            }
        }

        return count;
    }

    bool IsInRange(Vector3 playerPosition, Transform target)
    {
        return target != null && Vector3.Distance(playerPosition, target.position) <= interactionDistance;
    }

    void BuildRepairVisuals()
    {
        RemoveRepairVisuals();

        int installCount = GetInstallStepCount();
        installPanels = new Transform[installCount];

        for (int i = 0; i < installCount; i++)
        {
            float offset = installCount == 1 ? 0f : Mathf.Lerp(-1.25f, 1.25f, i / Mathf.Max(1f, installCount - 1f));
            installPanels[i] = CreateRepairPrimitive(
                "RelayRepair_InstallPanel_" + i,
                PrimitiveType.Cube,
                new Vector3(offset, 0.35f, 1.35f),
                new Vector3(0.55f, 0.42f, 0.22f),
                dormantColor
            ).transform;
        }

        int activationCount = GetActivationNodeCount();
        activationNodes = new Transform[activationCount];
        activationNodeComplete = new bool[activationCount];

        for (int i = 0; i < activationCount; i++)
        {
            float t = activationCount == 1 ? 0.5f : i / Mathf.Max(1f, activationCount - 1f);
            float angle = Mathf.Lerp(215f, 325f, t) * Mathf.Deg2Rad;
            Vector3 localPosition = new Vector3(Mathf.Cos(angle) * 1.75f, 0.12f, Mathf.Sin(angle) * 1.75f);
            activationNodes[i] = CreateRepairPrimitive(
                "RelayRepair_ActivationNode_" + i,
                GetActivationNodeShape(),
                localPosition,
                GetActivationNodeScale(),
                dormantColor
            ).transform;
        }

        rotatingRing = CreateRepairPrimitive(
            "RelayRepair_RestoredRing",
            PrimitiveType.Cylinder,
            new Vector3(0f, 1.75f, 0f),
            GetRestoredRingScale(),
            dormantColor
        ).transform;

        antenna = CreateRepairPrimitive(
            "RelayRepair_Antenna",
            PrimitiveType.Cylinder,
            new Vector3(0f, 2.1f, 0f),
            new Vector3(0.12f, 0.2f, 0.12f),
            dormantColor
        ).transform;

        GameObject lightObject = new GameObject("RelayRepair_RestoredLight");
        lightObject.transform.SetParent(transform, false);
        lightObject.transform.localPosition = new Vector3(0f, 2.65f, 0f);
        restoredLight = lightObject.AddComponent<Light>();
        restoredLight.type = LightType.Point;
        restoredLight.color = GetRestoredColor();
        restoredLight.range = GetRestoredLightRange();
        restoredLight.intensity = 0f;
        restoredLight.enabled = false;
    }

    GameObject CreateRepairPrimitive(string objectName, PrimitiveType type, Vector3 localPosition, Vector3 localScale, Color color)
    {
        GameObject primitive = GameObject.CreatePrimitive(type);
        primitive.name = objectName;
        primitive.transform.SetParent(transform, false);
        primitive.transform.localPosition = localPosition;
        primitive.transform.localScale = localScale;

        Collider collider = primitive.GetComponent<Collider>();

        if (collider != null)
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

        ApplyRendererColor(primitive.transform, color);
        return primitive;
    }

    void RemoveRepairVisuals()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);

            if (!child.name.StartsWith("RelayRepair_"))
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

    void ApplyStateVisuals()
    {
        Color color = state == RelayRestorationState.Restored
            ? GetRestoredColor()
            : state == RelayRestorationState.Restoring ? restoringColor : dormantColor;

        for (int i = 0; i < installPanels.Length; i++)
        {
            ApplyRendererColor(installPanels[i], i < installStepIndex ? GetRestoredColor() : color);
        }

        for (int i = 0; i < activationNodes.Length; i++)
        {
            ApplyRendererColor(activationNodes[i], activationNodeComplete[i] ? GetRestoredColor() : color);
        }

        ApplyRendererColor(rotatingRing, color);
        ApplyRendererColor(antenna, color);

        if (restoredLight != null)
        {
            restoredLight.color = GetRestoredColor();
            restoredLight.enabled = state == RelayRestorationState.Restored;
            restoredLight.intensity = state == RelayRestorationState.Restored ? GetRestoredLightIntensity() : 0f;
        }
    }

    void AnimateRestoredVisuals()
    {
        if (state != RelayRestorationState.Restored)
        {
            return;
        }

        if (rotatingRing != null)
        {
            rotatingRing.Rotate(Vector3.up, GetRingRotationSpeed() * Time.deltaTime, Space.Self);
        }

        if (antenna != null)
        {
            antennaExtension = Mathf.MoveTowards(antennaExtension, 1f, Time.deltaTime * 0.75f);
            Vector3 scale = antenna.localScale;
            scale.y = Mathf.Lerp(0.2f, GetAntennaHeight(), antennaExtension);
            antenna.localScale = scale;
            antenna.localPosition = new Vector3(0f, 2.1f + scale.y * 0.5f, 0f);
        }
    }

    void ApplyRendererColor(Transform target, Color color)
    {
        if (target == null)
        {
            return;
        }

        Renderer targetRenderer = target.GetComponent<Renderer>();

        if (targetRenderer == null)
        {
            return;
        }

        MaterialPropertyBlock block = new MaterialPropertyBlock();
        targetRenderer.GetPropertyBlock(block);
        block.SetColor("_BaseColor", color);
        block.SetColor("_Color", color);
        targetRenderer.SetPropertyBlock(block);
    }

    void FlashTarget(Transform target, Color color)
    {
        ApplyRendererColor(target, color);
    }

    void ShowMissingItem(string itemName)
    {
        ShowStepMessage("MISSING COMPONENT\n" + itemName + " required.", 3f);
    }

    void ShowStepMessage(string message, float duration)
    {
        if (SystemMessageUI.Instance != null)
        {
            SystemMessageUI.Instance.ShowMessage(message, duration);
        }
    }

    ItemCost[] GetInstallItems()
    {
        switch (GetNodeType())
        {
            case InfrastructureNodeType.PowerJunction:
                return new ItemCost[]
                {
                    new ItemCost(ItemType.PowerRegulator, 1),
                    new ItemCost(ItemType.PowerCoupler, 1)
                };

            case InfrastructureNodeType.TransitLift:
                return new ItemCost[]
                {
                    new ItemCost(ItemType.TransitActuator, 1),
                    new ItemCost(ItemType.TransitCore, 1),
                    new ItemCost(ItemType.TransitControlModule, 1)
                };

            default:
                return new ItemCost[]
                {
                    new ItemCost(ItemType.SignalProcessor, 1),
                    new ItemCost(ItemType.StabilizerModule, 1)
                };
        }
    }

    ItemType? GetSharedActivationItem()
    {
        return GetNodeType() == InfrastructureNodeType.SignalRelay
            ? ItemType.ConduitComponents
            : null;
    }

    int GetInstallStepCount()
    {
        return GetInstallItems().Length;
    }

    int GetActivationNodeCount()
    {
        switch (GetNodeType())
        {
            case InfrastructureNodeType.TransitLift:
                return 4;

            default:
                return 3;
        }
    }

    bool UsesOrderedActivation()
    {
        return GetNodeType() == InfrastructureNodeType.PowerJunction;
    }

    PrimitiveType GetActivationNodeShape()
    {
        if (GetNodeType() == InfrastructureNodeType.PowerJunction)
        {
            return PrimitiveType.Cube;
        }

        return PrimitiveType.Cylinder;
    }

    Vector3 GetActivationNodeScale()
    {
        if (GetNodeType() == InfrastructureNodeType.TransitLift)
        {
            return new Vector3(0.34f, 0.14f, 0.34f);
        }

        return new Vector3(0.28f, 0.12f, 0.28f);
    }

    Vector3 GetRestoredRingScale()
    {
        if (GetNodeType() == InfrastructureNodeType.TransitLift)
        {
            return new Vector3(0.95f, 0.08f, 0.95f);
        }

        if (GetNodeType() == InfrastructureNodeType.PowerJunction)
        {
            return new Vector3(0.85f, 0.07f, 0.85f);
        }

        return new Vector3(0.7f, 0.06f, 0.7f);
    }

    Color GetRestoredColor()
    {
        switch (GetNodeType())
        {
            case InfrastructureNodeType.PowerJunction:
                return new Color(1f, 0.72f, 0.25f, 1f);

            case InfrastructureNodeType.TransitLift:
                return new Color(0.5f, 0.82f, 1f, 1f);

            default:
                return restoredColor;
        }
    }

    float GetRestoredLightRange()
    {
        switch (GetNodeType())
        {
            case InfrastructureNodeType.TransitLift:
                return 18f;

            case InfrastructureNodeType.PowerJunction:
                return 15f;

            default:
                return 12f;
        }
    }

    float GetRestoredLightIntensity()
    {
        switch (GetNodeType())
        {
            case InfrastructureNodeType.TransitLift:
                return 4f;

            case InfrastructureNodeType.PowerJunction:
                return 3.4f;

            default:
                return 2.8f;
        }
    }

    float GetRingRotationSpeed()
    {
        switch (GetNodeType())
        {
            case InfrastructureNodeType.TransitLift:
                return 42f;

            case InfrastructureNodeType.PowerJunction:
                return 32f;

            default:
                return 22f;
        }
    }

    float GetAntennaHeight()
    {
        switch (GetNodeType())
        {
            case InfrastructureNodeType.TransitLift:
                return 1.65f;

            case InfrastructureNodeType.PowerJunction:
                return 1.25f;

            default:
                return 0.95f;
        }
    }

    string GetPreparationHint()
    {
        switch (GetNodeType())
        {
            case InfrastructureNodeType.PowerJunction:
                return "Search Power-sector infrastructure caches and craft at the workshop.";

            case InfrastructureNodeType.TransitLift:
                return "Search Transit-sector landmarks and craft at the workshop.";

            default:
                return "Search Signal-sector resource opportunities and craft at the workshop.";
        }
    }

    string GetActivationObjectiveText()
    {
        switch (GetNodeType())
        {
            case InfrastructureNodeType.PowerJunction:
                return "Route power through relay conduits in order (" + GetCompletedActivationNodeCount() + "/" + activationNodes.Length + ").";

            case InfrastructureNodeType.TransitLift:
                return "Bring transit control stations online (" + GetCompletedActivationNodeCount() + "/" + activationNodes.Length + ").";

            default:
                return "Reconnect conduit nodes around the relay (" + GetCompletedActivationNodeCount() + "/" + activationNodes.Length + ").";
        }
    }

    string GetActivationPrompt(int targetIndex)
    {
        switch (GetNodeType())
        {
            case InfrastructureNodeType.PowerJunction:
                return "route power conduit " + (targetIndex + 1) + "/" + activationNodes.Length;

            case InfrastructureNodeType.TransitLift:
                return "restore transit control station " + (targetIndex + 1) + "/" + activationNodes.Length;

            default:
                return sharedNodeItemConsumed
                    ? "reconnect conduit node " + (GetCompletedActivationNodeCount() + 1) + "/" + activationNodes.Length
                    : "install Conduit Components";
        }
    }

    string GetDiagnosticsMessage()
    {
        switch (GetNodeType())
        {
            case InfrastructureNodeType.PowerJunction:
                return "POWER RELAY DIAGNOSTICS\nRegulator burned out. Coupler missing. Three conduits require sequenced routing.";

            case InfrastructureNodeType.TransitLift:
                return "TRANSIT RELAY DIAGNOSTICS\nFrame actuator offline. Transit core missing. Control module required for station sync.";

            default:
                return "SIGNAL RELAY DIAGNOSTICS\nSignal Processor missing. Conduit Array damaged. Stabilizer Module required.";
        }
    }

    string GetInstallMessage(ItemType itemType)
    {
        return ItemDatabase.GetDisplayName(itemType).ToUpperInvariant() + " INSTALLED\nRelay subsystem accepted.";
    }

    string GetActivationMessage(int targetIndex)
    {
        switch (GetNodeType())
        {
            case InfrastructureNodeType.PowerJunction:
                return "POWER CONDUIT ROUTED\nEnergy path " + (targetIndex + 1) + " stable.";

            case InfrastructureNodeType.TransitLift:
                return "TRANSIT STATION ONLINE\nControl station " + (targetIndex + 1) + " synchronized.";

            default:
                return "CONDUIT NODE RECONNECTED\nSignal path " + (targetIndex + 1) + " restored.";
        }
    }

    string GetCompletionMessage()
    {
        switch (GetNodeType())
        {
            case InfrastructureNodeType.PowerJunction:
                return "POWER RELAY RESTORED\nEnergy district reclaimed. Transit Relay signal strengthened.";

            case InfrastructureNodeType.TransitLift:
                return "TRANSIT RELAY RESTORED\nTransit district reclaimed. Perimeter gate recognizes local chain.";

            default:
                return "SIGNAL RELAY RESTORED\nSignal sector reclaimed. Power Relay location identified.";
        }
    }

    InfrastructureNodeType GetNodeType()
    {
        return node != null ? node.nodeType : InfrastructureNodeType.SignalRelay;
    }
}
