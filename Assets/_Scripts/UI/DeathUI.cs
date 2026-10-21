using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeathUI : MonoBehaviour
{
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button tryAgainButton;
    [SerializeField] private TextMeshProUGUI deathDescription;

    private void Awake()
    {
        mainMenuButton.onClick.AddListener(() =>
        {
            Loader.Load(Loader.Scene.MainMenu);
        });
        tryAgainButton.onClick.AddListener(() =>
        {
            Loader.Load(Loader.Scene.Game);
        });
    }

    private void Start()
    {
        PlayerDeathManager.Instance.OnPlayerDeath += PlayerDeathManager_OnPlayerDeath;

        Hide();
    }

    private void PlayerDeathManager_OnPlayerDeath(object sender, PlayerDeathManager.OnPlayerDeathEventArgs e)
    {
        deathDescription.text = GetDeathMessage(e.deathCause);
        Show();
    }

    private string GetDeathMessage(PlayerDeathManager.DeathCause deathCause)
    {
        // TODO(human): converter a causa da morte em uma mensagem para o jogador.
        return $"Morreu de: {deathCause}";
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
