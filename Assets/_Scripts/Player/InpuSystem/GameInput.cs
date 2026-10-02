using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class GameInput : MonoBehaviour
{
    //----------------CONSTANTS ----------------//
    private const string PLAYER_PREFS_BINDING = "PlayerInputBindings"; // Key for storing player input bindings in PlayerPrefs


    public enum Binding
    {
        Move_Up,
        Move_Down,
        Move_Left,
        Move_Right,
        Interact,
        Pause,
        Sprint,
        Jump
    }

    //---------------SINGLETON-----------------
    public static GameInput Instance { get; private set; }

    //---------------EVENTS-----------------
    public event EventHandler OnInteractAction;
    public event EventHandler OnJumpPressed;
    public event EventHandler<bool> OnJumpHeldChanged;
    public event EventHandler<bool> OnSprintStateChanged;
    public event EventHandler OnPauseAction;


    private PlayerInputSystem playerInputActions;

    private void Awake()
    {
        Instance = this;
        playerInputActions = new PlayerInputSystem();
        //if (PlayerPrefs.HasKey(PLAYER_PREFS_BINDINGS))
        // {
        //     playerInputActions.LoadBindingOverridesFromJson(PlayerPrefs.GetString(PLAYER_PREFS_BINDINGS));
        // }

        playerInputActions.Player.Enable(); // Enable the player input actions

        // playerInputActions.Player.Interact.performed += Interact_performed;
        // playerInputActions.Player.InteractAlternate.performed += InteractAlternate_performed;
        // playerInputActions.Player.Pause.performed += Pause_performed;
        playerInputActions.Player.Jump.performed += Jump_performed;
        playerInputActions.Player.Jump.started += Jump_started;
        playerInputActions.Player.Jump.canceled += Jump_canceled;
        playerInputActions.Player.Sprint.started += Sprint_started;
        playerInputActions.Player.Sprint.canceled += Sprint_canceled;

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

    private void OnDestroy()
    {
        // playerInputActions.Player.Interact.performed -= Interact_performed;
        // playerInputActions.Player.InteractAlternate.performed -= InteractAlternate_performed;
        // playerInputActions.Player.Pause.performed -= Pause_performed;

        playerInputActions.Dispose(); // Dispose of the player input actions
    }


    //Retorna o vetor de movimento normalizado do jogador.
    public Vector2 GetMovementVectorNormalized()
    {
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();
        return inputVector.normalized;
    }
}
