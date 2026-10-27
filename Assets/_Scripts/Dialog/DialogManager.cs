using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    private const string SHOW_ANIMATION = "show";
    private const string HIDE_ANIMATION = "hide";

    public static DialogManager Instance { get; private set; }

    public event EventHandler<OnLetterTypedEventArgs> OnLetterTyped;
    public class OnLetterTypedEventArgs : EventArgs
    {
        public DialogVoiceSO voice;
        public char letter;
        public string line;
        public int letterIndex;
        // '.', '!' ou '?' que fecha a frase atual ('\0' se não houver), para a voz já subir no começo de uma pergunta.
        public char sentenceEnding;
        // Ignora o blipsPerWord: no combate cada vogal digitada é um feedback de acerto, então todas falam.
        public bool speakAllVowels;
    }

    [SerializeField] private Image characterIcon;
    [SerializeField] private TextMeshProUGUI characterName;
    [SerializeField] private TextMeshProUGUI dialogArea;
    [SerializeField] private Animator animator;
    [SerializeField] private float typingSpeed = 0.2f;
    [SerializeField] private float commaPause = 0.15f;
    [SerializeField] private float sentencePause = 0.35f;
    [SerializeField] private float hideDelay = 1f;
    [SerializeField] private float restartCooldown = 2f;

    private readonly Queue<DialogLine> lines = new Queue<DialogLine>();
    private GameObject dialogBox;
    private bool isDialogActive;
    private float lastDialogEndTime = float.NegativeInfinity;

    private void Awake()
    {
        Instance = this;
    }

    public void StartDialog(Dialog dialogue, GameObject dialogBox)
    {
        if (!CanStartDialog())
            return;
            
        this.dialogBox = dialogBox;
        this.dialogBox.SetActive(true);
        isDialogActive = true;

        animator.Play(SHOW_ANIMATION);
        CursorUtils.ShowCursor();

        lines.Clear();
        foreach (DialogLine dialogLine in dialogue.dialogueLines)
            lines.Enqueue(dialogLine);

        DisplayNextDialogLine();
    }

    // Chamado pelo botão "próximo" da caixa de diálogo (OnClick no Inspector).
    public void DisplayNextDialogLine()
    {
        if (lines.Count == 0)
        {
            EndDialog();
            return;
        }

        DialogLine currentLine = lines.Dequeue();

        characterIcon.gameObject.SetActive(currentLine.character.portrait != null);
        characterIcon.sprite = currentLine.character.portrait;
        characterName.text = currentLine.character.name;

        StopAllCoroutines();
        StartCoroutine(TypeSentence(currentLine));
    }

    public bool CanStartDialog()
    {
        return !isDialogActive && Time.time - lastDialogEndTime >= restartCooldown;
    }

    public void EndDialog()
    {
        isDialogActive = false;
        lastDialogEndTime = Time.time;
        CursorUtils.HideCursor();
        PlayerMovement.Instance.SetInteracting(false);

        animator.Play(HIDE_ANIMATION);
        StartCoroutine(HideDialogBoxAfterDelay());
    }

    public bool IsDialogActive()
    {
        return isDialogActive;
    }

    private IEnumerator TypeSentence(DialogLine dialogLine)
    {
        string line = dialogLine.line;
        dialogArea.text = "";
        for (int i = 0; i < line.Length; i++)
        {
            char letter = line[i];
            dialogArea.text += letter;

            OnLetterTyped?.Invoke(this, new OnLetterTypedEventArgs
            {
                voice = dialogLine.character.voice,
                letter = letter,
                line = line,
                letterIndex = i,
                sentenceEnding = GetSentenceEnding(line, i),
            });

            yield return new WaitForSeconds(GetDelayAfter(letter));
        }
    }

    private float GetDelayAfter(char letter)
    {
        switch (letter)
        {
            case ',':
            case ';':
            case ':':
                return commaPause;
            case '.':
            case '!':
            case '?':
                return sentencePause;
            default:
                return typingSpeed;
        }
    }

    public static char GetSentenceEnding(string line, int index)
    {
        for (int i = index; i < line.Length; i++)
        {
            if (line[i] == '.' || line[i] == '!' || line[i] == '?')
                return line[i];
        }
        return '\0';
    }

    private IEnumerator HideDialogBoxAfterDelay()
    {
        yield return new WaitForSeconds(hideDelay);
        dialogBox.SetActive(false);
    }
}
