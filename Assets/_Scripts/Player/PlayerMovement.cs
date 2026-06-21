using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using System.Collections;


public class PlayerMovement : MonoBehaviour
{
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
    [SerializeField] private float jumpForce;
    [FormerlySerializedAs("jumpColldown")]
    [SerializeField] private float jumpCooldown;
    [SerializeField] private float groundLinearDamping;

    [Header("Step Assist")]
    [SerializeField] private float stepCheckDistance = 0.35f;
    [SerializeField] private float stepMaxHeight = 0.4f;
    [SerializeField] private float stepLowerRayHeight = 0.05f;
    [SerializeField] private float stepSmoothSpeed = 6f;
    [SerializeField] private LayerMask stepMask;

    [Header("Physics")]
    [SerializeField] private bool autoConfigureRigidbody = true;

    [Header("Keybinds")]
    [SerializeField] private KeyCode jumpKey;
    [SerializeField] private KeyCode runKey;

    [Header("Ground Check")]
    [FormerlySerializedAs("playerHeigh")]
    [SerializeField] private float playerHeight;
    [SerializeField] private LayerMask whatIsGround;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Header("Interaction")]
    [SerializeField] private float rotationSpeed;

    [Header("References")]
    [SerializeField] private Transform orientation;
    [SerializeField] private Rigidbody rb;

    public bool IsInteracting { get; private set; }
    public float CurrentStamina => currentStamina;

    private float currentStamina;
    private float regenTimer;
    private bool grounded;
    private bool isRunning;
    private bool readyToJump = true;

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
        currentStamina = maxStamina;

        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.freezeRotation = true;
            if (autoConfigureRigidbody)
            {
                rb.interpolation = RigidbodyInterpolation.Interpolate;
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            }
        }

        UpdateStaminaUI();
    }

    private void FixedUpdate()
    {
        if (rb == null || orientation == null)
            return;

        MovePlayer();
        TryStepAssist();
        CheckStamina();
    }

    private void Update()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.5f, whatIsGround);

        HandleInput();
        UpdateMoveAnimationState();

        if (rb == null)
            return;

        if (grounded)
        {
            rb.linearDamping = groundLinearDamping;
        }
        else
        {
            rb.linearDamping = 2;
        }
    }

    private bool HasMovementInput()
    {
        return Mathf.Abs(horizontalInput) > 0.01f || Mathf.Abs(verticalInput) > 0.01f;
    }

    private void UpdateMoveAnimationState()
    {
        if (animator == null)
            return;

        animator.SetBool("Running", HasMovementInput());
    }

    private void HandleInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        isRunning = Input.GetKey(runKey) && CanRun() && HasMovementInput() && !IsInteracting;
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

        if (interacting)
            isRunning = false;
    }

    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (IsInteracting)
            return;

        Vector3 desiredDirection = moveDirection.sqrMagnitude > 0.001f ? moveDirection.normalized : Vector3.zero;
        float targetSpeed = isRunning ? runningSpeed : moveSpeed;
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

        if (blockedLow && !blockedHigh && lowerHit.normal.y < 0.2f)
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
        Transform playerObj = GetComponentInChildren<Transform>();
        if (playerObj != null && playerObj.name == "PlayerObj")
        {
            Vector3 direction = (target.position - playerObj.position).normalized;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.001f)
                return;

            Quaternion lookRotation = Quaternion.LookRotation(direction);
            playerObj.rotation = Quaternion.Slerp(playerObj.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }
    }

    private void Jump()
    {
        if (rb == null)
            return;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }
}
