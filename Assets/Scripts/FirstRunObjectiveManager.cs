using System.Collections.Generic;
using UnityEngine;

public class FirstRunObjectiveManager : MonoBehaviour
{
    public int requiredRestoredNodes = 3;
    public float messageDuration = 5f;

    private BaseCampZone baseCampZone;
    private InfrastructureNetworkManager networkManager;
    private bool sawBaseCamp = false;
    private bool announcedRootOnline = false;
    private bool announcedSignalRelay = false;
    private bool announcedPowerJunction = false;
    private bool announcedTransitRelay = false;
    private bool announcedReadyToReturn = false;
    private bool completed = false;
    private bool signalRelayDiagnosed = false;
    private bool signalProcessorRecovered = false;
    private bool conduitComponentsRecovered = false;
    private bool stabilizerModuleCrafted = false;
    private bool signalProcessorInstalled = false;
    private bool stabilizerModuleInstalled = false;
    private int signalConduitNodesReconnected = 0;
    private readonly HashSet<int> recoveredEnvironmentalFragments = new HashSet<int>();

    void Start()
    {
        ResolveReferences();
    }

    void Update()
    {
        ResolveReferences();

        if (baseCampZone == null || networkManager == null)
        {
            return;
        }

        if (!sawBaseCamp && baseCampZone.IsPlayerInside())
        {
            sawBaseCamp = true;
            ShowMessage("BASE CAMP SIGNAL FOUND\nPassive decay paused. Startup sweep marked nearby salvage signatures. Recover them to reconnect the root relay.\n" + GetNavigationHint());
        }

        if (!announcedRootOnline && networkManager.IsBaseCampRootOnline())
        {
            announcedRootOnline = true;
            ShowMessage("ROOT RELAY ONLINE\nA nearby signal-service courtyard is marked. Restore its relay to extend the map and expose the next route.\n" + GetNavigationHint());
        }

        if (!announcedSignalRelay && networkManager.HasRestoredNodeType(InfrastructureNodeType.SignalRelay))
        {
            announcedSignalRelay = true;
            ShowMessage("SIGNAL RELAY ONLINE\nNavigation scan expanded. Its service salvage can power the next junction; newly detected traces are marked.");
        }

        if (!announcedPowerJunction && networkManager.HasRestoredNodeType(InfrastructureNodeType.PowerJunction))
        {
            announcedPowerJunction = true;
            ShowMessage("POWER JUNCTION ONLINE\nStability boundary projected. Hazards inside the lit radius are suppressed; the transit route lies farther out.");
        }

        if (!announcedTransitRelay && networkManager.HasRestoredNodeType(InfrastructureNodeType.TransitLift))
        {
            announcedTransitRelay = true;
            ShowMessage("TRANSIT SPINE PING RECEIVED\nSealed perimeter link marked in violet. Route into the next district remains locked.", 6f);
        }

        if (!announcedReadyToReturn && networkManager.IsBaseCampRootOnline() && networkManager.IsRequiredChainComplete())
        {
            announcedReadyToReturn = true;
            ShowMessage("LOCAL CHAIN SYNCHRONIZED\nReturn to base camp to lock restoration state and decode the recovered fragment.");
        }

        if (!completed && announcedReadyToReturn && baseCampZone.IsPlayerInside())
        {
            completed = true;
            networkManager.LockLocalChainAtBase();
            baseCampZone.CompleteLocalChainMilestone();
            ShowMessage("MEMORY FRAGMENT RECOVERED\n...maintenance unit FS-7 assigned to municipal nervous system...\nLOCAL CHAIN LOCKED. DEEPER CITY SIGNAL DETECTED.", 9f);
        }
    }

    public string GetObjectiveText(Vector3 playerPosition, PlayerInventory inventory)
    {
        ResolveReferences();

        if (completed)
        {
            return "MILESTONE COMPLETE: local chain synchronized\nMemory fragment recovered. Review traces at the root terminal; locked deep-city access marked.";
        }

        if (baseCampZone == null || networkManager == null)
        {
            return "OBJECTIVE: recover local system signal";
        }

        if (!baseCampZone.rootRelayOnline)
        {
            return "OBJECTIVE: repair Base Camp root relay\nRecover marked salvage: 2 Wiring + 1 Core Fragment. Return to the root console.\n" + GetNavigationHint();
        }

        int restoredCount = networkManager.GetRequiredChainProgress();
        int chainLength = networkManager.GetRequiredChainLength();

        if (networkManager.IsRequiredChainComplete())
        {
            return "OBJECTIVE: return to Base Camp\nDock recovered chain data and decode the memory fragment.";
        }

        InfrastructureNode nearest = networkManager.GetCurrentRequiredNode(playerPosition);

        if (nearest == null)
        {
            return "OBJECTIVE: scan for further infrastructure signals";
        }

        if (nearest.nodeType == InfrastructureNodeType.SignalRelay)
        {
            return GetSignalRelayObjectiveText(nearest, playerPosition, inventory, restoredCount, chainLength);
        }

        string affordability = inventory != null && inventory.CanAfford(nearest.repairCosts)
            ? "repair resources ready"
            : "needs " + nearest.GetCostText();

        return
            "OBJECTIVE: restore " + nearest.GetDisplayName() + " (" + restoredCount + "/" + chainLength + ")\n" +
            nearest.GetEffectSummary() + "\n" +
            Mathf.RoundToInt(Vector3.Distance(playerPosition, nearest.transform.position)) + "m away - " + affordability + " | " + GetNavigationHint() + "\n" +
            networkManager.GetRequiredChainText();
    }

