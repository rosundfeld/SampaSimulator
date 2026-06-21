using UnityEngine;

public class SpriteDirectionalNPC : MonoBehaviour
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Animator animator;

    [SerializeField] Transform mainTransform;

    [SerializeField] float backAngle = 65f;
    [SerializeField] float sideAngle = 155f;


    private Vector2 lastMoveDir = new Vector2(0f, -1f); // direção inicial (ajuste se qu
    void LateUpdate()
    {
        if (Camera.main == null) return;

        SpriteDirectional();

    }
    private void SpriteDirectional() {

        Vector3 cameraForwardVector = new Vector3(Camera.main.transform.forward.x, 0f, Camera.main.transform.forward.z);

        float signedAngle = Vector3.SignedAngle(mainTransform.forward, cameraForwardVector, Vector3.up);

        Vector2 animationDirection = new Vector2(0f, -1f);

        float angle = Mathf.Abs(signedAngle);


        if (angle < backAngle)
        {
            animationDirection = new Vector2(0f, -1f); // Frente
        }
        else if (angle < sideAngle)
        {
            animationDirection = new Vector2(Mathf.Sign(signedAngle), 0f); // Lados
        }
        else
        {
            animationDirection = new Vector2(0f, 1f); // Costas
        }
        animator.SetFloat("MoveX", animationDirection.x);
        animator.SetFloat("MoveY", animationDirection.y);
    }
}