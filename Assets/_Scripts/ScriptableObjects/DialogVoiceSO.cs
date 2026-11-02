using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "DialogVoiceSO", menuName = "Scriptable Objects/DialogVoiceSO")]
public class DialogVoiceSO : ScriptableObject
{
    [Tooltip("Um clip por vogal na ordem A, E, I, O, U. Com menos de 5, os clips são reaproveitados em ciclo.")]
    public AudioClip[] vowelClips;
    [Tooltip("Quantas vogais do começo de cada palavra falam. Ex: 2 em \"Cidadão\" toca I, A.")]
    [FormerlySerializedAs("blipsPerSentence")]
    [Range(1, 3)] public int blipsPerWord = 2;
    [Range(0f, 1f)] public float volume = 0.6f;
    [Tooltip("Cada sílaba sai até essa fração mais baixa, para não soar repetitivo.")]
    [Range(0f, 0.5f)] public float volumeVariation = 0.2f;

    [Header("Pitch")]
    [Range(0.5f, 2f)] public float basePitch = 1f;
    [Tooltip("Variação aleatória para a fala não soar robótica.")]
    [Range(0f, 0.3f)] public float pitchVariation = 0.06f;
    [Tooltip("Quanto a voz sobe em frases que terminam com '?'.")]
    [Range(0f, 0.5f)] public float questionPitchRise = 0.2f;
    [Tooltip("Quanto a voz sobe em frases que terminam com '!'.")]
    [Range(0f, 0.5f)] public float exclamationPitchBoost = 0.1f;
    [Tooltip("Velocidade com que o pitch desliza até a próxima sílaba. Baixo = voz arrastada/cantada, alto = mais seca.")]
    [Range(1f, 40f)] public float pitchGlideSpeed = 12f;
}
