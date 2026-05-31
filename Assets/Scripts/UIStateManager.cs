using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class UIStateManager : MonoBehaviour
{
    public static UIState CurrentState { get; private set; } = UIState.Gameplay;

    public PlayerMovement playerMovement;
    public CameraFollow cameraFollow;

    void Awake()
    {
        ResetStateForLoadedScene();
    }

    public static bool IsGameplay()
    {
        return CurrentState == UIState.Gameplay;
    }

    public static bool CanInteract()
    {
        return CurrentState == UIState.Gameplay;
    }

    public static bool IsTypingInInputField()
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        GameObject selectedObject = EventSystem.current.currentSelectedGameObject;

        if (selectedObject == null)
        {
            return false;
        }

        return selectedObject.GetComponent<TMP_InputField>() != null;
    }

    public void SetState(UIState newState)
    {
        if (CurrentState == UIState.GameOver && newState != UIState.GameOver)
        {
            return;
        }

        CurrentState = newState;

        if (CurrentState == UIState.GameOver)
        {
            CloseBlockingInterfacesForGameOver();
        }

        ApplyStateEffects();
    }

    public void ReturnToGameplay()
    {
        if (CurrentState == UIState.GameOver)
        {
            return;
        }

        CurrentState = UIState.Gameplay;
        ApplyStateEffects();
    }

    private void ApplyStateEffects()
    {
        bool gameplayControlsEnabled = CurrentState == UIState.Gameplay;

        if (playerMovement != null)
        {
            playerMovement.SetCanMove(gameplayControlsEnabled);
        }

        if (cameraFollow != null)
        {
            cameraFollow.SetCanLook(gameplayControlsEnabled);
        }

        bool cursorVisible = CurrentState != UIState.Gameplay;

        Cursor.visible = cursorVisible;
        Cursor.lockState = cursorVisible ? CursorLockMode.None : CursorLockMode.Locked;

        Time.timeScale = CurrentState == UIState.Pause || CurrentState == UIState.GameOver ? 0f : 1f;
    }

    void ResetStateForLoadedScene()
    {
        CurrentState = UIState.Gameplay;
        Time.timeScale = 1f;
        ApplyStateEffects();
    }

    void CloseBlockingInterfacesForGameOver()
    {
        GameReferences refs = GameReferences.Instance;

        if (refs == null)
        {
            return;
        }

        if (refs.inventoryUI != null)
        {
            refs.inventoryUI.ForceCloseInventory();
        }

        if (refs.workbenchUI != null)
        {
            refs.workbenchUI.ForceCloseWorkbench();
        }

        if (refs.minimapUI != null)
        {
            refs.minimapUI.ForceCloseMap();
        }

        if (refs.interactionPromptUI != null)
        {
            refs.interactionPromptUI.HideAllPrompts();
        }
    }
}
