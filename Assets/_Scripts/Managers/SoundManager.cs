using System;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private const string PLAYER_PREFS_SOUND_EFFECTS_VOLUME = "SoundEffectsVolume";

    public static SoundManager Instance { get; private set; }

    [SerializeField] private AudioClipRefsSO audioClipRefsSO;

    private float volume = 1f;

    private void Awake()
    {
        Instance = this;
        volume = PlayerPrefs.GetFloat(PLAYER_PREFS_SOUND_EFFECTS_VOLUME, 1f);
    }

    private void Start()
    {
        ExplosiveDrain.OnAnyExplosion += ExplosiveDrain_OnAnyExplosion;
    }

    private void OnDestroy()
    {
        // Evento static sobrevive à troca de cena; sem isso o SoundManager destruído continuaria inscrito.
        ExplosiveDrain.OnAnyExplosion -= ExplosiveDrain_OnAnyExplosion;
    }

    private void ExplosiveDrain_OnAnyExplosion(object sender, EventArgs e)
    {
        ExplosiveDrain explosiveDrain = sender as ExplosiveDrain;
        PlaySound(audioClipRefsSO.explosion, explosiveDrain.transform.position);
    }

    public void PlayFootstepSound(Vector3 position, float volumeMultiplier = 1f)
    {
        PlaySound(audioClipRefsSO.footstep, position, volumeMultiplier);
    }

    private void PlaySound(AudioClip[] audioClipArray, Vector3 position, float volumeMultiplier = 1f)
    {
        if (audioClipArray.Length == 0)
            return;

        PlaySound(audioClipArray[UnityEngine.Random.Range(0, audioClipArray.Length)], position, volumeMultiplier);
    }

    private void PlaySound(AudioClip audioClip, Vector3 position, float volumeMultiplier = 1f)
    {
        AudioSource.PlayClipAtPoint(audioClip, position, volumeMultiplier * volume);
    }

    public void ChangeVolume()
    {
        volume += 0.1f;
        if (volume > 1f)
            volume = 0f;

        PlayerPrefs.SetFloat(PLAYER_PREFS_SOUND_EFFECTS_VOLUME, volume);
        PlayerPrefs.Save();
    }

    public float GetVolume()
    {
        return volume;
    }
}
