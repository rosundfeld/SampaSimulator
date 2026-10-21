using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public event EventHandler OnStateChanged;
    public event EventHandler OnGamePaused;
    public event EventHandler OnGameUnpaused;

    private enum State
    {
        GamePlaying,
        GameOver,
    }

    private State state;
    private bool isGamePaused = false;

    private void Awake()
    {
        Instance = this;
        state = State.GamePlaying;
    }

    private void Start()
    {
        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
        PlayerDeathManager.Instance.OnPlayerDeath += PlayerDeathManager_OnPlayerDeath;

        CursorUtils.HideCursor();
    }

    private void GameInput_OnPauseAction(object sender, EventArgs e)
    {
        TogglePauseGame();
    }

    private void PlayerDeathManager_OnPlayerDeath(object sender, PlayerDeathManager.OnPlayerDeathEventArgs e)
    {
        SetState(State.GameOver);
        CursorUtils.ShowCursor();
    }

    private void SetState(State newState)
    {
        state = newState;
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void TogglePauseGame()
    {
        // No game over a tela de morte já controla o fluxo; pausar por cima dela só confundiria.
        if (IsGameOver())
            return;

        isGamePaused = !isGamePaused;
        if (isGamePaused)
        {
            Time.timeScale = 0f;
            CursorUtils.ShowCursor();
            OnGamePaused?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            Time.timeScale = 1f;
            // Um diálogo aberto continua precisando do cursor para clicar em "próximo".
            if (DialogManager.Instance == null || !DialogManager.Instance.IsDialogActive())
                CursorUtils.HideCursor();
            OnGameUnpaused?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool IsGamePlaying()
    {
        return state == State.GamePlaying;
    }

    public bool IsGameOver()
    {
        return state == State.GameOver;
    }

    public bool IsGamePaused()
    {
        return isGamePaused;
    }
}
