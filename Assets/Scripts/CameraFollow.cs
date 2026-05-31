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

    [Header("Camera Collision")]
    public LayerMask collisionLayers;
    public float cameraCollisionRadius = 0.3f;
    public float cameraCollisionPadding = 0.2f;

    public float minCameraDistance = 2.25f;
    public float maxCameraPitch = 70f;
    public float focusHeight = 1.5f;
    public float collisionReturnSpeed = 12f;

    private PlayerMovement playerMovement;
    private float currentCameraDistance = -1f;

    void Start()
    {
        playerCondition = target != null ? target.GetComponent<PlayerCondition>() : null;
        playerMovement = target != null ? target.GetComponent<PlayerMovement>() : null;

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
                xRotation = Mathf.Clamp(xRotation, -maxCameraPitch, maxCameraPitch);
            }
        }

        float yaw = playerMovement != null ? playerMovement.GetViewYaw() : target.eulerAngles.y;
        Quaternion baseRotation = Quaternion.Euler(xRotation, yaw, 0f);
        Vector3 rotatedOffset = baseRotation * offset;

        Vector3 focusPoint = target.position + Vector3.up * focusHeight;
        Vector3 desiredPosition = target.position + rotatedOffset;
        Vector3 directionToCamera = desiredPosition - focusPoint;
        float desiredDistance = directionToCamera.magnitude;
        float unobstructedDistance = desiredDistance;

        if (Physics.SphereCast(
            focusPoint,
            cameraCollisionRadius,
            directionToCamera.normalized,
            out RaycastHit hit,
            desiredDistance,
            collisionLayers,
            QueryTriggerInteraction.Ignore))
        {
            unobstructedDistance = Mathf.Max(minCameraDistance, hit.distance - cameraCollisionPadding);
        }

        if (currentCameraDistance < 0f)
        {
            currentCameraDistance = unobstructedDistance;
        }
        else if (unobstructedDistance < currentCameraDistance)
        {
            currentCameraDistance = unobstructedDistance;
        }
        else
        {
            currentCameraDistance = Mathf.MoveTowards(
                currentCameraDistance,
                unobstructedDistance,
                collisionReturnSpeed * Time.deltaTime
            );
        }

        Vector3 finalPosition = focusPoint + directionToCamera.normalized * currentCameraDistance;
        Vector3 lookDirection = focusPoint - finalPosition;
        Quaternion finalRotation;

        if (lookDirection.sqrMagnitude > 0.001f)
        {
            finalRotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);

            Vector3 euler = finalRotation.eulerAngles;
            euler.z = 0f;
            finalRotation = Quaternion.Euler(euler);
        }
        else
        {
            finalRotation = baseRotation;
        }

        ApplyPerceptionWobble(ref finalPosition, ref finalRotation);

        transform.SetPositionAndRotation(finalPosition, finalRotation);
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
