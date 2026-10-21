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

    [SerializeField] private Image characterIcon;
    [SerializeField] private TextMeshProUGUI characterName;
    [SerializeField] private TextMeshProUGUI dialogArea;
    [SerializeField] private Animator animator;
    [SerializeField] private float typingSpeed = 0.2f;
    [SerializeField] private float hideDelay = 1f;

    private readonly Queue<DialogLine> lines = new Queue<DialogLine>();
    private GameObject dialogBox;
    private bool isDialogActive;

    private void Awake()
    {
        Instance = this;
    }

    public void StartDialog(Dialog dialogue, GameObject dialogBox)
    {
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

    public void EndDialog()
    {
        isDialogActive = false;
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
        dialogArea.text = "";
        foreach (char letter in dialogLine.line)
        {
            dialogArea.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    private IEnumerator HideDialogBoxAfterDelay()
    {
        yield return new WaitForSeconds(hideDelay);
        dialogBox.SetActive(false);
    }
}
