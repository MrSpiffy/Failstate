using UnityEngine;

public class PauseMenuUI : MonoBehaviour
{
    public static bool IsPauseMenuOpen { get; private set; } = false;

    public GameObject pauseMenuPanel;
    public GameObject pauseMainPage;
    public GameObject pauseSettingsPage;
    public GameObject pauseControlsPage;

    public PlayerMovement playerMovement;
    public CameraFollow cameraFollow;
    public InteractionPromptUI interactionPromptUI;

    void Update()
{
    if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (DevConsoleUI.ConsumeEscape())
            {
                return;
            }

            HandleEscapePressed();
        }

        if (DevConsoleUI.IsConsoleOpen)
        {
            return;
        }

    if (IsPauseMenuOpen)
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ShowMainPage();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ShowSettingsPage();
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ShowControlsPage();
        }
    }
}

    void HandleEscapePressed()
    {
        if (IsPauseMenuOpen)
        {
            ClosePauseMenu();
            return;
        }

        if (InventoryUI.IsInventoryOpen)
        {
            InventoryUI inventoryUI = FindFirstObjectByType<InventoryUI>();
            if (inventoryUI != null)
            {
                inventoryUI.CloseInventory();
            }
            return;
        }

        if (WorkbenchUI.IsWorkbenchOpen)
        {
            WorkbenchUI workbenchUI = FindFirstObjectByType<WorkbenchUI>();
            if (workbenchUI != null)
            {
                workbenchUI.CloseWorkbench();
            }
            return;
        }

        OpenPauseMenu();
    }

    public void OpenPauseMenu()
    {
        IsPauseMenuOpen = true;

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }

        ShowMainPage();

        if (playerMovement != null)
        {
            playerMovement.SetCanMove(false);
        }

        if (cameraFollow != null)
        {
            cameraFollow.SetCanLook(false);
        }

        if (interactionPromptUI != null)
        {
            interactionPromptUI.HideAllPrompts();
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;
    }

    public void ClosePauseMenu()
    {
        IsPauseMenuOpen = false;

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
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

        Time.timeScale = 1f;
    }

    public void ShowMainPage()
    {
        if (pauseMainPage != null) pauseMainPage.SetActive(true);
        if (pauseSettingsPage != null) pauseSettingsPage.SetActive(false);
        if (pauseControlsPage != null) pauseControlsPage.SetActive(false);
    }

    public void ShowSettingsPage()
    {
        if (pauseMainPage != null) pauseMainPage.SetActive(false);
        if (pauseSettingsPage != null) pauseSettingsPage.SetActive(true);
        if (pauseControlsPage != null) pauseControlsPage.SetActive(false);
    }

    public void ShowControlsPage()
    {
        if (pauseMainPage != null) pauseMainPage.SetActive(false);
        if (pauseSettingsPage != null) pauseSettingsPage.SetActive(false);
        if (pauseControlsPage != null) pauseControlsPage.SetActive(true);
    }
}