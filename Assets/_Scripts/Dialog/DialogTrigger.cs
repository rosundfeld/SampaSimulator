using System;
using UnityEngine;

public class DialogTrigger : MonoBehaviour
{
    private const string PLAYER_TAG = "Player";

    [SerializeField] private GameObject dialogAction;
    [SerializeField] private Dialog dialogue;
    [SerializeField] private GameObject dialogBox;

    [Tooltip("Inicia o diálogo sozinho quando o player entra no raio. Desmarcado: só inicia ao apertar Interagir [E]")]
    [SerializeField] private bool dialogWithoutInteraction = false;

    private bool isPlayerInRange;
    // Cópia de runtime do dialogWithoutInteraction: é consumida no auto-início e só volta quando o player sai do raio.
    private bool canAutoStart;

    private void Start()
    {
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;

        canAutoStart = dialogWithoutInteraction;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(PLAYER_TAG))
            return;

        isPlayerInRange = true;
        dialogAction.SetActive(true);

        if (canAutoStart)
        {
            canAutoStart = false;
            TriggerDialogue();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(PLAYER_TAG))
            return;

        isPlayerInRange = false;
        canAutoStart = dialogWithoutInteraction;
        dialogAction.SetActive(false);
    }

    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        if (!isPlayerInRange || !GameManager.Instance.IsGamePlaying() || GameManager.Instance.IsGamePaused())
            return;


        if (DialogManager.Instance.IsDialogActive())
        {
            DialogManager.Instance.DisplayNextDialogLine();
        }
        else
        {
            TriggerDialogue();
        }

    }

    public void TriggerDialogue()
    {
        // Checa antes de travar o player: se o StartDialog recusar, ninguém chamaria EndDialog para destravar.
        if (!DialogManager.Instance.CanStartDialog())
            return;

        PlayerMovement.Instance.SetInteracting(true);
        PlayerMovement.Instance.RotateTowardsTarget(transform);

        DialogManager.Instance.StartDialog(dialogue, dialogBox);
    }
}
