using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class PlayerDeathManager : MonoBehaviour
{
    public static PlayerDeathManager Instance { get; private set; }

    public static event EventHandler<PlayerDeathData> onPlayerDeath;
    public class PlayerDeathData
    {
        public string deathCause;
    }

    public enum DeathCause
    {
        Car,
        SmokeCloud,
        NpcAttack,
        ManholeExplosion
    }

    [SerializeField] private float reloadDelay = 1.5f;

    public bool IsDead { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Kill(DeathCause cause)
    {
        if (IsDead)
            return;

        IsDead = true;
        Debug.Log($"Igor morreu: {cause}");

        if (PlayerMovement.Instance != null)
            PlayerMovement.Instance.SetInteracting(true);
        onPlayerDeath?.Invoke(this, new PlayerDeathData { deathCause = cause.ToString() });
        
        CursorUtils.ShowCursor();
    }


    public static void ResetStaticData()
    {
        onPlayerDeath = null;
    }
}
