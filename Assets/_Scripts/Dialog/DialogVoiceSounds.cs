using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(AudioSource))]
public class DialogVoiceSounds : MonoBehaviour
{
    // Índice do grupo = índice da vogal (A, E, I, O, U); os acentos do português caem na vogal base.
    private static readonly string[] VOWEL_GROUPS = { "aáàâã", "eéê", "ií", "oóôõ", "uúü" };
    private const int CONSONANT_INDEX = -1;
    private const int VOWEL_I = 2;
    private const int VOWEL_O = 3;
    private const int VOWEL_U = 4;
    private const float VOWEL_PITCH_OFFSET = 0.05f;
    private const float MIN_PITCH = 0.5f;
    private const float MAX_PITCH = 2.5f;

    private AudioSource audioSource;
    private float targetPitch = 1f;
    private float pitchGlideSpeed;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // O pitch desliza até o alvo em vez de pular: vozes reais fazem glissando entre sílabas, saltos secos soam robóticos.
    private void Update()
    {
        audioSource.pitch = Mathf.Lerp(audioSource.pitch, targetPitch, pitchGlideSpeed * Time.deltaTime);
    }

    private void Start()
    {
        DialogManager.Instance.OnLetterTyped += DialogManager_OnLetterTyped;
        DialogCombatManager.Instance.OnLetterTyped += DialogCombatManager_OnLetterTyped;
    }

    private void OnDestroy()
    {
        if (DialogManager.Instance != null)
            DialogManager.Instance.OnLetterTyped -= DialogManager_OnLetterTyped;
        if (DialogCombatManager.Instance != null)
            DialogCombatManager.Instance.OnLetterTyped -= DialogCombatManager_OnLetterTyped;
    }

    private void DialogManager_OnLetterTyped(object sender, DialogManager.OnLetterTypedEventArgs e)
    {
        PlayVoice(e);
    }

    private void DialogCombatManager_OnLetterTyped(object sender, DialogManager.OnLetterTypedEventArgs e)
    {
        PlayVoice(e);
    }

    private void PlayVoice(DialogManager.OnLetterTypedEventArgs e)
    {
        if (e.voice == null || e.voice.vowelClips.Length == 0)
            return;

        int vowelIndex = GetVowelIndex(e.letter);
        if (vowelIndex == CONSONANT_INDEX)
            return;
        if (!e.speakAllVowels && !IsSpokenVowel(e.voice, e.line, e.letterIndex))
            return;

        AudioClip audioClip = e.voice.vowelClips[vowelIndex % e.voice.vowelClips.Length];

        targetPitch = GetPitch(e.voice, vowelIndex, e.sentenceEnding);
        pitchGlideSpeed = e.voice.pitchGlideSpeed;
        // Primeira sílaba depois de um silêncio começa já no tom certo, sem deslizar a partir da fala anterior.
        if (!audioSource.isPlaying)
            audioSource.pitch = targetPitch;

        float volumeScale = e.voice.volume * Random.Range(1f - e.voice.volumeVariation, 1f);

        // PlayOneShot deixa o blip anterior terminar; Play() cortaria a onda no meio e geraria um clique.
        audioSource.volume = SoundManager.Instance.GetVolume();
        audioSource.PlayOneShot(audioClip, volumeScale);
    }

    private float GetPitch(DialogVoiceSO voice, int vowelIndex, char sentenceEnding)
    {
        float pitchOffset = Random.Range(-voice.pitchVariation, voice.pitchVariation);

        if (sentenceEnding == '?')
            pitchOffset += voice.questionPitchRise;
        else if (sentenceEnding == '!')
            pitchOffset += voice.exclamationPitchBoost;

        // Entonação natural das vogais: I soa mais agudo, O e U mais graves.
        if (vowelIndex == VOWEL_I)
            pitchOffset += VOWEL_PITCH_OFFSET;
        else if (vowelIndex == VOWEL_O || vowelIndex == VOWEL_U)
            pitchOffset -= VOWEL_PITCH_OFFSET;

        return Mathf.Clamp(voice.basePitch + pitchOffset, MIN_PITCH, MAX_PITCH);
    }

    // Só as primeiras vogais de cada palavra falam ("Cidadão" → I, A); tocar todas soa como metralhadora.
    private bool IsSpokenVowel(DialogVoiceSO voice, string line, int letterIndex)
    {
        return CountVowelsBeforeInWord(line, letterIndex) < voice.blipsPerWord;
    }

    // Volta letra a letra até o início da palavra (espaço ou pontuação) contando as vogais anteriores.
    private int CountVowelsBeforeInWord(string line, int letterIndex)
    {
        int vowelCount = 0;
        for (int i = letterIndex - 1; i >= 0 && char.IsLetter(line[i]); i--)
        {
            if (GetVowelIndex(line[i]) != CONSONANT_INDEX)
                vowelCount++;
        }
        return vowelCount;
    }

    private int GetVowelIndex(char letter)
    {
        char lowerLetter = char.ToLowerInvariant(letter);
        for (int i = 0; i < VOWEL_GROUPS.Length; i++)
        {
            if (VOWEL_GROUPS[i].IndexOf(lowerLetter) >= 0)
                return i;
        }
        return CONSONANT_INDEX;
    }
}
