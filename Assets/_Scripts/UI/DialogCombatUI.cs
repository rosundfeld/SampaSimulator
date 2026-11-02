using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogCombatUI : MonoBehaviour
{
    private const string ELIMINATED_ALPHA = "<alpha=#40>";

    [SerializeField] private Image characterIcon;
    [SerializeField] private TextMeshProUGUI characterName;
    [SerializeField] private TextMeshProUGUI npcLineText;
    [SerializeField] private TextMeshProUGUI[] optionTexts;
    [SerializeField] private Image stressBarImage;
    [SerializeField] private Color typedColor = new Color(0.3f, 0.85f, 0.35f);

    private List<DialogCombatSO.Option> currentOptions;
    private string typedColorHex;

    private void Awake()
    {
        typedColorHex = ColorUtility.ToHtmlStringRGB(typedColor);
    }

    private void Start()
    {
        DialogCombatManager.Instance.OnCombatStarted += DialogCombatManager_OnCombatStarted;
        DialogCombatManager.Instance.OnRoundStarted += DialogCombatManager_OnRoundStarted;
        DialogCombatManager.Instance.OnTypingProgressChanged += DialogCombatManager_OnTypingProgressChanged;
        DialogCombatManager.Instance.OnStressChanged += DialogCombatManager_OnStressChanged;
        DialogCombatManager.Instance.OnCombatEnded += DialogCombatManager_OnCombatEnded;

        Hide();
    }

    private void DialogCombatManager_OnCombatStarted(object sender, EventArgs e)
    {
        Show();
    }

    private void DialogCombatManager_OnRoundStarted(object sender, DialogCombatManager.OnRoundStartedEventArgs e)
    {
        characterIcon.gameObject.SetActive(e.character.portrait != null);
        characterIcon.sprite = e.character.portrait;
        characterName.text = e.character.name;
        npcLineText.text = e.npcLine;

        currentOptions = e.options;
        for (int i = 0; i < optionTexts.Length; i++)
            optionTexts[i].gameObject.SetActive(i < currentOptions.Count);
    }

    private void DialogCombatManager_OnTypingProgressChanged(object sender, DialogCombatManager.OnTypingProgressChangedEventArgs e)
    {
        if (e.optionIndex >= optionTexts.Length)
            return;

        string phrase = currentOptions[e.optionIndex].phrase;
        if (e.isEliminated)
        {
            optionTexts[e.optionIndex].text = ELIMINATED_ALPHA + phrase;
            return;
        }

        string typedPart = phrase.Substring(0, e.typedLength);
        string remainingPart = phrase.Substring(e.typedLength);
        optionTexts[e.optionIndex].text = $"<color=#{typedColorHex}>{typedPart}</color>{remainingPart}";
    }

    private void DialogCombatManager_OnStressChanged(object sender, DialogCombatManager.OnStressChangedEventArgs e)
    {
        stressBarImage.fillAmount = e.stressNormalized;
    }

    private void DialogCombatManager_OnCombatEnded(object sender, DialogCombatManager.OnCombatEndedEventArgs e)
    {
        Hide();
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
