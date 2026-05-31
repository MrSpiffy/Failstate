using UnityEngine;

public class BaseCampZone : MonoBehaviour
{
    [Header("Root Relay")]
    public bool rootRelayOnline = false;
    public ItemCost[] rootRepairCosts = new ItemCost[]
    {
        new ItemCost(ItemType.Wiring, 2),
        new ItemCost(ItemType.CoreFragment, 1)
    };
    public float rootRevealRadius = 18f;
    public float initialSalvageSweepRadius = 28f;
    public float restoredRootSignalSweepRadius = 48f;

    [Header("Optional Resource Deposits")]
    public bool depositAllBasicResources = false;

    [Header("First Restoration Chain")]
    public bool localChainSynchronized = false;

    private int depositedMetalScrap = 0;
    private int depositedWiring = 0;
    private int depositedCoreFragments = 0;
    private bool playerInside = false;
    private bool issuedStartupSalvageSweep = false;
    private GameObject synchronizedIndicator;

    void Update()
    {
        if (!playerInside || rootRelayOnline || !UIStateManager.CanInteract())
        {
            return;
        }

        GameReferences refs = GameReferences.Instance;

        if (refs == null || refs.inputSettings == null)
        {
            return;
        }

        if (refs.interactionPromptUI != null)
        {
            refs.interactionPromptUI.ShowPrompt(
                GetRootRelayPrompt(refs.playerInventory, refs.inputSettings.interactKey),
                gameObject,
                0
            );
        }

        if (Input.GetKeyDown(refs.inputSettings.interactKey))
        {
            TryRestoreRootRelay(refs);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerInside = true;
        PlayerCondition playerCondition = other.GetComponent<PlayerCondition>();

        if (playerCondition != null)
        {
            playerCondition.passiveDecayEnabled = false;
        }

        GameReferences refs = GameReferences.Instance;
        PlayerInventory inventory = other.GetComponent<PlayerInventory>();

        if (inventory == null && refs != null)
        {
            inventory = refs.playerInventory;
        }

        if (depositAllBasicResources)
        {
            DepositInventoryItems(inventory);
        }

        if (refs != null && refs.infrastructureNetworkManager != null)
        {
            refs.infrastructureNetworkManager.SetBaseCampRootOnline(rootRelayOnline);
        }

        if (refs != null && refs.baseCampStatusUI != null)
        {
            refs.baseCampStatusUI.ShowBaseCampStatus(this);
        }

        if (!rootRelayOnline && !issuedStartupSalvageSweep && refs != null && refs.minimapUI != null)
        {
            issuedStartupSalvageSweep = true;
            refs.minimapUI.ScanAroundWorldPosition(transform.position, initialSalvageSweepRadius);
        }

        Debug.Log("Entered base camp. Passive decay paused.");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerInside = false;

        PlayerCondition playerCondition = other.GetComponent<PlayerCondition>();

        if (playerCondition != null)
        {
            PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
            playerCondition.passiveDecayEnabled = playerMovement == null || !playerMovement.creativeModeEnabled;
        }

        GameReferences refs = GameReferences.Instance;

        if (refs != null && refs.interactionPromptUI != null)
        {
            refs.interactionPromptUI.HidePrompt(gameObject);
        }

        if (refs != null && refs.baseCampStatusUI != null)
        {
            refs.baseCampStatusUI.HideBaseCampStatus();
        }

        Debug.Log("Exited base camp. Passive decay resumed.");
    }

    public bool TryRestoreRootRelay(GameReferences refs)
    {
        if (rootRelayOnline || refs == null || refs.playerInventory == null)
        {
            return false;
        }

        if (!refs.playerInventory.SpendItems(rootRepairCosts))
        {
            return false;
        }

        return CompleteRootRestoration(refs);
    }

    public bool DebugRestoreRootRelay(GameReferences refs)
    {
        if (rootRelayOnline)
        {
            return false;
        }

        return CompleteRootRestoration(refs);
    }

    bool CompleteRootRestoration(GameReferences refs)
    {
        rootRelayOnline = true;

        if (refs != null && refs.infrastructureNetworkManager != null)
        {
            refs.infrastructureNetworkManager.SetBaseCampRootOnline(true);
        }

        if (refs != null && refs.minimapUI != null)
        {
            refs.minimapUI.RevealAroundWorldPosition(transform.position, rootRevealRadius);
            refs.minimapUI.ScanAroundWorldPosition(transform.position, restoredRootSignalSweepRadius);
        }

        if (refs != null && refs.interactionPromptUI != null)
        {
            refs.interactionPromptUI.HidePrompt(gameObject);
        }

        if (refs != null && refs.baseCampStatusUI != null)
        {
            refs.baseCampStatusUI.ShowBaseCampStatus(this);
        }

        Debug.Log("Base Camp root relay restored.");
        return true;
    }

    void DepositInventoryItems(PlayerInventory inventory)
    {
        if (inventory == null)
        {
            return;
        }

        DepositItem(inventory, ItemType.MetalScrap);
        DepositItem(inventory, ItemType.Wiring);
        DepositItem(inventory, ItemType.CoreFragment);
    }

    public int StoreBasicSalvage(PlayerInventory inventory)
    {
        int before = GetStoredSalvageTotal();
        DepositInventoryItems(inventory);
        RefreshDisplayedStatus();
        return GetStoredSalvageTotal() - before;
    }

    public int WithdrawBasicSalvage(PlayerInventory inventory)
    {
        if (inventory == null)
        {
            return 0;
        }

        int total = GetStoredSalvageTotal();

        if (depositedMetalScrap > 0)
        {
            inventory.AddItem(ItemType.MetalScrap, depositedMetalScrap);
        }

        if (depositedWiring > 0)
        {
            inventory.AddItem(ItemType.Wiring, depositedWiring);
        }

        if (depositedCoreFragments > 0)
        {
            inventory.AddItem(ItemType.CoreFragment, depositedCoreFragments);
        }

        depositedMetalScrap = 0;
        depositedWiring = 0;
        depositedCoreFragments = 0;
        RefreshDisplayedStatus();
        return total;
    }

    public bool HasStoredSalvage()
    {
        return GetStoredSalvageTotal() > 0;
    }

    public int GetStoredSalvageTotal()
    {
        return depositedMetalScrap + depositedWiring + depositedCoreFragments;
    }

    void DepositItem(PlayerInventory inventory, ItemType itemType)
    {
        int amount = inventory.GetItemCount(itemType);

        if (amount <= 0 || !inventory.TryRemoveItem(itemType, amount))
        {
            return;
        }

        if (itemType == ItemType.MetalScrap)
        {
            depositedMetalScrap += amount;
        }
        else if (itemType == ItemType.Wiring)
        {
            depositedWiring += amount;
        }
        else if (itemType == ItemType.CoreFragment)
        {
            depositedCoreFragments += amount;
        }
    }

    public bool IsObjectiveComplete()
    {
        return rootRelayOnline;
    }

    public bool IsPlayerInside()
    {
        return playerInside;
    }

    public void CompleteLocalChainMilestone()
    {
        if (localChainSynchronized)
        {
            return;
        }

        localChainSynchronized = true;
        BuildSynchronizedIndicator();
    }

    public string GetRootRelayStatusText()
    {
        if (rootRelayOnline)
        {
            return localChainSynchronized
                ? "Root relay online - local chain synchronized"
                : "Root relay online";
        }

        return "Root relay offline - repair cost: " + GetCostText(rootRepairCosts);
    }

    public string GetDepositSummary()
    {
        return
            "Stored salvage" +
            "\nMetal: " + depositedMetalScrap +
            " | Wiring: " + depositedWiring +
            " | Core Fragments: " + depositedCoreFragments;
    }

    void RefreshDisplayedStatus()
    {
        GameReferences refs = GameReferences.Instance;

        if (playerInside && refs != null && refs.baseCampStatusUI != null)
        {
            refs.baseCampStatusUI.ShowBaseCampStatus(this);
        }
    }

    string GetRootRelayPrompt(PlayerInventory inventory, KeyCode interactKey)
    {
        bool canAfford = inventory != null && inventory.CanAfford(rootRepairCosts);

        if (canAfford)
        {
            return "Press " + interactKey + " to reconnect Base Camp root relay";
        }

        return "Root relay offline - missing: " + GetCostText(rootRepairCosts);
    }

    string GetCostText(ItemCost[] costs)
    {
        if (costs == null || costs.Length == 0)
        {
            return "no resources";
        }

        string result = "";

        for (int i = 0; i < costs.Length; i++)
        {
            result += costs[i].amount + " " + ItemDatabase.GetDisplayName(costs[i].itemType);

            if (i < costs.Length - 1)
            {
                result += " + ";
            }
        }

        return result;
    }

    void BuildSynchronizedIndicator()
    {
        if (synchronizedIndicator != null)
        {
            Destroy(synchronizedIndicator);
        }

        synchronizedIndicator = new GameObject("BaseCamp_SynchronizedChainIndicator");
        synchronizedIndicator.transform.position = new Vector3(transform.position.x, 0.11f, transform.position.z);
        synchronizedIndicator.transform.rotation = transform.rotation;

        GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.name = "BaseCampSync_RootRing";
        ring.transform.SetParent(synchronizedIndicator.transform, false);
        ring.transform.localScale = new Vector3(2.2f, 0.035f, 2.2f);
        RemoveColliderAndTint(ring, new Color(0.38f, 1f, 0.72f, 1f));

        GameObject innerRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        innerRing.name = "BaseCampSync_DataLock";
        innerRing.transform.SetParent(synchronizedIndicator.transform, false);
        innerRing.transform.localPosition = new Vector3(0f, 0.04f, 0f);
        innerRing.transform.localScale = new Vector3(1.35f, 0.045f, 1.35f);
        RemoveColliderAndTint(innerRing, new Color(0.3f, 0.75f, 0.62f, 1f));

        GameObject labelObject = new GameObject("BaseCampSync_Label");
        labelObject.transform.SetParent(synchronizedIndicator.transform, false);
        labelObject.transform.localPosition = new Vector3(0f, 1.35f, 0f);

        TextMesh label = labelObject.AddComponent<TextMesh>();
        label.text = "GRID LINKED";
        label.fontSize = 40;
        label.characterSize = 0.05f;
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.color = new Color(0.55f, 1f, 0.8f, 1f);

        Color poweredColor = new Color(0.18f, 0.72f, 0.57f, 1f);
        CreatePoweredBaseStrip("BaseCampSync_LeftSolarRail", new Vector3(-2.45f, 5.41f, -0.35f), new Vector3(0.11f, 0.1f, 2.25f), poweredColor);
        CreatePoweredBaseStrip("BaseCampSync_RightSolarRail", new Vector3(2.45f, 5.41f, -0.35f), new Vector3(0.11f, 0.1f, 2.25f), poweredColor);
        CreatePoweredBaseStrip("BaseCampSync_ConsoleLine", new Vector3(3.85f, 1.16f, -5.76f), new Vector3(1.7f, 0.08f, 0.05f), poweredColor);
        CreatePoweredBaseLight("BaseCampSync_LeftLight", new Vector3(-2.45f, 4.97f, -2.3f), poweredColor);
        CreatePoweredBaseLight("BaseCampSync_RightLight", new Vector3(2.45f, 4.97f, -2.3f), poweredColor);
    }

    void CreatePoweredBaseStrip(string objectName, Vector3 localPosition, Vector3 scale, Color color)
    {
        GameObject strip = GameObject.CreatePrimitive(PrimitiveType.Cube);
        strip.name = objectName;
        strip.transform.SetParent(synchronizedIndicator.transform, false);
        strip.transform.localPosition = localPosition;
        strip.transform.localScale = scale;
        RemoveColliderAndTint(strip, color);
    }

    void CreatePoweredBaseLight(string objectName, Vector3 localPosition, Color color)
    {
        GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Cube);
        indicator.name = objectName;
        indicator.transform.SetParent(synchronizedIndicator.transform, false);
        indicator.transform.localPosition = localPosition;
        indicator.transform.localScale = new Vector3(0.38f, 0.14f, 0.16f);
        RemoveColliderAndTint(indicator, color);

        Light light = indicator.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.range = 5.5f;
        light.intensity = 0.8f;
    }

    void RemoveColliderAndTint(GameObject visual, Color color)
    {
        Collider collider = visual.GetComponent<Collider>();

        if (collider != null)
        {
            Destroy(collider);
        }

        Renderer renderer = visual.GetComponent<Renderer>();

        if (renderer != null)
        {
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            block.SetColor("_BaseColor", color);
            block.SetColor("_Color", color);
            renderer.SetPropertyBlock(block);
        }
    }
}
