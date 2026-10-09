using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class DeathUI : MonoBehaviour
{

    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button tryAgainButton;
    [SerializeField] private TextMeshProUGUI deathDescription;

     private void Start()
    {
        if (PlayerDeathManager.Instance != null)
        {
            PlayerDeathManager.onPlayerDeath += Player_OnAnyPlayerDeath;
        }

        mainMenuButton.onClick.AddListener(() => Loader.Load(Loader.Scene.MainMenu));
        tryAgainButton.onClick.AddListener(() => Loader.Load(Loader.Scene.Game));

        Hide();
    }

    private void Player_OnAnyPlayerDeath(object sender, PlayerDeathManager.PlayerDeathData e)
    {
        Show();
        deathDescription.text = $"Morreu de: {e.deathCause}";
    }

   public void Show()
   {
       gameObject.SetActive(true);
   }

   public void Hide()
   {
       gameObject.SetActive(false);
   }    

   private void OnDestroy()
   {
       if (PlayerDeathManager.Instance != null)
       {
           PlayerDeathManager.onPlayerDeath -= Player_OnAnyPlayerDeath;
       }
   }
}
