using UnityEngine;

public class SpriteBillboard : MonoBehaviour
{
    [Tooltip("Gira só no eixo Y (sprite fica em pé). Desmarque para encarar a câmera em todos os eixos")]
    [SerializeField] private bool fixedXZ = true;

    private void LateUpdate()
    {
        if (Camera.main == null)
            return;

        Quaternion cameraRotation = Camera.main.transform.rotation;
        transform.rotation = fixedXZ
            ? Quaternion.Euler(0f, cameraRotation.eulerAngles.y, 0f)
            : cameraRotation;
    }
}
