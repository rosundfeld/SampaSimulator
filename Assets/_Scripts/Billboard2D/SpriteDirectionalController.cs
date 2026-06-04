using System;
using UnityEngine;

public class SpriteDirectionalController : MonoBehaviour
{
    [SerializeField] float backAngle = 65f;
    [SerializeField] float sideAngle = 155f;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Animator animator;
    [SerializeField] Transform mainTransform;

    void LateUpdate()
    {
        Vector3 cameraForwardVector = new Vector3(Camera.main.transform.forward.x, 0f, Camera.main.transform.forward.z);

        float signedAngle = Vector3.SignedAngle(mainTransform.forward, cameraForwardVector, Vector3.up);

        Vector2 animationDirection = new Vector2(0f, -1f);

        float angle = MathF.Abs(signedAngle);


        if (angle < backAngle)
        {
            //Back
            animationDirection = new Vector2(0f, -1f);
        }
        else if (angle < sideAngle)
        {
            //Side
            if (signedAngle < 0f)
            {
                //Flip sprite if camera is on the left side of the character
                animationDirection = new Vector2(1f, 0f);
                spriteRenderer.flipX = true;

            }
            else
            {
                animationDirection = new Vector2(-1f, 0f);
                spriteRenderer.flipX = false;
            }
        }
        else
        {
            //Front
            animationDirection = new Vector2(0f, 1f);
        }

        animator.SetFloat("MoveX", animationDirection.x);
        animator.SetFloat("MoveY", animationDirection.y);
    }
}
