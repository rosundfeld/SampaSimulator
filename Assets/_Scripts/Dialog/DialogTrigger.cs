using UnityEngine;
using System.Collections.Generic;
using System;

[System.Serializable]
public class DialogCharacter
{
    public string name;
    public Sprite portrait;
}

[System.Serializable]
public class DialogLine
{
    public DialogCharacter character;
    [TextArea(3, 10)]
    public string line;
}

[System.Serializable]
public class Dialog
{
    public List<DialogLine> dialogueLines = new List<DialogLine>();
}

public class DialogTrigger : MonoBehaviour
{
    public GameObject dialogAction;
    public Dialog dialogue;
    public GameObject dialogBox;

    public void TriggerDialogue()
    {
        if (DialogManager.Instance == null)
        {
            Debug.LogError("DialogTrigger: DialogManager.Instance is null. Make sure a DialogManager exists in the scene.", this);
            return;
        }

        if (dialogue == null)
        {
            Debug.LogError("DialogTrigger: dialogue is not assigned.", this);
            return;
        }

        if (dialogBox == null)
        {
            Debug.LogError("DialogTrigger: dialogBox is not assigned.", this);
            return;
        }

        if (PlayerMovement.Instance != null)
        {
            PlayerMovement.Instance.SetInteracting(true);
            PlayerMovement.Instance.RotateTowardsTarget(this.transform);
        }

        DialogManager.Instance.StartDialog(dialogue, dialogBox);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //PlayerMovement.Instance.SetInteracting(true);
            dialogAction.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //PlayerMovement.Instance.SetInteracting(false);
            dialogAction.SetActive(false);
        }
    }
}