    public bool IsCompleted()
    {
        return completed;
    }

    public void NotifySignalRelayDiagnosed()
    {
        signalRelayDiagnosed = true;
    }

    public void NotifyRelaySalvageLooted(ItemCost[] contents)
    {
        if (contents == null)
        {
            return;
        }

        for (int i = 0; i < contents.Length; i++)
        {
            if (contents[i].itemType == ItemType.SignalProcessor)
            {
                signalProcessorRecovered = true;
            }
            else if (contents[i].itemType == ItemType.ConduitComponents)
            {
                conduitComponentsRecovered = true;
            }
        }
    }

    public void NotifyStabilizerModuleCrafted()
    {
        stabilizerModuleCrafted = true;
    }

    public void NotifySignalProcessorInstalled()
    {
        signalProcessorInstalled = true;
    }

    public void NotifyStabilizerModuleInstalled()
    {
        stabilizerModuleInstalled = true;
    }

    public void NotifyConduitNodeReconnected(int activeNodeCount)
    {
        signalConduitNodesReconnected = Mathf.Clamp(activeNodeCount, 0, 3);
    }

    public void NotifySignalRelayRestored()
    {
        signalRelayDiagnosed = true;
        signalProcessorRecovered = true;
        conduitComponentsRecovered = true;
        stabilizerModuleCrafted = true;
        signalProcessorInstalled = true;
        stabilizerModuleInstalled = true;
        signalConduitNodesReconnected = 3;
    }

    public void RegisterEnvironmentalFragment(int fragmentIndex)
    {
        recoveredEnvironmentalFragments.Add(fragmentIndex);
    }

    public bool HasEnvironmentalFragment(int fragmentIndex)
    {
        return recoveredEnvironmentalFragments.Contains(fragmentIndex);
    }

    public int GetRecoveredEnvironmentalFragmentCount()
    {
        return recoveredEnvironmentalFragments.Count;
    }

    public string GetArchiveSummary()
    {
        string summary = "Context traces recovered: " + recoveredEnvironmentalFragments.Count + "/3";

        if (completed)
        {
            return summary +
                "\nIdentity fragment: FS-7 municipal nervous-system maintenance." +
                "\nDeep-route access remains locked.";
        }

        if (recoveredEnvironmentalFragments.Count > 0)
        {
            return summary + "\nIdentity data remains incomplete.";
        }

        return summary + "\nNo peripheral traces cached.";
    }

    string GetSignalRelayObjectiveText(
        InfrastructureNode signalRelay,
        Vector3 playerPosition,
        PlayerInventory inventory,
        int restoredCount,
        int chainLength)
    {
        int distance = Mathf.RoundToInt(Vector3.Distance(playerPosition, signalRelay.transform.position));

        if (!signalRelayDiagnosed)
        {
            return
                "OBJECTIVE: diagnose Signal Relay (" + restoredCount + "/" + chainLength + ")\n" +
                distance + "m away - run diagnostics at the relay courtyard.\n" +
                networkManager.GetRequiredChainText();
        }

        bool hasProcessor = signalProcessorInstalled || signalProcessorRecovered || HasInventoryItem(inventory, ItemType.SignalProcessor);
        bool hasConduitComponents = signalConduitNodesReconnected > 0 || conduitComponentsRecovered || HasInventoryItem(inventory, ItemType.ConduitComponents);
        bool hasStabilizer = stabilizerModuleInstalled || stabilizerModuleCrafted || HasInventoryItem(inventory, ItemType.StabilizerModule);

        if (!hasProcessor)
        {
            return
                "OBJECTIVE: find Signal Processor\n" +
                "Search Signal-sector resource opportunities near the relay.";
        }

        if (!hasConduitComponents)
        {
            return
                "OBJECTIVE: find Conduit Components\n" +
                "Search compact depots and utility alcoves in the Signal sector.";
        }

        if (!hasStabilizer)
        {
            string resourceLine = inventory != null
                ? "Workbench recipe needs 2 Metal Scrap + 2 Circuit Scrap + 1 Energy Cell."
                : "Craft the Stabilizer Module at the workshop.";
            return
                "OBJECTIVE: craft Stabilizer Module at workshop\n" +
                resourceLine;
        }

        if (!signalProcessorInstalled)
        {
            return
                "OBJECTIVE: return to Signal Relay\n" +
                "Install Signal Processor - " + distance + "m away.";
        }

        if (!stabilizerModuleInstalled)
        {
            return
                "OBJECTIVE: repair Signal Relay\n" +
                "Install Stabilizer Module at the relay panel.";
        }

        return
            "OBJECTIVE: reconnect Conduit Array\n" +
            "Activate conduit nodes around the relay (" + signalConduitNodesReconnected + "/3).";
    }

