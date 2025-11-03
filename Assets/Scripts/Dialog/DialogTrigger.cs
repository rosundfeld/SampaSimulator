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
    public Dialog dialogue;

    public void TriggerDialogue()
    {
        PlayerMovement.Instance.isInteracting = true;
        PlayerMovement.Instance.RotateTowardsTarget(this.transform);
        DialogManager.Instance.StartDialog(dialogue);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.tag);
        if (other.CompareTag("Player"))
        {
            TriggerDialogue();
        }
    }
}
