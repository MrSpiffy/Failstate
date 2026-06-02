using UnityEngine;

public class SignalRelayRestorationController : MonoBehaviour
{
    public InfrastructureNode node;
    public float interactionDistance = 3.2f;
    public Color inactivePanelColor = new Color(0.16f, 0.34f, 0.38f, 1f);
    public Color activePanelColor = new Color(0.26f, 0.95f, 1f, 1f);
    public Color missingColor = new Color(0.85f, 0.18f, 0.12f, 1f);

    private bool diagnosticsRun = false;
    private bool signalProcessorInstalled = false;
    private bool stabilizerInstalled = false;
    private bool conduitComponentsInstalled = false;
    private readonly bool[] conduitNodes = new bool[3];
    private Transform processorPanel;
    private Transform stabilizerPanel;
    private readonly Transform[] conduitNodeTransforms = new Transform[3];
    private Light restoredLight;

    void Awake()
    {
        if (node == null)
        {
            node = GetComponent<InfrastructureNode>();
        }

        BuildRepairVisuals();
    }

    void Update()
    {
        if (node == null || node.restored)
        {
            UpdateRestoredPrompt();
            return;
        }

        GameReferences refs = GameReferences.Instance;

        if (refs == null || refs.playerTransform == null || refs.inputSettings == null || refs.playerInventory == null)
        {
            return;
        }

        if (!CanUseSignalRelay(refs, out string lockReason))
        {
            UpdateLockedPrompt(refs, lockReason);
            return;
        }

        if (!diagnosticsRun)
        {
            UpdateDiagnosticsInteraction(refs);
            return;
        }

        if (!signalProcessorInstalled)
        {
            UpdateItemInstallInteraction(
                refs,
                processorPanel,
                ItemType.SignalProcessor,
                "Signal Processor",
                "Install Signal Processor",
                OnSignalProcessorInstalled
            );
            return;
        }

        if (!stabilizerInstalled)
        {
            UpdateItemInstallInteraction(
                refs,
                stabilizerPanel,
                ItemType.StabilizerModule,
                "Stabilizer Module",
                "Install Stabilizer Module",
                OnStabilizerInstalled
            );
            return;
        }

        UpdateConduitInteraction(refs);
    }

    public void Configure(InfrastructureNode configuredNode)
    {
        node = configuredNode;
        BuildRepairVisuals();
    }

    void UpdateDiagnosticsInteraction(GameReferences refs)
    {
        bool inRange = IsInRange(refs.playerTransform.position, transform);

        if (inRange && UIStateManager.CanInteract())
        {
            refs.interactionPromptUI?.ShowPrompt(
                "Press " + refs.inputSettings.interactKey + " to run Signal Relay diagnostics",
                gameObject,
                22
            );

            if (Input.GetKeyDown(refs.inputSettings.interactKey))
            {
                diagnosticsRun = true;
                refs.interactionPromptUI?.HidePrompt(gameObject);

                FirstRunObjectiveManager objectiveManager = GetObjectiveManager(refs);
                objectiveManager?.NotifySignalRelayDiagnosed();

                if (SystemMessageUI.Instance != null)
                {
                    SystemMessageUI.Instance.ShowMessage(
                        "SIGNAL RELAY DIAGNOSTICS\nSignal Processor missing. Conduit Array damaged. Stabilizer Module required.",
                        6f
                    );
                }
            }
        }
        else
        {
            refs.interactionPromptUI?.HidePrompt(gameObject);
        }
    }

