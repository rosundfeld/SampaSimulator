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
        int deathMessageIndex = Random.Range(0, 3); // Exemplo de índice aleatório para variar a mensagem de morte
        switch (deathCause)
        {
            case PlayerDeathManager.DeathCause.Car:
                if (deathMessageIndex == 0)
                    return "Tem que olhar para os dois lados antes de atravessar a rua!";
                else if (deathMessageIndex == 1)
                    return "Cuidado ao atravessar a rua!";
                else
                    return "Morreu de carro...";
            case PlayerDeathManager.DeathCause.SmokeCloud:
                if (deathMessageIndex == 0)
                    return "Cof cof cof, RIP";
                else if (deathMessageIndex == 1)
                    return "Fumo desde meus 15 anos e to benzão!";
                else
                    return "CO2 não caiu bem";
            case PlayerDeathManager.DeathCause.NpcAttack:
                if (deathMessageIndex == 0)
                    return "'Irmão, tem um troco ai?'";
                else if (deathMessageIndex == 1)
                    return "Nóia";
                else
                    return "'O Goku reagiria a um assalto'";
            case PlayerDeathManager.DeathCause.ManholeExplosion:
                if (deathMessageIndex == 0)
                    return "Morreu de curioso";
                else if (deathMessageIndex == 1)
                    return "O bueiro explodiu e ele não resistiu! (é mais comum do que parece)";
                else
                    return "Virou noticia no Bom dia São Paulo";
            default:
                return $"Morreu de: ????";


        }
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
