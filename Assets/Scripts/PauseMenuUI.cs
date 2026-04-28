using UnityEngine;

public class PauseMenuUI : MonoBehaviour
{
    public static bool IsPauseMenuOpen { get; private set; } = false;

    public UIStateManager uiStateManager;

    public GameObject pauseMenuPanel;
    public GameObject pauseMainPage;
    public GameObject pauseSettingsPage;
    public GameObject pauseControlsPage;

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
        GameReferences refs = GameReferences.Instance;

        if (IsPauseMenuOpen)
        {
            ClosePauseMenu();
            return;
        }

        if (InventoryUI.IsInventoryOpen)
        {
            if (refs != null && refs.inventoryUI != null)
            {
                refs.inventoryUI.CloseInventory();
            }

            return;
        }

        if (WorkbenchUI.IsWorkbenchOpen)
        {
            if (refs != null && refs.workbenchUI != null)
            {
                refs.workbenchUI.CloseWorkbench();
            }

            return;
        }

        if (UIStateManager.CurrentState == UIState.Gameplay)
        {
            OpenPauseMenu();
        }
    }

    public void OpenPauseMenu()
    {
        IsPauseMenuOpen = true;

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }

        ShowMainPage();

        if (interactionPromptUI != null)
        {
            interactionPromptUI.HideAllPrompts();
        }

        if (uiStateManager != null)
        {
            uiStateManager.SetState(UIState.Pause);
        }
    }

    public void ClosePauseMenu()
    {
        IsPauseMenuOpen = false;

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        if (uiStateManager != null)
        {
            uiStateManager.ReturnToGameplay();
        }
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