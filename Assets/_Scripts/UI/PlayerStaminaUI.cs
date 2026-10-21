using UnityEngine;
using UnityEngine.UI;

public class PlayerStaminaUI : MonoBehaviour
{
    [SerializeField] private Image barImage;
    [SerializeField] private float hideDelay = 5f;

    private float hideTimer;

    private void Start()
    {
        PlayerMovement.Instance.OnStaminaChanged += PlayerMovement_OnStaminaChanged;

        barImage.fillAmount = 1f;
        Hide();
    }

    private void Update()
    {
        hideTimer -= Time.deltaTime;
        if (hideTimer <= 0f)
            Hide();
    }

    private void PlayerMovement_OnStaminaChanged(object sender, PlayerMovement.OnStaminaChangedEventArgs e)
    {
        barImage.fillAmount = e.staminaNormalized;

        if (e.isDraining)
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
