using System;
using UnityEngine;

// Nuvem de CO2: enquanto o player está dentro do trigger a exposição sobe até matar em secondsToDie.
// O tamanho vem só do Transform/Collider, então qualquer escala funciona.
[RequireComponent(typeof(Collider))]
public class CO2Cloud : MonoBehaviour
{
    private const string PLAYER_TAG = "Player";

    // Static igual ao ExplosiveDrain: a UI escuta todas as nuvens da cena sem conhecer cada uma.
    public static event EventHandler<ExposureChangedEventArgs> OnAnyExposureChanged;
    public class ExposureChangedEventArgs : EventArgs
    {
        public float ExposureNormalized;
        public bool IsPlayerInside;
    }

    [SerializeField] private float secondsToDie = 8f;
    [Tooltip("1 = recupera na mesma velocidade que intoxica.")]
    [SerializeField] private float recoverySpeedMultiplier = 1f;
    [Tooltip("x = intervalo da tosse com exposição máxima, y = no começo da exposição.")]
    [SerializeField] private Vector2 coughInterval = new Vector2(0.6f, 2.5f);

    private Transform player;
    private float exposure;
    private float coughTimer;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(PLAYER_TAG))
            return;

        player = other.transform;
        coughTimer = coughInterval.y;
        SoundManager.Instance.PlayCO2WarningSound(player.position);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(PLAYER_TAG))
            player = null;
    }

    private void Update()
    {
        if (PlayerDeathManager.Instance.IsDead)
            return;

        bool isPlayerInside = player != null;
        if (!isPlayerInside && exposure <= 0f)
            return;

        float step = Time.deltaTime / secondsToDie;
        exposure = Mathf.Clamp01(exposure + (isPlayerInside ? step : -step * recoverySpeedMultiplier));

        OnAnyExposureChanged?.Invoke(this, new ExposureChangedEventArgs
        {
            ExposureNormalized = exposure,
            IsPlayerInside = isPlayerInside,
        });

        if (!isPlayerInside)
            return;

        if (exposure >= 1f)
        {
            PlayerDeathManager.Instance.Kill(PlayerDeathManager.DeathCause.SmokeCloud);
            return;
        }

        coughTimer -= Time.deltaTime;
        if (coughTimer <= 0f)
        {
            SoundManager.Instance.PlayCoughSound(player.position);
            coughTimer = Mathf.Lerp(coughInterval.y, coughInterval.x, exposure);
        }
    }
}
