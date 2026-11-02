using UnityEngine;

// Sobe degraus baixos (meio-fio, escada, calçada) sem o player travar no canto do collider.
// Não roda sozinho: o PlayerMovement chama TryStepUp no FixedUpdate quando o player está no chão e quer andar.
//
// Detecção, sempre medida a partir do PÉ (fundo do collider):
//   1. Raio baixo para frente     → tem algo na frente do pé?
//   2. Raio alto para frente      → esse algo é mais alto que um degrau? Então é parede.
//   3. Raio de cima para baixo    → onde fica o topo do degrau e ele é pisável?
//   4. Raio para cima             → tem teto que impede de subir?
[RequireComponent(typeof(Rigidbody))]
public class PlayerStepAssist : MonoBehaviour
{
    // normal.y abaixo disso = face quase vertical (frente de um degrau).
    private const float MaxStepFaceNormalY = 0.2f;
    // normal.y acima disso = topo plano o bastante para pisar (~45°).
    private const float MinWalkableNormalY = 0.7f;
    // Degraus menores que isso são ruído de colisão; a física resolve sozinha.
    private const float MinStepHeight = 0.02f;
    // Quanto o raio vertical entra além da borda do degrau, para não acertar exatamente a quina.
    private const float StepTopInset = 0.05f;

    [Tooltip("Altura máxima de degrau que o player sobe sozinho")]
    [SerializeField] private float stepMaxHeight = 0.4f;
    [Tooltip("Quanto à frente do corpo procurar degraus")]
    [SerializeField] private float stepCheckDistance = 0.35f;
    [Tooltip("Altura do raio baixo acima do pé (acima de zero para não raspar no próprio chão)")]
    [SerializeField] private float stepLowerRayHeight = 0.05f;
    [Tooltip("Velocidade de subida em metros por segundo; maior = mais seco, menor = mais suave")]
    [SerializeField] private float stepSmoothSpeed = 6f;
    [Tooltip("Espaço livre exigido acima da cabeça para subir")]
    [SerializeField] private float stepClearance = 0.1f;
    [Tooltip("Layers que podem ser subidas como degrau")]
    [SerializeField] private LayerMask stepMask;

    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [Tooltip("Collider do corpo; o pé é o fundo dele. Vazio = primeiro Collider do objeto")]
    [SerializeField] private Collider bodyCollider;

    public float MaxStepHeight => stepMaxHeight;

    // Guardado só para os Gizmos mostrarem a última checagem.
    private Vector3 lastDirection = Vector3.forward;

    private void Reset()
    {
        stepMask = LayerMask.GetMask("whatIsGround");
    }

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();
        if (bodyCollider == null)
            bodyCollider = GetComponent<Collider>();
    }

    // Retorna true se subiu um pedaço do degrau neste passo de física.
    public bool TryStepUp(Vector3 moveDirection)
    {
        if (bodyCollider == null)
            return false;

        lastDirection = moveDirection;
        Bounds bounds = bodyCollider.bounds;

        if (!TryGetStepHeight(bounds, moveDirection, out float stepHeight))
            return false;

        if (!HasHeadroom(bounds, stepHeight))
            return false;

        ClimbStep(stepHeight);
        return true;
    }

    private bool TryGetStepHeight(Bounds bounds, Vector3 direction, out float stepHeight)
    {
        stepHeight = 0f;

        float feetY = bounds.min.y;
        Vector3 center = new Vector3(bounds.center.x, feetY, bounds.center.z);
        float reach = GetHalfWidth(bounds, direction) + stepCheckDistance;

        // 1. Algo na altura do pé, com face vertical?
        Vector3 lowerOrigin = center + Vector3.up * stepLowerRayHeight;
        if (!Physics.Raycast(lowerOrigin, direction, out RaycastHit faceHit, reach, stepMask, QueryTriggerInteraction.Ignore))
            return false;
        if (faceHit.normal.y > MaxStepFaceNormalY)
            return false; // É rampa; o Rigidbody sobe sozinho.

        // 2. Se também bloqueia acima da altura máxima, é parede.
        Vector3 upperOrigin = center + Vector3.up * (stepMaxHeight + stepClearance);
        if (Physics.Raycast(upperOrigin, direction, reach, stepMask, QueryTriggerInteraction.Ignore))
            return false;

        // 3. Desce um raio logo depois da borda para achar o topo do degrau.
        Vector3 topProbe = faceHit.point + direction * StepTopInset;
        topProbe.y = feetY + stepMaxHeight + stepClearance;
        float probeLength = stepMaxHeight + stepClearance;
        if (!Physics.Raycast(topProbe, Vector3.down, out RaycastHit topHit, probeLength, stepMask, QueryTriggerInteraction.Ignore))
            return false;
        if (topHit.normal.y < MinWalkableNormalY)
            return false;

        stepHeight = topHit.point.y - feetY;
        return stepHeight > MinStepHeight && stepHeight <= stepMaxHeight;
    }

    // 4. A cabeça cabe depois de subir?
    private bool HasHeadroom(Bounds bounds, float stepHeight)
    {
        Vector3 head = new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
        return !Physics.Raycast(head, Vector3.up, stepHeight + stepClearance, stepMask, QueryTriggerInteraction.Ignore);
    }

    // Sobe no máximo stepSmoothSpeed·dt por passo: o degrau leva alguns frames, em vez de um teleporte.
    private void ClimbStep(float stepHeight)
    {
        float rise = Mathf.Min(stepHeight, stepSmoothSpeed * Time.fixedDeltaTime);
        rb.MovePosition(rb.position + Vector3.up * rise);

        // Cancela a queda acumulada, senão a gravidade puxa de volta para baixo do degrau.
        Vector3 velocity = rb.linearVelocity;
        if (velocity.y < 0f)
        {
            velocity.y = 0f;
            rb.linearVelocity = velocity;
        }
    }

    // Distância do centro até a borda do collider na direção do movimento (aproximação pelos bounds).
    private static float GetHalfWidth(Bounds bounds, Vector3 direction)
    {
        return Mathf.Abs(direction.x) * bounds.extents.x + Mathf.Abs(direction.z) * bounds.extents.z;
    }

    private void OnDrawGizmosSelected()
    {
        Collider body = bodyCollider != null ? bodyCollider : GetComponent<Collider>();
        if (body == null)
            return;

        Bounds bounds = body.bounds;
        Vector3 feet = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
        float reach = GetHalfWidth(bounds, lastDirection) + stepCheckDistance;

        GizmosUtils.DrawRay(feet + Vector3.up * stepLowerRayHeight, lastDirection, reach, Color.yellow);
        GizmosUtils.DrawRay(feet + Vector3.up * (stepMaxHeight + stepClearance), lastDirection, reach, Color.magenta);
    }
}
