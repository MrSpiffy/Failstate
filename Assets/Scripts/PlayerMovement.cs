using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float baseMoveSpeed = 5f;
    public float minimumMoveSpeedMultiplier = 0.35f;
    public float mouseSensitivity = 100f;

    private Rigidbody rb;
    private Vector3 movement;
    private bool canMove = true;
    private PlayerCondition playerCondition;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerCondition = GetComponent<PlayerCondition>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (!canMove)
        {
            movement = Vector3.zero;
            return;
        }

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        movement = transform.right * moveX + transform.forward * moveZ;

        float rawMouseX = Input.GetAxis("Mouse X");

if (Mathf.Abs(rawMouseX) > 0.01f)
{
    float mouseX = rawMouseX * mouseSensitivity * Time.deltaTime;
    transform.Rotate(Vector3.up * mouseX);
}
    }

    void FixedUpdate()
    {
        float currentMoveSpeed = baseMoveSpeed;

        if (playerCondition != null)
        {
            float mobilityPercent = playerCondition.GetMobilityPercent();
            float speedMultiplier = Mathf.Lerp(minimumMoveSpeedMultiplier, 1f, mobilityPercent);
            currentMoveSpeed *= speedMultiplier;
        }

        Vector3 newPosition = rb.position + movement.normalized * currentMoveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);
    }

    public void SetCanMove(bool value)
    {
        canMove = value;
    }
}