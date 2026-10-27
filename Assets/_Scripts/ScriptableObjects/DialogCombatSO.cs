using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogCombatSO", menuName = "Scriptable Objects/DialogCombatSO")]
public class DialogCombatSO : ScriptableObject
{
    [Serializable]
    public class Option
    {
        public string phrase;
        [Tooltip("Positivo enche a barra (convence o NPC), negativo esvazia. Ex: +0.1 pouco, +0.3 muito, -0.25 ruim")]
        public float stressDelta;
    }

    [Serializable]
    public class Round
    {
        [TextArea(2, 5)]
        public string npcLine;
        public List<Option> options = new List<Option>();
    }

    public DialogCharacter character;
    public List<Round> rounds = new List<Round>();

    [Header("Stress Bar (0 = player morre, 1 = NPC desiste)")]
    [Range(0f, 1f)] public float startStress = 0.5f;
    public float decayPerSecond = 0.04f;

    [Tooltip("Penalidade aplicada ao stress quando o jogador comete um erro de digitação.")]
    [Range(-1f, 0f)] public float typingMistakePenalty = -0.1f;
}
