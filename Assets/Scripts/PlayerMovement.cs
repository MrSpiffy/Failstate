using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float baseMoveSpeed = 5f;
    public float minimumMoveSpeedMultiplier = 0.35f;
    public float mouseSensitivity = 100f;

    [Header("Ground Movement")]
    public float groundAcceleration = 24f;
    public float groundDirectionChangeAcceleration = 38f;
    public float groundDeceleration = 42f;
    public float airControlMultiplier = 0.45f;

    [Header("Wall Contact")]
    public bool useLowFrictionCollider = true;
    public float wallNormalMaxVerticalComponent = 0.3f;
    public float wallImpactInputThreshold = 0.15f;

    [Header("Jumping")]
    public float jumpForce = 6f;
    public LayerMask groundLayer;
    public float groundCheckDistance = 1.1f;
    public float jumpCooldown = 0.22f;
    public float groundedJumpMaxUpwardVelocity = 0.1f;

    [Header("Creative Mode")]
    public bool creativeModeEnabled = false;
    public bool flyingEnabled = false;
    public float creativeMoveSpeed = 12f;
    public float doubleTapTime = 0.3f;

    private Rigidbody rb;
    private Vector3 movement;
    private Vector3 planarVelocity;
    private bool canMove = true;
    private PlayerCondition playerCondition;
    private InputSettings inputSettings;
    private float targetYaw;
    private Vector3 recentAirborneWallNormal;
    private float airborneWallContactUntil = -999f;
    private PhysicsMaterial runtimeMovementMaterial;

    private float lastJumpTapTime = -999f;
    private float lastJumpTime = -999f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerCondition = GetComponent<PlayerCondition>();
        inputSettings = GetComponent<InputSettings>();

        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.freezeRotation = true;
        }

        ConfigureMovementCollider();
        targetYaw = transform.eulerAngles.y;
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

        HandleLook();
        HandleMovementInput();
        HandleJumpAndFlyingInput();
    }

    void FixedUpdate()
    {
        if (flyingEnabled)
        {
            MoveFlying();
        }
        else
        {
            MoveGrounded();
        }
    }

    void HandleLook()
    {
        float rawMouseX = Input.GetAxisRaw("Mouse X");

        if (Mathf.Abs(rawMouseX) > 0.01f)
        {
            float mouseX = rawMouseX * mouseSensitivity * Time.deltaTime;
            targetYaw += mouseX;
        }
    }

    void HandleMovementInput()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        Vector3 localMovement = new Vector3(moveX, 0f, moveZ);
        movement = Quaternion.Euler(0f, targetYaw, 0f) * localMovement;

        if (flyingEnabled)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                movement += Vector3.up;
            }

            if (Input.GetKey(KeyCode.LeftControl))
            {
                movement += Vector3.down;
            }
        }
    }

    void HandleJumpAndFlyingInput()
    {
        if (inputSettings == null) return;

        if (Input.GetKeyDown(inputSettings.jumpKey))
        {
            if (creativeModeEnabled)
            {
                if (Time.time - lastJumpTapTime <= doubleTapTime)
                {
                    SetFlying(!flyingEnabled);
                    lastJumpTapTime = -999f;
                    return;
                }

                lastJumpTapTime = Time.time;
            }

            if (!flyingEnabled && CanJump())
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                lastJumpTime = Time.time;
            }
        }
    }

    void MoveGrounded()
    {
        rb.MoveRotation(Quaternion.Euler(0f, targetYaw, 0f));

        float currentMoveSpeed = creativeModeEnabled ? creativeMoveSpeed : baseMoveSpeed;

        if (!creativeModeEnabled && playerCondition != null)
        {
            currentMoveSpeed *= playerCondition.GetMovementSpeedMultiplier(minimumMoveSpeedMultiplier);
        }

        bool grounded = IsGrounded();
        Vector3 currentPlanarVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        bool hasMovementInput = movement.sqrMagnitude > 0.01f;

        if (!grounded)
        {
            Vector3 airTargetVelocity = hasMovementInput ? movement.normalized * currentMoveSpeed : currentPlanarVelocity;

            if (HasRecentAirborneWallContact() && Vector3.Dot(airTargetVelocity, recentAirborneWallNormal) < 0f)
            {
                airTargetVelocity = Vector3.ProjectOnPlane(airTargetVelocity, recentAirborneWallNormal);
            }

            planarVelocity = Vector3.MoveTowards(
                currentPlanarVelocity,
                airTargetVelocity,
                groundAcceleration * airControlMultiplier * Time.fixedDeltaTime
            );
            rb.linearVelocity = new Vector3(planarVelocity.x, rb.linearVelocity.y, planarVelocity.z);
            return;
        }

        if (!hasMovementInput)
        {
            planarVelocity = Vector3.MoveTowards(
                currentPlanarVelocity,
                Vector3.zero,
                groundDeceleration * Time.fixedDeltaTime
            );
            rb.linearVelocity = new Vector3(planarVelocity.x, rb.linearVelocity.y, planarVelocity.z);
            return;
        }

        Vector3 targetPlanarVelocity = movement.normalized * currentMoveSpeed;
        float acceleration = groundAcceleration;

        if (currentPlanarVelocity.sqrMagnitude > 0.01f)
        {
            float directionAlignment = Vector3.Dot(currentPlanarVelocity.normalized, targetPlanarVelocity.normalized);
            float reversalAmount = Mathf.InverseLerp(0.2f, -1f, directionAlignment);
            acceleration = Mathf.Lerp(groundAcceleration, groundDirectionChangeAcceleration, reversalAmount);
        }

        planarVelocity = Vector3.MoveTowards(
            currentPlanarVelocity,
            targetPlanarVelocity,
            acceleration * Time.fixedDeltaTime
        );
        rb.linearVelocity = new Vector3(planarVelocity.x, rb.linearVelocity.y, planarVelocity.z);
    }

    void MoveFlying()
    {
        rb.MoveRotation(Quaternion.Euler(0f, targetYaw, 0f));
        rb.linearVelocity = movement.normalized * creativeMoveSpeed;
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
    }

    bool CanJump()
    {
        if (Time.time - lastJumpTime < jumpCooldown)
        {
            return false;
        }

        if (rb != null && rb.linearVelocity.y > groundedJumpMaxUpwardVelocity)
        {
            return false;
        }

        return IsGrounded();
    }

    public void SetCanMove(bool value)
    {
        canMove = value;

        if (!value)
        {
            movement = Vector3.zero;
            planarVelocity = Vector3.zero;

            if (rb != null)
            {
                rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            }
        }
    }

    public float GetViewYaw()
    {
        return targetYaw;
    }

    public void SetCreativeMode(bool enabled)
    {
        creativeModeEnabled = enabled;

        if (!enabled)
        {
            SetFlying(false);
        }

        if (playerCondition != null)
        {
            playerCondition.passiveDecayEnabled = !enabled;
        }
    }

    public void SetCreativeSpeed(float speed)
    {
        creativeMoveSpeed = Mathf.Max(1f, speed);
    }

    void SetFlying(bool enabled)
    {
        flyingEnabled = enabled;
        rb.useGravity = !enabled;

        if (enabled)
        {
            rb.linearVelocity = Vector3.zero;
            planarVelocity = Vector3.zero;
        }
    }

    void ConfigureMovementCollider()
    {
        if (!useLowFrictionCollider)
        {
            return;
        }

        Collider playerCollider = GetComponent<Collider>();

        if (playerCollider == null)
        {
            return;
        }

        runtimeMovementMaterial = new PhysicsMaterial("Player Low Friction")
        {
            dynamicFriction = 0f,
            staticFriction = 0f,
            bounciness = 0f,
            frictionCombine = PhysicsMaterialCombine.Minimum,
            bounceCombine = PhysicsMaterialCombine.Minimum
        };
        playerCollider.material = runtimeMovementMaterial;
    }

    void OnCollisionEnter(Collision collision)
    {
        TrackAirborneWallContact(collision);
    }

    void OnCollisionStay(Collision collision)
    {
        TrackAirborneWallContact(collision);
    }

    void TrackAirborneWallContact(Collision collision)
    {
        if (flyingEnabled || IsGrounded()) return;

        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;

            if (Mathf.Abs(normal.y) <= wallNormalMaxVerticalComponent)
            {
                recentAirborneWallNormal = normal;
                airborneWallContactUntil = Time.time + Time.fixedDeltaTime * 2.1f;

                bool pressingIntoWall = movement.sqrMagnitude > 0.01f &&
                    Vector3.Dot(movement.normalized, normal) < -wallImpactInputThreshold;

                if (pressingIntoWall && rb.linearVelocity.y > 0f)
                {
                    Vector3 velocity = rb.linearVelocity;
                    velocity.y = 0f;
                    rb.linearVelocity = velocity;
                }

                return;
            }
        }
    }

    bool HasRecentAirborneWallContact()
    {
        return Time.time <= airborneWallContactUntil;
    }

    void OnDestroy()
    {
        if (runtimeMovementMaterial != null)
        {
            Destroy(runtimeMovementMaterial);
        }
    }
}
