using UnityEngine;

// Visual do player: escolhe a animação direcional (relativa à câmera) a partir da velocidade do Rigidbody.
public class SpriteDirectionalController : MonoBehaviour
{
    private const string RUNNING = "Running";
    private const string MOVE_X = "MoveX";
    private const string MOVE_Y = "MoveY";

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody playerRb;
    [SerializeField] private float moveThreshold = 0.05f;
    [SerializeField] private float dampTime = 0.1f;
    [SerializeField] private float minAnimatorSpeed = 0.5f;
    [SerializeField] private float maxAnimatorSpeed = 2.5f;

    private Vector2 lastMoveDir = new Vector2(0f, -1f); // Começa virado para a câmera

    private void LateUpdate()
    {
        if (Camera.main == null)
            return;

        Vector3 velocity = playerRb.linearVelocity;
        velocity.y = 0f;
        bool isMoving = velocity.sqrMagnitude >= moveThreshold * moveThreshold;
        animator.SetBool(RUNNING, isMoving);

        // Sem isso a animação toca sempre no mesmo ritmo, independente da velocidade real (walk == sprint visualmente).
        float moveSpeed = PlayerMovement.Instance.MoveSpeed;
        animator.speed = isMoving && moveSpeed > 0.01f
            ? Mathf.Clamp(velocity.magnitude / moveSpeed, minAnimatorSpeed, maxAnimatorSpeed)
            : 1f;

        if (isMoving)
            lastMoveDir = GetCameraRelativeDirection(velocity.normalized);

        animator.SetFloat(MOVE_X, lastMoveDir.x, dampTime, Time.deltaTime);
        animator.SetFloat(MOVE_Y, lastMoveDir.y, dampTime, Time.deltaTime);

        if (isMoving && Mathf.Abs(lastMoveDir.x) > 0.1f)
            spriteRenderer.flipX = lastMoveDir.x < 0f;
    }

    private Vector2 GetCameraRelativeDirection(Vector3 worldDirection)
    {
        Vector3 cameraForward = Camera.main.transform.forward;
        cameraForward.y = 0f;
        cameraForward.Normalize();

        Vector3 cameraRight = Camera.main.transform.right;
        cameraRight.y = 0f;
        cameraRight.Normalize();

        return new Vector2(Vector3.Dot(worldDirection, cameraRight), Vector3.Dot(worldDirection, cameraForward));
    }
}
