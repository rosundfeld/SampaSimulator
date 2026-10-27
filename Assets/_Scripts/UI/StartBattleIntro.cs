using System;
using TMPro;
using UnityEngine;

public class StartBattleIntro : MonoBehaviour
{
    private const string INTRO_ANIMATION = "CombatIntroTextAnimation";
    private static readonly string[] INTRO_WORDS = { "READY?", "SET", "TYPE!" };

    [SerializeField] private TextMeshProUGUI startBattleText;
    [SerializeField] private Animator animator;

    private int currentWordIndex = -1;

    private void Start()
    {
        DialogCombatManager.Instance.OnStateChanged += DialogCombatManager_OnStateChanged;

        // Filho do DialogCombatUI: se o pai se esconder primeiro, este Start só roda quando o combate abre,
        // já no meio da intro. Sincronizar com o estado atual em vez de um Hide() cego evita sumir com ela.
        UpdateVisibility();
    }

    // O tempo da intro é do manager; aqui só se traduz o progresso na palavra da vez.
    private void Update()
    {
        float introProgress = DialogCombatManager.Instance.GetIntroProgressNormalized();
        int wordIndex = Mathf.Min((int)(introProgress * INTRO_WORDS.Length), INTRO_WORDS.Length - 1);
        if (wordIndex == currentWordIndex)
            return;

        currentWordIndex = wordIndex;
        startBattleText.text = INTRO_WORDS[wordIndex];
        // normalizedTime 0 reinicia o clip, senão a segunda palavra apareceria sem o "pop".
        animator.Play(INTRO_ANIMATION, 0, 0f);
    }

    private void DialogCombatManager_OnStateChanged(object sender, EventArgs e)
    {
        UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        if (DialogCombatManager.Instance.IsIntroActive())
        {
            currentWordIndex = -1;
            Show();
        }
        else
        {
            Hide();
        }
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
