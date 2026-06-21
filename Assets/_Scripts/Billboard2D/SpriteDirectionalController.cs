using UnityEngine;

public class SpriteDirectionalController : MonoBehaviour
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Animator animator;
    [SerializeField] Rigidbody playerRb;
    [SerializeField] float moveThreshold = 0.05f;
    [SerializeField] float dampTime = 0.1f;


    private Vector2 lastMoveDir = new Vector2(0f, -1f); // direção inicial (ajuste se qu
    void LateUpdate()
    {
        if (Camera.main == null || playerRb == null) return;

        //SpriteDirectional();

        Vector3 vel = playerRb.linearVelocity;
        vel.y = 0f;
        bool isMoving = vel.sqrMagnitude >= moveThreshold * moveThreshold;
        animator.SetBool("Running", isMoving);

        if (!isMoving)
        {
                animator.SetFloat("MoveX", lastMoveDir.x, dampTime, Time.deltaTime);
        animator.SetFloat("MoveY", lastMoveDir.y, dampTime, Time.deltaTime);
            return;
        }

        Vector3 dir = vel.normalized;

        Vector3 camForward = Camera.main.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = Camera.main.transform.right;
        camRight.y = 0f;
        camRight.Normalize();

        float moveX = Vector3.Dot(dir, camRight);
        float moveY = Vector3.Dot(dir, camForward);

        lastMoveDir = new Vector2(moveX, moveY);

        // Se W estiver tocando animação invertida, troque por -moveY
        animator.SetFloat("MoveX", moveX, dampTime, Time.deltaTime);
        animator.SetFloat("MoveY", moveY, dampTime, Time.deltaTime);


        if (Mathf.Abs(moveX) > 0.1f)
        {
            spriteRenderer.flipX = moveX < 0f;
        }
    }
    // private void SpriteDirectional() {

    //     Vector3 cameraForwardVector = new Vector3(Camera.main.transform.forward.x, 0f, Camera.main.transform.forward.z);

    //     float signedAngle = Vector3.SignedAngle(mainTransform.forward, cameraForwardVector, Vector3.up);

    //     Vector2 animationDirection = new Vector2(0f, -1f);

    //     float angle = Mathf.Abs(signedAngle);


    //     if (angle < backAngle)
    //     {
    //         animationDirection = new Vector2(0f, -1f); // Frente
    //     }
    //     else if (angle < sideAngle)
    //     {
    //         animationDirection = new Vector2(Mathf.Sign(signedAngle), 0f); // Lados
    //     }
    //     else
    //     {
    //         animationDirection = new Vector2(0f, 1f); // Costas
    //     }
    //     animator.SetFloat("MoveX", animationDirection.x);
    //     animator.SetFloat("MoveY", animationDirection.y);
    // }
}