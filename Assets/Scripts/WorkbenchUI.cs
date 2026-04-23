using UnityEngine;

public class WorkbenchUI : MonoBehaviour
{
    public static bool IsWorkbenchOpen { get; private set; } = false;

    public GameObject workbenchPanel;
    public PlayerMovement playerMovement;
    public CameraFollow cameraFollow;

    public void ToggleWorkbench()
    {
        IsWorkbenchOpen = !IsWorkbenchOpen;

        if (workbenchPanel != null)
        {
            workbenchPanel.SetActive(IsWorkbenchOpen);
        }

        if (playerMovement != null)
        {
            playerMovement.SetCanMove(!IsWorkbenchOpen);
        }

        if (cameraFollow != null)
        {
            cameraFollow.SetCanLook(!IsWorkbenchOpen);
        }

        Cursor.lockState = IsWorkbenchOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = IsWorkbenchOpen;
    }

    public void CloseWorkbench()
    {
        if (!IsWorkbenchOpen) return;

        IsWorkbenchOpen = false;

        if (workbenchPanel != null)
        {
            workbenchPanel.SetActive(false);
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