    void UpdateItemInstallInteraction(
        GameReferences refs,
        Transform target,
        ItemType requiredItem,
        string itemName,
        string promptAction,
        System.Action onInstalled)
    {
        bool inRange = target != null && IsInRange(refs.playerTransform.position, target);

        if (inRange && UIStateManager.CanInteract())
        {
            refs.interactionPromptUI?.ShowPrompt(
                "Press " + refs.inputSettings.interactKey + " to " + promptAction,
                target.gameObject,
                23
            );

            if (Input.GetKeyDown(refs.inputSettings.interactKey))
            {
                if (!refs.playerInventory.TryRemoveItem(requiredItem, 1))
                {
                    ShowMissingItem(itemName);
                    FlashTarget(target, missingColor);
                    return;
                }

                onInstalled?.Invoke();
                refs.interactionPromptUI?.HidePrompt(target.gameObject);
            }
        }
        else if (target != null)
        {
            refs.interactionPromptUI?.HidePrompt(target.gameObject);
        }
    }

    void UpdateConduitInteraction(GameReferences refs)
    {
        int nearestIndex = GetNearestInactiveConduitNode(refs.playerTransform.position);

        if (nearestIndex < 0)
        {
            refs.interactionPromptUI?.HidePrompt(gameObject);
            return;
        }

        Transform target = conduitNodeTransforms[nearestIndex];
        bool inRange = target != null && IsInRange(refs.playerTransform.position, target);

        if (inRange && UIStateManager.CanInteract())
        {
            string requirementText = conduitComponentsInstalled
                ? "reconnect conduit node " + (GetActiveConduitCount() + 1) + "/3"
                : "install Conduit Components";

            refs.interactionPromptUI?.ShowPrompt(
                "Press " + refs.inputSettings.interactKey + " to " + requirementText,
                target.gameObject,
                23
            );

            if (Input.GetKeyDown(refs.inputSettings.interactKey))
            {
                if (!conduitComponentsInstalled)
                {
                    if (!refs.playerInventory.TryRemoveItem(ItemType.ConduitComponents, 1))
                    {
                        ShowMissingItem("Conduit Components");
                        FlashTarget(target, missingColor);
                        return;
                    }

                    conduitComponentsInstalled = true;
                }

                conduitNodes[nearestIndex] = true;
                ApplyConduitVisual(nearestIndex);
                GetObjectiveManager(refs)?.NotifyConduitNodeReconnected(GetActiveConduitCount());

                if (GetActiveConduitCount() >= conduitNodes.Length)
                {
                    CompleteSignalRelay(refs);
                }
            }
        }
        else if (target != null)
        {
            refs.interactionPromptUI?.HidePrompt(target.gameObject);
        }
    }

    void OnSignalProcessorInstalled()
    {
        signalProcessorInstalled = true;
        ApplyRendererColor(processorPanel, activePanelColor);

        GameReferences refs = GameReferences.Instance;
        GetObjectiveManager(refs)?.NotifySignalProcessorInstalled();
        ShowStepMessage("SIGNAL PROCESSOR INSTALLED\nRelay logic core accepted.");
    }

    void OnStabilizerInstalled()
    {
        stabilizerInstalled = true;
        ApplyRendererColor(stabilizerPanel, activePanelColor);

        GameReferences refs = GameReferences.Instance;
        GetObjectiveManager(refs)?.NotifyStabilizerModuleInstalled();
        ShowStepMessage("STABILIZER MODULE INSTALLED\nOutput surge contained. Reconnect conduit array.");
    }

