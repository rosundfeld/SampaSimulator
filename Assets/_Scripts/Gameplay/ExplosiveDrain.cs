using System;
using UnityEngine;
using UnityEngine.Serialization;

public class ExplosiveDrain : MonoBehaviour
{
    private const string PLAYER_TAG = "Player";
    private const string EXPLOSION_TRIGGER = "Explosion";

    // Static porque a cena pode ter vários bueiros; quem escuta (SoundManager) não precisa conhecer cada um.
    public static event EventHandler OnAnyExplosion;

    [FormerlySerializedAs("GFX")]
    [SerializeField] private Transform gfx;

    private Animator gfxAnimator;

    private void Awake()
    {
        gfxAnimator = gfx.GetComponent<Animator>();
    }

    private void Start()
    {
        gfx.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(PLAYER_TAG))
            return;

        Explode();
    }

    private void Explode()
    {
        gfx.gameObject.SetActive(true);
        gfxAnimator.SetTrigger(EXPLOSION_TRIGGER);

        OnAnyExplosion?.Invoke(this, EventArgs.Empty);
        PlayerDeathManager.Instance.Kill(PlayerDeathManager.DeathCause.ManholeExplosion);
    }
}
