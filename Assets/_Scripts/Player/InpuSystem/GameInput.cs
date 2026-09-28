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
    public event EventHandler OnSprintPressed;
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
        playerInputActions.Player.Sprint.performed += Sprint_performed;

    }

    private void Jump_performed(InputAction.CallbackContext context)
    {
        OnJumpPressed?.Invoke(this, EventArgs.Empty);
    }

    private void Sprint_performed(InputAction.CallbackContext context)
    {
        OnSprintPressed?.Invoke(this, EventArgs.Empty);
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
        Debug.Log("Raw input vector: " + inputVector);

        inputVector = inputVector.normalized;

        return inputVector;
    }
}
