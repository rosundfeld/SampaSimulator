
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using System.Collections;
using System;


public class PlayerMovement : MonoBehaviour
{
    private const string PlayerObjName = "PlayerObj";

    public static PlayerMovement Instance { get; private set; }

    [Header("Stats")]
    [SerializeField] private float maxStamina;
    [SerializeField] private float staminaDrainRate;
    [SerializeField] private float staminaRegenRate;
    [SerializeField] private float minStaminaToRun;
    [SerializeField] private float regenDelay;

    [Header("UI")]
    [SerializeField] private Image staminaBar;

    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float runningSpeed;
    [SerializeField] private float airMultiplier;
    [SerializeField] private float groundAcceleration = 40f;
    [SerializeField] private float airAcceleration = 16f;
    [FormerlySerializedAs("jumpForce")]
    [SerializeField] private float jumpHeight = 1.2f;
    [FormerlySerializedAs("jumpColldown")]
    [SerializeField] private float jumpCooldown;

    [Header("Jump Feel")]
    [SerializeField] private float coyoteTime = 0.12f;
    [SerializeField] private float jumpBufferTime = 0.12f;

    [Header("Gravity")]
    [SerializeField] private float fallGravityMultiplier = 2.5f;
    [SerializeField] private float lowJumpGravityMultiplier = 2f;

    [Header("Step Assist")]
    [SerializeField] private float stepCheckDistance = 0.35f;
    [SerializeField] private float stepMaxHeight = 0.4f;
    [SerializeField] private float stepLowerRayHeight = 0.05f;
    [SerializeField] private float stepSmoothSpeed = 6f;
    [SerializeField] private float stepClearance = 0.1f;
    [SerializeField] private LayerMask stepMask;

    [Header("Physics")]
    [SerializeField] private bool autoConfigureRigidbody = true;
    [SerializeField] private float playerRadius = 0.4f;
    [SerializeField] private LayerMask collisionsLayerMask;

    [Header("Ground Check")]
    [FormerlySerializedAs("playerHeigh")]
    [SerializeField] private float playerHeight;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private float groundCheckDistance = 0.5f;
    [SerializeField] private LayerMask whatIsGround;

    [Header("Interaction")]
    [SerializeField] private float rotationSpeed;

    [Header("References")]
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform playerObj;
    [SerializeField] private Rigidbody rb;

    public bool IsInteracting { get; private set; }
    public float CurrentStamina => currentStamina;
    public float MoveSpeed => moveSpeed;

    private float currentStamina;
    private float regenTimer;
    private bool grounded;
    private bool isRunning;
    private bool wantsToRun;
    private bool isJumpHeld;
    private bool readyToJump = true;
    private float lastGroundedTime;
    private float lastJumpPressedTime = float.NegativeInfinity;

    private float horizontalInput;
    private float verticalInput;
    private Vector3 moveDirection;

    private Coroutine hideCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        GameInput.Instance.OnJumpPressed += GameInput_OnJumpPressed;
        GameInput.Instance.OnJumpHeldChanged += GameInput_OnJumpHeldChanged;
        GameInput.Instance.OnSprintStateChanged += GameInput_OnSprintStateChanged;

