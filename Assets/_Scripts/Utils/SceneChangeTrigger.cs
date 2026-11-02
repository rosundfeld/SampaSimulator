using System;
using UnityEngine;

public class SceneChangeTrigger : MonoBehaviour
{
    private const string PLAYER_TAG = "Player";

    [Tooltip("Cena carregada quando o player aperta Interagir [E] dentro da área")]
    [SerializeField] private Loader.Scene targetScene;
    [Tooltip("Prompt mostrado enquanto o player está na área (ex: \"[E] Entrar\")")]
    [SerializeField] private GameObject sceneChangeAction;

    private bool isPlayerInRange;

    private void Start()
    {
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
        sceneChangeAction.SetActive(false);
    }

    private void OnDestroy()
    {
        // A troca de cena destrói este objeto; sem isso o GameInput ficaria chamando um trigger morto.
        if (GameInput.Instance != null)
            GameInput.Instance.OnInteractAction -= GameInput_OnInteractAction;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(PLAYER_TAG))
            return;

        isPlayerInRange = true;
        sceneChangeAction.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(PLAYER_TAG))
            return;

        isPlayerInRange = false;
        sceneChangeAction.SetActive(false);
    }

    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        if (!isPlayerInRange || !GameManager.Instance.IsGamePlaying() || GameManager.Instance.IsGamePaused())
            return;

        // Não troca de cena no meio de um diálogo ou combate (o "e" digitado no combate também dispara o Interagir).
        if (DialogCombatManager.Instance != null && DialogCombatManager.Instance.IsCombatActive())
            return;
        if (DialogManager.Instance != null && DialogManager.Instance.IsDialogActive())
            return;

        Loader.Load(targetScene);
    }
}
