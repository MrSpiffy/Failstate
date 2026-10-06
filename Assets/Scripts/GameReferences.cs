using UnityEngine;

public class GameReferences : MonoBehaviour
{
    public static GameReferences Instance { get; private set; }

    public GameObject playerObject;
    public Transform playerTransform;
    public PlayerCondition playerCondition;
    public InputSettings inputSettings;

    public PlayerInventory playerInventory;
    public InteractionPromptUI interactionPromptUI;
    public WorkbenchUI workbenchUI;
    public InventoryUI inventoryUI;
    public PauseMenuUI pauseMenuUI;
    public RestUI restUI;
    public BaseCampStatusUI baseCampStatusUI;
    public MinimapUI minimapUI;
    public NetworkObjectiveUI networkObjectiveUI;
    public FirstRunObjectiveManager firstRunObjectiveManager;
    public RobotSystemStatusHUD robotSystemStatusHUD;
    public SystemMessageUI systemMessageUI;

    [Header("Scene Systems")]
    public CityBlockoutGenerator cityGenerator;
    public BaseCampZone baseCampZone;
    public WorkbenchStation workbenchStation;
    public RechargeStation rechargeStation;
    public InfrastructureNetworkManager infrastructureNetworkManager;

    void Awake()
    {
        Instance = this;
        ResolveSceneBootstrapReferences();
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    void ResolveSceneBootstrapReferences()
    {
        if (playerObject == null)
        {
            playerObject = GameObject.FindGameObjectWithTag("Player");
        }

        if (playerTransform == null && playerObject != null) playerTransform = playerObject.transform;
        if (playerCondition == null && playerObject != null) playerCondition = playerObject.GetComponent<PlayerCondition>();
        if (inputSettings == null && playerObject != null) inputSettings = playerObject.GetComponent<InputSettings>();
        if (playerInventory == null && playerObject != null) playerInventory = playerObject.GetComponent<PlayerInventory>();

        if (interactionPromptUI == null) interactionPromptUI = FindFirstObjectByType<InteractionPromptUI>();
        if (workbenchUI == null) workbenchUI = FindFirstObjectByType<WorkbenchUI>();
        if (inventoryUI == null) inventoryUI = FindFirstObjectByType<InventoryUI>();
        if (pauseMenuUI == null) pauseMenuUI = FindFirstObjectByType<PauseMenuUI>();
        if (restUI == null) restUI = FindFirstObjectByType<RestUI>();
        if (baseCampStatusUI == null) baseCampStatusUI = FindFirstObjectByType<BaseCampStatusUI>();
        if (minimapUI == null) minimapUI = FindFirstObjectByType<MinimapUI>();
        if (networkObjectiveUI == null) networkObjectiveUI = FindFirstObjectByType<NetworkObjectiveUI>();
        if (firstRunObjectiveManager == null) firstRunObjectiveManager = FindFirstObjectByType<FirstRunObjectiveManager>();
        if (robotSystemStatusHUD == null) robotSystemStatusHUD = FindFirstObjectByType<RobotSystemStatusHUD>();
        if (systemMessageUI == null) systemMessageUI = FindFirstObjectByType<SystemMessageUI>();

        if (cityGenerator == null) cityGenerator = FindFirstObjectByType<CityBlockoutGenerator>();
        if (baseCampZone == null) baseCampZone = FindFirstObjectByType<BaseCampZone>();
        if (workbenchStation == null) workbenchStation = FindFirstObjectByType<WorkbenchStation>();
        if (rechargeStation == null) rechargeStation = FindFirstObjectByType<RechargeStation>();

        if (infrastructureNetworkManager == null)
        {
            infrastructureNetworkManager = FindFirstObjectByType<InfrastructureNetworkManager>();
        }

        if (infrastructureNetworkManager == null)
        {
            GameObject networkObject = new GameObject("InfrastructureNetwork");
            infrastructureNetworkManager = networkObject.AddComponent<InfrastructureNetworkManager>();
        }
    }

}
