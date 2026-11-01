using System;
using UnityEngine;
using UnityEngine.Serialization;

// Locomoção do player (andar, correr, pular, gravidade e virar o corpo) sobre um Rigidbody.
// Responsabilidades vizinhas ficam em outros componentes:
//   GameInput                  → lê teclado/controle
//   PlayerStamina              → gasta e recupera stamina da corrida
//   PlayerStepAssist           → sobe degraus baixos
//   SpriteDirectionalController → animação, lida a partir da velocidade do Rigidbody
[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    private const string PlayerObjName = "PlayerObj";
    // Folga para o SphereCast do chão não começar encostado no piso (cast que começa sobreposto não detecta nada).
    private const float GroundProbeSkin = 0.05f;
    // Folga extra do cast de parede, para zerar a velocidade um pouco antes do contato físico.
    private const float WallCheckSkin = 0.02f;
    private const float MinInputSqr = 0.0001f;

    public static PlayerMovement Instance { get; private set; }

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float runningSpeed = 14f;
    [Tooltip("Fração da velocidade alvo no ar (1 = mesmo controle do chão)")]
    [SerializeField] private float airMultiplier = 0.4f;
    [SerializeField] private float groundAcceleration = 40f;
    [SerializeField] private float airAcceleration = 4f;

    [Header("Jump")]
    [FormerlySerializedAs("jumpForce")]
    [Tooltip("Altura do pulo em metros; a velocidade inicial é calculada a partir dela")]
    [SerializeField] private float jumpHeight = 1f;
    [FormerlySerializedAs("jumpColldown")]
    [SerializeField] private float jumpCooldown = 0.25f;
    [Tooltip("Tempo depois de sair do chão em que o pulo ainda é aceito")]
    [SerializeField] private float coyoteTime = 0.12f;
    [Tooltip("Tempo antes de tocar o chão em que um pulo apertado fica guardado")]
    [SerializeField] private float jumpBufferTime = 0.12f;

    [Header("Gravity")]
    [SerializeField] private float fallGravityMultiplier = 2.5f;
    [Tooltip("Gravidade extra quando o player solta o pulo cedo (pulo curto)")]
    [SerializeField] private float lowJumpGravityMultiplier = 2f;

    [Header("Ground Check")]
    [Tooltip("Deixe um pouco menor que a metade da largura do collider: raio pequeno faz o player 'flutuar' preso na beirada de degraus")]
    [SerializeField] private float groundCheckRadius = 0.4f;
    [Tooltip("Distância abaixo do pé que ainda conta como chão")]
    [SerializeField] private float groundProbeDistance = 0.15f;
    [SerializeField] private LayerMask whatIsGround;

    [Header("Walls")]
    [Tooltip("Raio do cast que impede empurrar a velocidade contra paredes. Deixe perto da metade da largura do collider")]
    [SerializeField] private float playerRadius = 0.45f;
    [SerializeField] private LayerMask collisionsLayerMask;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 5f;

    [Header("References")]
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform playerObj;
    [SerializeField] private Rigidbody rb;
    [Tooltip("Collider do corpo; o pé do player é o fundo dele. Vazio = primeiro Collider do objeto")]
    [SerializeField] private Collider bodyCollider;
    [Tooltip("Opcional: sem ele o player corre sem gastar stamina")]
    [SerializeField] private PlayerStamina stamina;
    [Tooltip("Opcional: sem ele o player não sobe degraus sozinho")]
    [SerializeField] private PlayerStepAssist stepAssist;

    [Header("Setup")]
    [SerializeField] private bool autoConfigureRigidbody = true;

    public bool IsInteracting { get; private set; }
    public bool IsGrounded { get; private set; }
    public bool IsRunning { get; private set; }
    public float MoveSpeed => moveSpeed;

    // Estado de input (escrito pelo GameInput, lido no FixedUpdate)
    private Vector2 moveInput;
    private bool wantsToRun;
    private bool isJumpHeld;
    private float lastJumpPressedTime = float.NegativeInfinity;

    // Estado de física
    private float lastGroundedTime = float.NegativeInfinity;
    private float nextJumpAllowedTime;
    private Vector3 desiredDirection;
    private Transform lookTarget;

    #region Unity lifecycle

    private void Awake()
    {
        Instance = this;

        if (rb == null)
            rb = GetComponent<Rigidbody>();
        if (bodyCollider == null)
            bodyCollider = GetComponent<Collider>();
        if (stamina == null)
            stamina = GetComponent<PlayerStamina>();
        if (stepAssist == null)
            stepAssist = GetComponent<PlayerStepAssist>();
        if (playerObj == null)
            playerObj = FindPlayerObjFallback();

        ConfigureRigidbody();
    }

    private void Start()
    {
        GameInput.Instance.OnJumpPressed += GameInput_OnJumpPressed;
        GameInput.Instance.OnJumpHeldChanged += GameInput_OnJumpHeldChanged;
        GameInput.Instance.OnSprintStateChanged += GameInput_OnSprintStateChanged;
    }

    private void OnDestroy()
    {
        if (GameInput.Instance == null)
            return;

        GameInput.Instance.OnJumpPressed -= GameInput_OnJumpPressed;
        GameInput.Instance.OnJumpHeldChanged -= GameInput_OnJumpHeldChanged;
        GameInput.Instance.OnSprintStateChanged -= GameInput_OnSprintStateChanged;
    }

    private void Update()
    {
        moveInput = GameInput.Instance.GetMovementVectorNormalized();
    }

    // Toda a física roda aqui, em passo fixo. A ordem importa: cada etapa usa o resultado da anterior.
    private void FixedUpdate()
    {
        if (orientation == null || bodyCollider == null)
            return;

        UpdateGroundCheck();
        UpdateRunningState();
        HandleJump();
        MoveHorizontally();
        HandleStepAssist();
        ApplyExtraGravity();
        UpdateFacing();

        if (stamina != null)
            stamina.Tick(IsRunning, Time.fixedDeltaTime);
    }

    #endregion

    #region Public API

    public void SetInteracting(bool interacting)
    {
        IsInteracting = interacting;

        if (!interacting)
            lookTarget = null;
    }

    // Vira o corpo para o alvo ao longo dos próximos frames, até SetInteracting(false).
    public void RotateTowardsTarget(Transform target)
    {
        lookTarget = target;
    }

    #endregion

    #region Input callbacks

    private void GameInput_OnJumpPressed(object sender, EventArgs e)
    {
        lastJumpPressedTime = Time.time;
    }

    private void GameInput_OnJumpHeldChanged(object sender, bool held)
    {
        isJumpHeld = held;
    }

    private void GameInput_OnSprintStateChanged(object sender, bool pressed)
    {
        wantsToRun = pressed;
    }

    #endregion

    #region Physics steps

    private void UpdateGroundCheck()
    {
        Vector3 origin = GetFeetPosition() + Vector3.up * (groundCheckRadius + GroundProbeSkin);

        IsGrounded = Physics.SphereCast(
            origin,
            groundCheckRadius,
            Vector3.down,
            out _,
            GroundProbeSkin + groundProbeDistance,
            whatIsGround,
            QueryTriggerInteraction.Ignore);

        if (IsGrounded)
            lastGroundedTime = Time.time;
    }

    private void UpdateRunningState()
    {
        bool hasStamina = stamina == null || stamina.CanSprint(IsRunning);
        IsRunning = wantsToRun && HasMovementInput() && CanMove() && hasStamina;
    }

    private void HandleJump()
    {
        bool jumpBuffered = Time.time - lastJumpPressedTime <= jumpBufferTime;
        bool withinCoyoteTime = Time.time - lastGroundedTime <= coyoteTime;
        bool cooldownDone = Time.time >= nextJumpAllowedTime;

        if (!jumpBuffered || !withinCoyoteTime || !cooldownDone || !CanMove())
            return;

        // Consome o buffer e o coyote time para um único aperto não gerar dois pulos.
        lastJumpPressedTime = float.NegativeInfinity;
        lastGroundedTime = float.NegativeInfinity;
        nextJumpAllowedTime = Time.time + jumpCooldown;

        // v = √(2·g·h): velocidade inicial exata para atingir jumpHeight.
        float jumpVelocity = Mathf.Sqrt(2f * jumpHeight * Mathf.Abs(Physics.gravity.y));
        Vector3 velocity = rb.linearVelocity;
        velocity.y = jumpVelocity;
        rb.linearVelocity = velocity;
    }

    private void MoveHorizontally()
    {
        float targetSpeed = IsRunning ? runningSpeed : moveSpeed;

        // Sem movimento permitido o alvo vira zero, então o player desacelera em vez de deslizar com a velocidade antiga.
        desiredDirection = CanMove() ? GetCameraRelativeInput() : Vector3.zero;
        Vector3 allowedDirection = SlideAlongWalls(desiredDirection, targetSpeed * Time.fixedDeltaTime);

        Vector3 targetVelocity = allowedDirection * targetSpeed;
        if (!IsGrounded)
            targetVelocity *= airMultiplier;

        Vector3 velocity = rb.linearVelocity;
        Vector3 horizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);
        float acceleration = IsGrounded ? groundAcceleration : airAcceleration;
        horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);

        rb.linearVelocity = new Vector3(horizontalVelocity.x, velocity.y, horizontalVelocity.z);
    }

    private void HandleStepAssist()
    {
        // Usa a direção desejada, não a velocidade: encostado no degrau a velocidade já é zero.
        bool isRising = rb.linearVelocity.y > 0.1f;
        if (stepAssist == null || !IsGrounded || isRising || desiredDirection.sqrMagnitude < MinInputSqr)
            return;

        stepAssist.TryStepUp(desiredDirection);
    }

    private void ApplyExtraGravity()
    {
        if (IsGrounded)
            return;

        // O Rigidbody já aplica 1x a gravidade; aqui só somamos o excedente.
        float multiplier = rb.linearVelocity.y < 0f
            ? fallGravityMultiplier
            : (isJumpHeld ? 1f : lowJumpGravityMultiplier);

        rb.AddForce(Physics.gravity * (multiplier - 1f), ForceMode.Acceleration);
    }

    private void UpdateFacing()
    {
        if (playerObj == null)
            return;

        Vector3 facing = desiredDirection;
        if (IsInteracting && lookTarget != null)
        {
            facing = lookTarget.position - playerObj.position;
            facing.y = 0f;
        }

        if (facing.sqrMagnitude < MinInputSqr)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(facing.normalized, Vector3.up);
        Quaternion newRotation = Quaternion.Slerp(playerObj.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);

        // Se o objeto visual é o próprio Rigidbody, gira pela física para não brigar com a interpolação.
        if (playerObj == rb.transform)
            rb.MoveRotation(newRotation);
        else
            playerObj.rotation = newRotation;
    }

    #endregion

    #region Helpers

    private Vector3 GetCameraRelativeInput()
    {
        Vector3 direction = orientation.forward * moveInput.y + orientation.right * moveInput.x;
        direction.y = 0f;
        return direction.sqrMagnitude > MinInputSqr ? direction.normalized : Vector3.zero;
    }

    // Collide-and-slide: se a direção bate numa parede, desliza ao longo dela (como o CharacterController faz).
    // Empurrar a velocidade do Rigidbody contra a parede faria o player "grudar" nela pelo atrito.
    private Vector3 SlideAlongWalls(Vector3 direction, float moveDistance)
    {
        if (direction.sqrMagnitude < MinInputSqr)
            return Vector3.zero;

        if (!CastBody(direction, moveDistance, out RaycastHit hit))
            return direction;

        Vector3 wallNormal = new Vector3(hit.normal.x, 0f, hit.normal.z);
        if (wallNormal.sqrMagnitude < MinInputSqr)
            return Vector3.zero;

        // Batendo de frente o resultado é ~zero; de raspão, sobra quase toda a velocidade.
        Vector3 slide = Vector3.ProjectOnPlane(direction, wallNormal.normalized);
        if (slide.sqrMagnitude < MinInputSqr || CastBody(slide.normalized, moveDistance, out _))
            return Vector3.zero; // Quina: a direção de deslize também está bloqueada.

        return slide;
    }

    // Cápsula do tamanho do corpo, começando ACIMA da altura de degrau:
    // obstáculos baixos não contam como parede, eles são problema do StepAssist.
    private bool CastBody(Vector3 direction, float distance, out RaycastHit hit)
    {
        Bounds bounds = bodyCollider.bounds;
        float stepHeight = stepAssist != null ? stepAssist.MaxStepHeight : 0f;

        float bottomY = bounds.min.y + stepHeight + playerRadius;
        float topY = Mathf.Max(bottomY, bounds.max.y - playerRadius);
        Vector3 bottom = new Vector3(bounds.center.x, bottomY, bounds.center.z);
        Vector3 top = new Vector3(bounds.center.x, topY, bounds.center.z);

        return Physics.CapsuleCast(
            bottom,
            top,
            playerRadius,
            direction,
            out hit,
            distance + WallCheckSkin,
            collisionsLayerMask,
            QueryTriggerInteraction.Ignore);
    }

    // O pivô do player fica no centro do corpo; o pé é o fundo do collider.
    private Vector3 GetFeetPosition()
    {
        Bounds bounds = bodyCollider.bounds;
        return new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
    }

    private bool HasMovementInput()
    {
        return moveInput.sqrMagnitude > MinInputSqr;
    }

    // Diálogo, pause (timeScale 0) e game over bloqueiam o movimento por caminhos diferentes; centraliza a regra aqui.
    private bool CanMove()
    {
        return !IsInteracting && GameManager.Instance.IsGamePlaying();
    }

    private void ConfigureRigidbody()
    {
        rb.freezeRotation = true;
        // MoveTowards já controla aceleração e frenagem; linearDamping brigaria com ele e limitaria a velocidade real.
        rb.linearDamping = 0f;

        if (autoConfigureRigidbody)
        {
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        // A velocidade já é toda controlada pelo código; atrito só serve para o corpo enroscar em quinas e beiradas.
        if (bodyCollider != null && bodyCollider.sharedMaterial == null)
        {
            bodyCollider.sharedMaterial = new PhysicsMaterial("PlayerNoFriction")
            {
                dynamicFriction = 0f,
                staticFriction = 0f,
                frictionCombine = PhysicsMaterialCombine.Minimum,
            };
        }
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

    #endregion

    private void OnDrawGizmosSelected()
    {
        Collider body = bodyCollider != null ? bodyCollider : GetComponent<Collider>();
        if (body == null)
            return;

        Bounds bounds = body.bounds;
        Vector3 feet = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);

        GizmosUtils.DrawSphereCast(
            feet + Vector3.up * (groundCheckRadius + GroundProbeSkin),
            groundCheckRadius,
            Vector3.down,
            GroundProbeSkin + groundProbeDistance,
            IsGrounded ? Color.green : Color.red);

        // Cápsula de parede: tudo abaixo dela é tratado como degrau.
        PlayerStepAssist step = stepAssist != null ? stepAssist : GetComponent<PlayerStepAssist>();
        float stepHeight = step != null ? step.MaxStepHeight : 0f;
        float bottomY = bounds.min.y + stepHeight + playerRadius;
        float topY = Mathf.Max(bottomY, bounds.max.y - playerRadius);
        GizmosUtils.DrawWireSphere(new Vector3(feet.x, bottomY, feet.z), playerRadius, Color.cyan);
        GizmosUtils.DrawWireSphere(new Vector3(feet.x, topY, feet.z), playerRadius, Color.cyan);
    }
}
