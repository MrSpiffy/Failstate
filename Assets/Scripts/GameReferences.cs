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

    void Awake()
    {
        Instance = this;
    }
}