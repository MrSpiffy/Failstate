using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public static bool IsInventoryOpen { get; private set; } = false;

    public GameObject inventoryPanel;
    public ScrapInventory scrapInventory;
    public InputSettings inputSettings;
    public PlayerMovement playerMovement;
    public CameraFollow cameraFollow;

    void Update()
    {
        if (inputSettings == null) return;

        if (!WorkbenchUI.IsWorkbenchOpen && !PauseMenuUI.IsPauseMenuOpen && !DevConsoleUI.IsConsoleOpen && Input.GetKeyDown(inputSettings.inventoryKey))
        {
            ToggleInventory();
        }

        if (IsInventoryOpen && scrapInventory != null)
        {
            scrapInventory.UpdateInventoryText();
        }
    }

    void ToggleInventory()
    {
        if (IsInventoryOpen)
        {
            CloseInventory();
        }
        else
        {
            OpenInventory();
        }
    }

    public void OpenInventory()
    {
        IsInventoryOpen = true;

        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(true);
        }

        if (scrapInventory != null)
        {
            scrapInventory.UpdateInventoryText();
        }

        if (playerMovement != null)
        {
            playerMovement.SetCanMove(false);
        }

        if (cameraFollow != null)
        {
            cameraFollow.SetCanLook(false);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseInventory()
    {
        IsInventoryOpen = false;

        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }

        if (playerMovement != null)
        {
            playerMovement.SetCanMove(true);
        }

        if (cameraFollow != null)
        {
            cameraFollow.SetCanLook(true);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}