    bool HasInventoryItem(PlayerInventory inventory, ItemType itemType)
    {
        return inventory != null && inventory.GetItemCount(itemType) > 0;
    }

    public int GetArchivePageCount()
    {
        int pages = 1 + recoveredEnvironmentalFragments.Count;

        if (completed)
        {
            pages++;
        }

        return pages;
    }

    public string GetArchivePage(int requestedPage)
    {
        int pageCount = GetArchivePageCount();
        int page = Mathf.Clamp(requestedPage, 0, Mathf.Max(0, pageCount - 1));

        if (page == 0)
        {
            string correlationState = recoveredEnvironmentalFragments.Count == 0
                ? "NO MATCH"
                : recoveredEnvironmentalFragments.Count < 3 ? "PARTIAL MATCH" : "STRONG MATCH";
            return
                "ROOT ARCHIVE // LOCAL GRID\n" +
                "Recovered traces: " + recoveredEnvironmentalFragments.Count + "/3  Correlation: " + correlationState + "\n" +
                "Press interact again to cycle recovered records.";
        }

        int visiblePage = 1;

        for (int fragmentIndex = 1; fragmentIndex <= 3; fragmentIndex++)
        {
            if (!HasEnvironmentalFragment(fragmentIndex))
            {
                continue;
            }

            if (page == visiblePage)
            {
                return GetRecoveredTracePage(fragmentIndex);
            }

            visiblePage++;
        }

        return
            "IDENTITY RECONSTRUCTION // PARTIAL\n" +
            "\"FS-7 assigned to municipal nervous system.\"\n" +
            "Deep-route signal recognized. Origin remains unresolved.";
    }

    string GetRecoveredTracePage(int fragmentIndex)
    {
        switch (fragmentIndex)
        {
            case 1:
                return
                    "TRACE 01 // MAINTENANCE TRACE 04\n" +
                    "\"FS-7 reassigned beneath civic layer.\"\n" +
                    "Identity fields: intentionally scrubbed.";

            case 2:
                return
                    "TRACE 02 // GRID FAILURE TRACE\n" +
                    "\"ISOLATE LOWER GRID.\"\n" +
                    "Maintenance disconnect status: disputed.";

            default:
                return
                    "TRACE 03 // TRANSIT AUTHORIZATION\n" +
                    "\"Deep-route lock accepts FS-7 credential.\"\n" +
                    "Authorization source: [SELF / CORRUPT].";
        }
    }

    void ResolveReferences()
    {
        GameReferences refs = GameReferences.Instance;

        if (baseCampZone == null)
        {
            baseCampZone = refs != null ? refs.baseCampZone : null;
        }

        if (networkManager == null)
        {
            networkManager = refs != null ? refs.infrastructureNetworkManager : null;
        }

        if (baseCampZone == null)
        {
            baseCampZone = FindFirstObjectByType<BaseCampZone>();
        }

        if (networkManager == null)
        {
            networkManager = FindFirstObjectByType<InfrastructureNetworkManager>();
        }
    }

    void ShowMessage(string message)
    {
        ShowMessage(message, messageDuration);
    }

    void ShowMessage(string message, float duration)
    {
        if (SystemMessageUI.Instance != null)
        {
            SystemMessageUI.Instance.ShowMessage(message, duration);
        }

        Debug.Log(message.Replace("\n", " - "));
    }

    string GetNavigationHint()
    {
        GameReferences refs = GameReferences.Instance;
        KeyCode mapKey = refs != null && refs.inputSettings != null ? refs.inputSettings.mapKey : KeyCode.M;
        KeyCode scanKey = refs != null && refs.inputSettings != null ? refs.inputSettings.scanKey : KeyCode.Q;
        return "Map [" + mapKey + "]  Pulse scan [" + scanKey + "]";
    }
}
