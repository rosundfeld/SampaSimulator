using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    public event EventHandler OnInteractAction;
    public event EventHandler OnPauseAction;
    public event EventHandler OnJumpPressed;
    public event EventHandler<bool> OnJumpHeldChanged;
    public event EventHandler<bool> OnSprintStateChanged;

    private PlayerInputSystem playerInputActions;

    private void Awake()
    {
        Instance = this;

        playerInputActions = new PlayerInputSystem();
        playerInputActions.Player.Enable();

        playerInputActions.Player.Interact.performed += Interact_performed;
        playerInputActions.Player.Pause.performed += Pause_performed;
        playerInputActions.Player.Jump.performed += Jump_performed;
        playerInputActions.Player.Jump.started += Jump_started;
        playerInputActions.Player.Jump.canceled += Jump_canceled;
        playerInputActions.Player.Sprint.started += Sprint_started;
        playerInputActions.Player.Sprint.canceled += Sprint_canceled;
    }

    private void OnDestroy()
    {
        playerInputActions.Player.Interact.performed -= Interact_performed;
        playerInputActions.Player.Pause.performed -= Pause_performed;
        playerInputActions.Player.Jump.performed -= Jump_performed;
        playerInputActions.Player.Jump.started -= Jump_started;
        playerInputActions.Player.Jump.canceled -= Jump_canceled;
        playerInputActions.Player.Sprint.started -= Sprint_started;
        playerInputActions.Player.Sprint.canceled -= Sprint_canceled;

        // Disable antes do Dispose evita o aviso de leak do finalizer gerado pelo Input System.
        playerInputActions.Player.Disable();
        playerInputActions.Dispose();
    }

    private void Interact_performed(InputAction.CallbackContext context)
    {
        OnInteractAction?.Invoke(this, EventArgs.Empty);
    }

    private void Pause_performed(InputAction.CallbackContext context)
    {
        OnPauseAction?.Invoke(this, EventArgs.Empty);
    }

    private void Jump_performed(InputAction.CallbackContext context)
    {
        OnJumpPressed?.Invoke(this, EventArgs.Empty);
    }

    private void Jump_started(InputAction.CallbackContext context)
    {
        OnJumpHeldChanged?.Invoke(this, true);
    }

    private void Jump_canceled(InputAction.CallbackContext context)
    {
        OnJumpHeldChanged?.Invoke(this, false);
    }

    private void Sprint_started(InputAction.CallbackContext context)
    {
        OnSprintStateChanged?.Invoke(this, true);
    }

    private void Sprint_canceled(InputAction.CallbackContext context)
    {
        OnSprintStateChanged?.Invoke(this, false);
    }

    public Vector2 GetMovementVectorNormalized()
    {
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();
        return inputVector.normalized;
    }
}
