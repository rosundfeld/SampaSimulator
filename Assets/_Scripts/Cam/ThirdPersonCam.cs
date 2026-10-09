using Unity.Cinemachine;
using UnityEngine;

public class ThirdPersonCam : MonoBehaviour
{

    public CinemachineInputAxisController axisController;

    [Header("References")]
    public Transform orientation; // The target the camera follows

    [SerializeField]
    private Transform player; // The pivot point for camera rotation

    private Transform startCamPosition;
    private FadeObstacle _currentFadeObstacle;


    private void Start()
    {
        startCamPosition = this.transform;
        CursorUtils.HideCursor();
    }

    private void Update()
    {
        CheckIfPlayerHasInteraction();
    }

    // Only orients the camera-relative forward/right axes; player facing is owned by PlayerMovement.
    private void handleCamPosition()
    {
        Vector3 viewDir = player.position - new Vector3(transform.position.x, player.position.y, transform.position.z);
        orientation.forward = viewDir.normalized;
    }

    public void CheckIfPlayerHasInteraction()
    {
        if (PlayerMovement.Instance != null && PlayerMovement.Instance.IsInteracting)
        {
            LockCamera();
        }
        else
        {
            UnlockCamera();
            handleCamPosition();
        }
    }

    public void LockCamera()
    {
        axisController.enabled = false;
    }

    public void UnlockCamera()
    {
        axisController.enabled = true;
    }
}
