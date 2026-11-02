using UnityEngine;

public class DialogCombatTrigger : MonoBehaviour
{
    private const string PLAYER_TAG = "Player";

    [SerializeField] private DialogCombatSO dialogCombatSO;

    // O manager é único e avisa todos os triggers; só quem iniciou o combate deve reagir ao fim dele.
    private bool isCurrentCombat;
    private bool hasBeenConvinced;

    private void Start()
    {
        DialogCombatManager.Instance.OnCombatEnded += DialogCombatManager_OnCombatEnded;
    }

    // NPCs vêm do spawner e são destruídos; sem isso o manager ficaria chamando um objeto morto.
    private void OnDestroy()
    {
        if (DialogCombatManager.Instance != null)
            DialogCombatManager.Instance.OnCombatEnded -= DialogCombatManager_OnCombatEnded;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(PLAYER_TAG) || !CanStartCombat())
            return;

        isCurrentCombat = true;
        PlayerMovement.Instance.SetInteracting(true);
        PlayerMovement.Instance.RotateTowardsTarget(transform);

        DialogCombatManager.Instance.StartCombat(dialogCombatSO);
    }

    private bool CanStartCombat()
    {
        return !hasBeenConvinced
            && GameManager.Instance.IsGamePlaying()
            && !DialogManager.Instance.IsDialogActive()
            && !DialogCombatManager.Instance.IsCombatActive();
    }

    private void DialogCombatManager_OnCombatEnded(object sender, DialogCombatManager.OnCombatEndedEventArgs e)
    {
        if (!isCurrentCombat)
            return;

        isCurrentCombat = false;
        hasBeenConvinced = e.playerWon;
    }
}
