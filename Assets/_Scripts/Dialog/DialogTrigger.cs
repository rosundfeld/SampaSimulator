using UnityEngine;

public class DialogTrigger : MonoBehaviour
{
    private const string PLAYER_TAG = "Player";

    [SerializeField] private GameObject dialogAction;
    [SerializeField] private Dialog dialogue;
    [SerializeField] private GameObject dialogBox;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(PLAYER_TAG))
            return;

        TriggerDialogue();
        dialogAction.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(PLAYER_TAG))
            return;

        dialogAction.SetActive(false);
    }

    public void TriggerDialogue()
    {
        PlayerMovement.Instance.SetInteracting(true);
        PlayerMovement.Instance.RotateTowardsTarget(transform);

        DialogManager.Instance.StartDialog(dialogue, dialogBox);
    }
}
