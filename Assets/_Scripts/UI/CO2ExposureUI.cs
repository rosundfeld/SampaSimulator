using System;
using UnityEngine;
using UnityEngine.UI;

// Escurece as bordas da tela conforme a exposição ao CO2 e pisca um aviso enquanto o player está na nuvem.
public class CO2ExposureUI : MonoBehaviour
{
    [Tooltip("Image em tela cheia (stretch). O sprite da vinheta é gerado aqui, não precisa de textura.")]
    [SerializeField] private Image vignetteImage;
    [Tooltip("Ícone/texto de aviso. Pode ficar vazio.")]
    [SerializeField] private GameObject warning;
    [SerializeField] private float warningBlinkSpeed = 4f;
    [Tooltip("Escala da vinheta sem exposição; >1 empurra o preto para fora da tela.")]
    [SerializeField] private float startScale = 1.4f;
    [Tooltip("<1 escurece mais cedo. 0.4 = ~75% do efeito na metade do tempo.")]
    [Range(0.1f, 1f)]
    [SerializeField] private float intensityCurve = 0.4f;

    private bool isPlayerInside;

    private void Start()
    {
        CO2Cloud.OnAnyExposureChanged += CO2Cloud_OnAnyExposureChanged;

        vignetteImage.sprite = CreateVignetteSprite(256);
        vignetteImage.raycastTarget = false;
        SetExposure(0f);
        if (warning != null)
            warning.SetActive(false);
    }

    private void OnDestroy()
    {
        // Evento static sobrevive à troca de cena.
        CO2Cloud.OnAnyExposureChanged -= CO2Cloud_OnAnyExposureChanged;
    }

    private void Update()
    {
        if (warning != null)
            warning.SetActive(isPlayerInside && Mathf.PingPong(Time.time * warningBlinkSpeed, 1f) > 0.5f);
    }

    private void CO2Cloud_OnAnyExposureChanged(object sender, CO2Cloud.ExposureChangedEventArgs e)
    {
        isPlayerInside = e.IsPlayerInside;
        SetExposure(e.ExposureNormalized);
    }

    private void SetExposure(float exposure)
    {
        exposure = Mathf.Pow(exposure, intensityCurve);

        Color color = vignetteImage.color;
        color.a = exposure;
        vignetteImage.color = color;
        // Encolher a vinheta faz o preto "fechar" das bordas para o centro.
        vignetteImage.rectTransform.localScale = Vector3.one * Mathf.Lerp(startScale, 1f, exposure);
    }

    private static Sprite CreateVignetteSprite(int size)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;

        Vector2 center = Vector2.one * (size - 1) * 0.5f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // 0 no centro, 1 no meio das bordas, ~1.41 nos cantos.
                float distance = Vector2.Distance(new Vector2(x, y), center) / center.x;
                float alpha = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.15f, 0.9f, distance));
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, size, size), Vector2.one * 0.5f);
    }
}
