using Unity.Cinemachine;
using UnityEngine;

public class ThirdPersonCam : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CinemachineInputAxisController axisController;
    [SerializeField] private Transform orientation; // Eixos forward/right relativos à câmera, lidos pelo PlayerMovement
    [SerializeField] private Transform player;

    private void Update()
    {
        bool isCameraLocked = PlayerMovement.Instance.IsInteracting || !GameManager.Instance.IsGamePlaying();
        axisController.enabled = !isCameraLocked;

        if (!isCameraLocked)
            UpdateOrientation();
    }

    // Only orients the camera-relative forward/right axes; player facing is owned by PlayerMovement.
    private void UpdateOrientation()
    {
        Vector3 viewDir = player.position - new Vector3(transform.position.x, player.position.y, transform.position.z);
        orientation.forward = viewDir.normalized;
    }
}
