using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class ThirdPersonCam : MonoBehaviour
{

    public CinemachineInputAxisController axisController;

    [Header("References")]
    public Transform orientation; // The target the camera follows
    public Transform player; // The pivot point for camera rotation
    public Transform playerObj; // The pivot point for camera rotation
    public Rigidbody rb;

    public float rotationSpeed; // Speed of camera rotation
    private Transform startCamPosition;


    private void Start()
    {
        startCamPosition = this.transform;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        CheckIfPlayerHasInteraction();
    }

    private void handleCamPosition()
    {
        Vector3 viewDir = player.position - new Vector3(transform.position.x, player.position.y, transform.position.z);
        orientation.forward = viewDir.normalized;

        //rotate player object
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        Vector3 inputDir = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (inputDir != Vector3.zero)
        {
            playerObj.forward = Vector3.Slerp(playerObj.forward, inputDir.normalized, Time.deltaTime * rotationSpeed);
        }
    }

    public void CheckIfPlayerHasInteraction()
    {
        if (PlayerMovement.Instance.isInteracting)
        {
            LockCamera();
        } else
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
