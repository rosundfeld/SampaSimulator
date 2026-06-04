using TMPro;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    public static DialogManager Instance;

    public Image characterIcon;
    public TextMeshProUGUI characterName;
    public TextMeshProUGUI dialogArea;
	private Queue<DialogLine> lines = new Queue<DialogLine>();

    public bool isDialogActive = false;

    public float typingSpeed = 0.2f;

    public Animator animator;

    void Start()
    {
        if (Instance == null)
            Instance = this;
    }

    public void StartDialog(Dialog dialogue)
    {
        isDialogActive = true;

		animator.Play("show");

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        lines.Clear();

        foreach (DialogLine dialogueLine in dialogue.dialogueLines)
        {
            lines.Enqueue(dialogueLine);
        }

        DisplayNextDialogLine();
    }

    public void DisplayNextDialogLine()
    {
        if(lines.Count == 0)
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

    IEnumerator<WaitForSeconds> TypeSentence(DialogLine dialogLine)
    {
        dialogArea.text = "";
        foreach(char letter in dialogLine.line.ToCharArray())
        {
            dialogArea.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    public void EndDialog()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        PlayerMovement.Instance.isInteracting = false;
		isDialogActive = false;
        animator.Play("hide");
    }
}
