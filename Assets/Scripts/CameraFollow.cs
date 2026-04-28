using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 2f, -4f);
    public float mouseSensitivity = 100f;

    [Header("Perception Wobble")]
    public float maxWobblePositionAmount = 0.15f;
    public float maxWobbleRotationAmount = 2f;
    public float wobbleSpeed = 3f;

    private float xRotation = 0f;
    private bool canLook = true;
    private PlayerCondition playerCondition;

    void Start()
    {
        playerCondition = target != null ? target.GetComponent<PlayerCondition>() : null;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (target == null) return;

        if (canLook)
        {
            float rawMouseY = Input.GetAxisRaw("Mouse Y");

            if (Mathf.Abs(rawMouseY) > 0.01f)
            {
                float mouseY = rawMouseY * mouseSensitivity * Time.deltaTime;
                xRotation -= mouseY;
                xRotation = Mathf.Clamp(xRotation, -60f, 60f);
            }
        }

        Quaternion baseRotation = Quaternion.Euler(xRotation, target.eulerAngles.y, 0f);
        Vector3 rotatedOffset = baseRotation * offset;

        Vector3 finalPosition = target.position + rotatedOffset;
        Quaternion finalRotation = Quaternion.LookRotation((target.position + Vector3.up * 1.5f) - finalPosition);

        ApplyPerceptionWobble(ref finalPosition, ref finalRotation);

        transform.position = finalPosition;
        transform.rotation = finalRotation;
    }

    void ApplyPerceptionWobble(ref Vector3 position, ref Quaternion rotation)
    {
        if (playerCondition == null) return;

        float perceptionPercent = 1f;

        if (playerCondition.maxPerceptionIntegrity > 0f)
        {
            perceptionPercent = playerCondition.currentPerceptionIntegrity / playerCondition.maxPerceptionIntegrity;
        }

        float wobbleStrength = 1f - perceptionPercent;

        if (wobbleStrength <= 0f) return;

        float time = Time.time * wobbleSpeed;

        float xOffset = Mathf.Sin(time * 1.3f) * maxWobblePositionAmount * wobbleStrength;
        float yOffset = Mathf.Cos(time * 1.7f) * maxWobblePositionAmount * wobbleStrength * 0.75f;

        position += transform.right * xOffset;
        position += transform.up * yOffset;

        float zTilt = Mathf.Sin(time * 1.5f) * maxWobbleRotationAmount * wobbleStrength;
        rotation *= Quaternion.Euler(0f, 0f, zTilt);
    }

    public void SetCanLook(bool value)
    {
        canLook = value;
    }
}