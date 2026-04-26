using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class UIStateManager : MonoBehaviour
{
    public static UIState CurrentState { get; private set; } = UIState.Gameplay;

    public PlayerMovement playerMovement;
    public CameraFollow cameraFollow;

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
        CurrentState = newState;
        ApplyStateEffects();
    }

    public void ReturnToGameplay()
    {
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

        Time.timeScale = CurrentState == UIState.Pause ? 0f : 1f;
    }
}