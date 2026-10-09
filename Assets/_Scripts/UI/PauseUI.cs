using UnityEngine;
using UnityEngine.UI;

public class PauseUI : MonoBehaviour
{
    [SerializeField] private Button menuButton;
    [SerializeField] private Button resumeButton;

    private void Start()
    {
        menuButton.onClick.AddListener(() => { Loader.Load(Loader.Scene.MainMenu); });
        resumeButton.onClick.AddListener(() =>
        {
            GameManager.Instance.setGamePaused(false);
            HidePauseMenu();
        });

        GameManager.Instance.OnGamePaused += GameManager_OnGamePaused;
        GameManager.Instance.OnGameUnpaused += GameManager_OnGameUnpaused;

        HidePauseMenu();
    }

    private void GameManager_OnGamePaused(object sender, System.EventArgs e)
    {
        ShowPauseMenu();
    }

    private void GameManager_OnGameUnpaused(object sender, System.EventArgs e)
    {
        HidePauseMenu();
    }

    public void ShowPauseMenu()
    {
        PlayerMovement.Instance.SetInteracting(true);
        CursorUtils.ShowCursor();
        gameObject.SetActive(true);
    }

    public void HidePauseMenu()
    {
        PlayerMovement.Instance.SetInteracting(false);
        CursorUtils.HideCursor();
        gameObject.SetActive(false);
    }
}
