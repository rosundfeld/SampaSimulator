using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{

    public static SoundManager Instance { get; private set; }

    private const string PLAYER_PREFS_SOUND_EFFECTS_VOLUME = "SoundEffectsVolume";


    [SerializeField] private AudioClipRefsSO audioClipRefsSO;

    private float volume = 1f;

    private void Awake()
    {
        Instance = this;
        // if (!PlayerPrefs.HasKey(PLAYER_PREFS_SOUND_EFFECTS_VOLUME))
        // {
        //     volume = PlayerPrefs.GetFloat(PLAYER_PREFS_SOUND_EFFECTS_VOLUME);
        // }
    }

    private void Start()
    {
        ExplosiveDrain.Instance.OnPlayerKilled += ExplosiveDrain_OnPlayerKilled;
    }

    //------------------------------Player Events---------------------------------
    private void ExplosiveDrain_OnPlayerKilled(object sender, System.EventArgs e)
    {
        ExplosiveDrain drain = sender as ExplosiveDrain;
        PlaySound(audioClipRefsSO.explosion, drain.transform.position);
    }
    //------------------------------BaseCounter Events---------------------------------
    private void BaseCounter_OnAnyObjectPlacedHere(object sender, System.EventArgs e)
    {
        // BaseCounter baseCounter = sender as BaseCounter;
        // PlaySound(audioClipRefsSO.objectDrop, baseCounter.transform.position);
    }


    //------------------------------DeliveryManager Events---------------------------------
    private void DeliveryManager_OnRecipeSuccess(object sender, System.EventArgs e)
    {
        // DeliveryCounter deliveryCounter = DeliveryCounter.Instance;
        // PlaySound(audioClipRefsSO.deliverySuccess, deliveryCounter.transform.position);
    }


    private void PlaySound(AudioClip clip, Vector3 position, float volumeMultiplier = 1f)
    {
        AudioSource.PlayClipAtPoint(clip, position, volumeMultiplier);
    }

    private void PlaySound(AudioClip[] clipsArray, Vector3 position, float volumeMultiplier = 1f)
    {
        if (clipsArray.Length == 0) return;
        AudioClip randomClip = clipsArray[Random.Range(0, clipsArray.Length)];
        AudioSource.PlayClipAtPoint(randomClip, position, volumeMultiplier * GetVolume());
    }

    public void PlayFootstepSound(Vector3 position, float volumeMultiplier = 1f)
    {
        PlaySound(audioClipRefsSO.footstep, position, volumeMultiplier);
    }

    //------------------------------Countdown---------------------------------
    // public void PlayCountdownSound()
    // {
    //     PlaySound(audioClipRefsSO.warning, Vector3.zero);
    // }

    // public void PlayWarningSound(Vector3 position)
    // {
    //     PlaySound(audioClipRefsSO.warning, position);
    // }

    public void ChangeVolume()
    {
        volume += 0.1f;
        if (volume > 1f)
        {
            volume = 0f;
        }

        PlayerPrefs.SetFloat(PLAYER_PREFS_SOUND_EFFECTS_VOLUME, volume);
        PlayerPrefs.Save();
    }

    public float GetVolume()
    {
        return volume;
    }
}