        currentStamina = maxStamina;

        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.freezeRotation = true;
            // MoveTowards already fully drives accel/decel; linearDamping would fight it and cap real speed below the configured target.
            rb.linearDamping = 0f;
            if (autoConfigureRigidbody)
            {
                rb.interpolation = RigidbodyInterpolation.Interpolate;
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            }
        }

        if (playerObj == null)
            playerObj = FindPlayerObjFallback();

        UpdateStaminaUI();
    }

    private Transform FindPlayerObjFallback()
    {
        foreach (Transform child in GetComponentsInChildren<Transform>(true))
        {
            if (child.name == PlayerObjName)
                return child;
        }

        Debug.LogWarning($"PlayerMovement: nenhum filho chamado '{PlayerObjName}' encontrado. Atribua 'Player Obj' no Inspector.", this);
        return null;
    }

    private void FixedUpdate()
    {
        if (rb == null || orientation == null)
            return;

        UpdateGroundCheck();
        UpdateRunningState();
        HandleJump();
        MovePlayer();
        ApplyExtraGravity();
        TryStepAssist();
        CheckStamina();
    }

    private void Update()
    {
        HandleInput();
    }

    private void UpdateGroundCheck()
    {
        grounded = Physics.SphereCast(
            transform.position + Vector3.up * groundCheckRadius,
            groundCheckRadius,
            Vector3.down,
            out _,
            playerHeight * 0.5f + groundCheckDistance,
            whatIsGround,
            QueryTriggerInteraction.Ignore);

        if (grounded)
            lastGroundedTime = Time.time;
    }

    private void UpdateRunningState()
    {
        bool canStartOrContinueRunning = isRunning ? currentStamina > 0f : CanRun();
        isRunning = wantsToRun && HasMovementInput() && !IsInteracting && canStartOrContinueRunning;
    }

    private void ApplyExtraGravity()
    {
        if (grounded)
            return;

        float multiplier = rb.linearVelocity.y < 0f
            ? fallGravityMultiplier
            : (isJumpHeld ? 1f : lowJumpGravityMultiplier);

        rb.AddForce(Physics.gravity * (multiplier - 1f), ForceMode.Acceleration);
    }

    private void HandleJump()
    {
        bool canJump = readyToJump
            && grounded
            && Time.time - lastGroundedTime <= coyoteTime
            && Time.time - lastJumpPressedTime <= jumpBufferTime;

        if (!canJump)
            return;

        readyToJump = false;
        lastJumpPressedTime = float.NegativeInfinity;
        Jump();
        Invoke(nameof(ResetJump), jumpCooldown);
    }

    private bool HasMovementInput()
    {
        return Mathf.Abs(horizontalInput) > 0.01f || Mathf.Abs(verticalInput) > 0.01f;
    }

    private void HandleInput()
    {
        Vector2 movement = GameInput.Instance.GetMovementVectorNormalized();
        horizontalInput = movement.x;
        verticalInput = movement.y;
    }

    private void GameInput_OnSprintStateChanged(object sender, bool pressed)
    {
        wantsToRun = pressed;
    }

    private void GameInput_OnJumpPressed(object sender, EventArgs e)
    {
        lastJumpPressedTime = Time.time;
    }

    private void GameInput_OnJumpHeldChanged(object sender, bool held)
    {
        isJumpHeld = held;
    }

    private IEnumerator WaitToHideStamina()
    {
        yield return new WaitForSeconds(5f);

        if (staminaBar != null)
            staminaBar.gameObject.SetActive(false);

        hideCoroutine = null;
    }

    private void CheckStamina()
    {
        if (staminaBar == null)
            return;

        if (isRunning)
        {
            if (hideCoroutine != null)
            {
                StopCoroutine(hideCoroutine);
                hideCoroutine = null;
            }

            staminaBar.gameObject.SetActive(true);
            DrainStamina();
        }
        else
        {
            if (hideCoroutine == null)
            {
                hideCoroutine = StartCoroutine(WaitToHideStamina());
            }

            RegenStamina();
        }

        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        UpdateStaminaUI();
    }

    private void DrainStamina()
    {
        currentStamina -= staminaDrainRate * Time.deltaTime;
        regenTimer = 0f;
    }

    private void RegenStamina()
    {
        if (currentStamina < maxStamina)
        {
            regenTimer += Time.deltaTime;
            if (regenTimer >= regenDelay)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
            }
        }
    }

    public bool CanRun()
    {
        return currentStamina > minStaminaToRun;
    }

    public void SetInteracting(bool interacting)
    {
        IsInteracting = interacting;
    }

    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (IsInteracting)
            return;

        Vector3 desiredDirection = moveDirection.sqrMagnitude > 0.001f ? moveDirection.normalized : Vector3.zero;
        float targetSpeed = isRunning ? runningSpeed : moveSpeed;
        desiredDirection = ResolveMoveDirection(desiredDirection, targetSpeed);
        Vector3 targetHorizontalVelocity = desiredDirection * targetSpeed;

        if (!grounded)
            targetHorizontalVelocity *= airMultiplier;

        Vector3 currentHorizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        float acceleration = grounded ? groundAcceleration : airAcceleration;
        Vector3 smoothedHorizontalVelocity = Vector3.MoveTowards(
            currentHorizontalVelocity,
            targetHorizontalVelocity,
            acceleration * Time.fixedDeltaTime);

        rb.linearVelocity = new Vector3(smoothedHorizontalVelocity.x, rb.linearVelocity.y, smoothedHorizontalVelocity.z);

        RotatePlayerObjTowardsMovement(desiredDirection);
    }

    // Prevents pushing Rigidbody velocity into a blocked surface; falls back to sliding along a single axis, like a CharacterController would.
    private Vector3 ResolveMoveDirection(Vector3 desiredDirection, float speed)
    {
        if (desiredDirection.sqrMagnitude < 0.0001f)
            return desiredDirection;

        float moveDistance = speed * Time.fixedDeltaTime;

        if (CanMoveInDirection(desiredDirection, moveDistance))
            return desiredDirection;

        Vector3 directionX = new Vector3(desiredDirection.x, 0f, 0f).normalized;
        if (Mathf.Abs(desiredDirection.x) > 0.5f && CanMoveInDirection(directionX, moveDistance))
            return directionX;

        Vector3 directionZ = new Vector3(0f, 0f, desiredDirection.z).normalized;
        if (Mathf.Abs(desiredDirection.z) > 0.5f && CanMoveInDirection(directionZ, moveDistance))
            return directionZ;

        return Vector3.zero;
    }

    private bool CanMoveInDirection(Vector3 direction, float distance)
    {
        return !Physics.BoxCast(
            transform.position,
            Vector3.one * playerRadius,
            direction,
            Quaternion.identity,
            distance,
            collisionsLayerMask,
            QueryTriggerInteraction.Ignore);
    }

    private void RotatePlayerObjTowardsMovement(Vector3 desiredDirection)
    {
        if (playerObj == null || desiredDirection.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(desiredDirection, Vector3.up);
        playerObj.rotation = Quaternion.Slerp(playerObj.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
    }

    private void TryStepAssist()
    {
        if (!grounded)
            return;

        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        if (horizontalVelocity.sqrMagnitude < 0.01f)
            return;

        Vector3 stepDirection = horizontalVelocity.normalized;
        Vector3 lowerOrigin = transform.position + Vector3.up * stepLowerRayHeight;
        Vector3 upperOrigin = transform.position + Vector3.up * stepMaxHeight;

        int mask = stepMask == 0 ? whatIsGround : stepMask;

        bool blockedLow = Physics.Raycast(lowerOrigin, stepDirection, out RaycastHit lowerHit, stepCheckDistance, mask, QueryTriggerInteraction.Ignore);
        bool blockedHigh = Physics.Raycast(upperOrigin, stepDirection, stepCheckDistance, mask, QueryTriggerInteraction.Ignore);
        bool ceilingBlocked = Physics.Raycast(transform.position, Vector3.up, stepMaxHeight + stepClearance, mask, QueryTriggerInteraction.Ignore);

        if (blockedLow && !blockedHigh && !ceilingBlocked && lowerHit.normal.y < 0.2f)
        {
            Vector3 stepOffset = Vector3.up * (stepSmoothSpeed * Time.fixedDeltaTime);
            rb.MovePosition(rb.position + stepOffset);
        }
    }

    private void UpdateStaminaUI()
    {
        if (staminaBar != null && maxStamina > 0f)
            staminaBar.fillAmount = currentStamina / maxStamina;
    }

    public void RotateTowardsTarget(Transform target)
    {
        if (playerObj == null)
            return;

        Vector3 direction = target.position - playerObj.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion lookRotation = Quaternion.LookRotation(direction);
        playerObj.rotation = Quaternion.Slerp(playerObj.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

    private void Jump()
    {
        if (rb == null)
            return;

        float jumpVelocity = Mathf.Sqrt(2f * jumpHeight * Mathf.Abs(Physics.gravity.y));
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpVelocity, rb.linearVelocity.z);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    private void OnDrawGizmosSelected()
    {
        // Box used by ResolveMoveDirection's BoxCast; tune Player Radius until it matches the character's real footprint.
        GizmosUtils.DrawWireBox(transform.position, Vector3.one * playerRadius * 2f, Color.cyan);

        GizmosUtils.DrawSphereCast(
            transform.position + Vector3.up * groundCheckRadius,
            groundCheckRadius,
            Vector3.down,
            playerHeight * 0.5f + groundCheckDistance,
            grounded ? Color.green : Color.red);

        Vector3 lowerOrigin = transform.position + Vector3.up * stepLowerRayHeight;
        Vector3 upperOrigin = transform.position + Vector3.up * stepMaxHeight;
        GizmosUtils.DrawRay(lowerOrigin, transform.forward, stepCheckDistance, Color.yellow);
        GizmosUtils.DrawRay(upperOrigin, transform.forward, stepCheckDistance, Color.magenta);
    }
}