    void CompleteSignalRelay(GameReferences refs)
    {
        refs.interactionPromptUI?.HideAllPrompts();

        if (node != null)
        {
            node.CompleteRestoration(refs);
        }

        if (restoredLight != null)
        {
            restoredLight.enabled = true;
        }

        GetObjectiveManager(refs)?.NotifySignalRelayRestored();

        if (SystemMessageUI.Instance != null)
        {
            SystemMessageUI.Instance.ShowMessage(
                "SIGNAL RELAY RESTORED\nSignal sector reclaimed. Power Relay location identified.",
                6f
            );
        }
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
            refs.interactionPromptUI.ShowPrompt("Signal Relay restored - sector lighting online", gameObject, 10);
        }
        else
        {
            refs.interactionPromptUI.HidePrompt(gameObject);
        }
    }

    bool CanUseSignalRelay(GameReferences refs, out string lockReason)
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

    int GetNearestInactiveConduitNode(Vector3 playerPosition)
    {
        int nearestIndex = -1;
        float nearestDistance = float.MaxValue;

        for (int i = 0; i < conduitNodeTransforms.Length; i++)
        {
            if (conduitNodes[i] || conduitNodeTransforms[i] == null)
            {
                continue;
            }

            float distance = Vector3.Distance(playerPosition, conduitNodeTransforms[i].position);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestIndex = i;
            }
        }

        return nearestDistance <= interactionDistance ? nearestIndex : -1;
    }

    int GetActiveConduitCount()
    {
        int count = 0;

        for (int i = 0; i < conduitNodes.Length; i++)
        {
            if (conduitNodes[i])
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

        processorPanel = CreateRepairPrimitive("SignalRepair_ProcessorPanel", PrimitiveType.Cube, new Vector3(-1.2f, 0.35f, 1.35f), new Vector3(0.55f, 0.42f, 0.22f), inactivePanelColor).transform;
        stabilizerPanel = CreateRepairPrimitive("SignalRepair_StabilizerPanel", PrimitiveType.Cube, new Vector3(1.2f, 0.35f, 1.35f), new Vector3(0.55f, 0.42f, 0.22f), inactivePanelColor).transform;

        conduitNodeTransforms[0] = CreateRepairPrimitive("SignalRepair_ConduitNode_A", PrimitiveType.Cylinder, new Vector3(-1.55f, 0.12f, -1.45f), new Vector3(0.28f, 0.12f, 0.28f), inactivePanelColor).transform;
        conduitNodeTransforms[1] = CreateRepairPrimitive("SignalRepair_ConduitNode_B", PrimitiveType.Cylinder, new Vector3(0f, 0.12f, -1.9f), new Vector3(0.28f, 0.12f, 0.28f), inactivePanelColor).transform;
        conduitNodeTransforms[2] = CreateRepairPrimitive("SignalRepair_ConduitNode_C", PrimitiveType.Cylinder, new Vector3(1.55f, 0.12f, -1.45f), new Vector3(0.28f, 0.12f, 0.28f), inactivePanelColor).transform;

        GameObject lightObject = new GameObject("SignalRepair_RestoredLight");
        lightObject.transform.SetParent(transform, false);
        lightObject.transform.localPosition = new Vector3(0f, 2.4f, 0f);
        restoredLight = lightObject.AddComponent<Light>();
        restoredLight.type = LightType.Point;
        restoredLight.color = activePanelColor;
        restoredLight.range = 12f;
        restoredLight.intensity = 2.8f;
        restoredLight.enabled = node != null && node.restored;
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

            if (!child.name.StartsWith("SignalRepair_"))
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

    void ApplyConduitVisual(int index)
    {
        if (index < 0 || index >= conduitNodeTransforms.Length)
        {
            return;
        }

        ApplyRendererColor(conduitNodeTransforms[index], activePanelColor);
    }

    void FlashTarget(Transform target, Color color)
    {
        ApplyRendererColor(target, color);
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

    void ShowMissingItem(string itemName)
    {
        if (SystemMessageUI.Instance != null)
        {
            SystemMessageUI.Instance.ShowMessage("MISSING COMPONENT\n" + itemName + " required.", 3f);
        }
    }

    void ShowStepMessage(string message)
    {
        if (SystemMessageUI.Instance != null)
        {
            SystemMessageUI.Instance.ShowMessage(message, 4f);
        }
    }

    FirstRunObjectiveManager GetObjectiveManager(GameReferences refs)
    {
        if (refs != null && refs.firstRunObjectiveManager != null)
        {
            return refs.firstRunObjectiveManager;
        }

        return FindFirstObjectByType<FirstRunObjectiveManager>();
    }
}
