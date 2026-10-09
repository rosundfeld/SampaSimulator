using System;
using UnityEngine;

public class ExplosiveDrain : MonoBehaviour
{
    const string PLAYER_TAG = "Player";
    const string EXPLODE_TRIGGER = "Explosion";

    [SerializeField] private Transform GFX;

    public event EventHandler OnPlayerKilled;

    public static ExplosiveDrain Instance { get; private set; }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(PLAYER_TAG))
            return;
        showGFX();
        KillPlayer();
    }

    private void Awake()
    {
        Instance = this;
    }

    private void KillPlayer()
    {
        GFX.GetComponent<Animator>().SetTrigger(EXPLODE_TRIGGER);
        OnPlayerKilled?.Invoke(this, EventArgs.Empty);
        PlayerDeathManager.Instance?.Kill(PlayerDeathManager.DeathCause.ManholeExplosion);
    }

    private void Start()
    {
        hideGFX();
    }

    private void showGFX()
    {
        GFX.gameObject.SetActive(true);
    }
    private void hideGFX()
    {
        GFX.gameObject.SetActive(false);
    }
}
