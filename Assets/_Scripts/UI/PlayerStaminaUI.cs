using UnityEngine;
using UnityEngine.UI;

public class PlayerStaminaUI : MonoBehaviour
{
    [SerializeField] private Image barImage;
    [SerializeField] private float hideDelay = 5f;

    private float hideTimer;

    private void Start()
    {
        PlayerStamina.Instance.OnStaminaChanged += PlayerStamina_OnStaminaChanged;

        barImage.fillAmount = 1f;
        Hide();
    }

    private void Update()
    {
        hideTimer -= Time.deltaTime;
        if (hideTimer <= 0f)
            Hide();
    }

    private void PlayerStamina_OnStaminaChanged(object sender, PlayerStamina.StaminaChangedEventArgs e)
    {
        barImage.fillAmount = e.StaminaNormalized;

        if (e.IsDraining)
        {
            hideTimer = hideDelay;
            Show();
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
