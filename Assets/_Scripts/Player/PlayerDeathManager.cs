using System;
using UnityEngine;

public class PlayerDeathManager : MonoBehaviour
{
    public static PlayerDeathManager Instance { get; private set; }

    public event EventHandler<OnPlayerDeathEventArgs> OnPlayerDeath;
    public class OnPlayerDeathEventArgs : EventArgs
    {
        public DeathCause deathCause;
    }

    public enum DeathCause
    {
        Car,
        SmokeCloud,
        NpcAttack,
        ManholeExplosion,
    }

    public bool IsDead { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void Kill(DeathCause deathCause)
    {
        if (IsDead)
            return;

        IsDead = true;
        Debug.Log($"Igor morreu: {deathCause}");

        OnPlayerDeath?.Invoke(this, new OnPlayerDeathEventArgs { deathCause = deathCause });
    }
}
