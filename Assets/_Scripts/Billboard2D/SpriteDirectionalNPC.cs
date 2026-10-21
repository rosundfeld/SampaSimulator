using UnityEngine;
using UnityEngine.Serialization;

// Visual dos NPCs: escolhe frente/lado/costas comparando para onde o NPC olha com a direção da câmera.
public class SpriteDirectionalNPC : MonoBehaviour
{
    private const string MOVE_X = "MoveX";
    private const string MOVE_Y = "MoveY";

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform mainTransform;

    [Tooltip("Marque se o sprite tem animações próprias para esquerda e direita; senão a lateral é espelhada com flipX")]
    [FormerlySerializedAs("hasTwoAniamations")]
    [SerializeField] private bool hasTwoSideAnimations = false;

    [Range(0f, 180f)][SerializeField] private float backAngle = 65f;
    [Range(0f, 180f)][SerializeField] private float sideAngle = 155f;

    private void LateUpdate()
    {
        if (Camera.main == null)
            return;

        Vector2 animationDirection = GetAnimationDirection();
        animator.SetFloat(MOVE_X, animationDirection.x);
        animator.SetFloat(MOVE_Y, animationDirection.y);
    }

    private Vector2 GetAnimationDirection()
    {
        Vector3 cameraForward = Camera.main.transform.forward;
        cameraForward.y = 0f;

        float signedAngle = Vector3.SignedAngle(mainTransform.forward, cameraForward, Vector3.up);
        float angle = Mathf.Abs(signedAngle);

        if (angle < backAngle)
            return new Vector2(0f, -1f); // Frente

        if (angle >= sideAngle)
            return new Vector2(0f, 1f); // Costas

        // Lados
        if (!hasTwoSideAnimations)
            spriteRenderer.flipX = signedAngle >= 0f;

        return new Vector2(signedAngle < 0f ? -1f : 1f, 0f);
    }
}
