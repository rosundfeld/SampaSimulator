using System;
using UnityEngine;

// Stamina da corrida. Não sabe nada de física: o PlayerMovement diz se está correndo e este componente gasta ou recupera.
public class PlayerStamina : MonoBehaviour
{
    public static PlayerStamina Instance { get; private set; }

    // Struct em vez de classe: o evento dispara quase todo frame enquanto a barra mexe, sem gerar lixo para o GC.
    public readonly struct StaminaChangedEventArgs
    {
        public readonly float StaminaNormalized;
        public readonly bool IsDraining;

        public StaminaChangedEventArgs(float staminaNormalized, bool isDraining)
        {
            StaminaNormalized = staminaNormalized;
            IsDraining = isDraining;
        }
    }

    public event EventHandler<StaminaChangedEventArgs> OnStaminaChanged;

    [SerializeField] private float maxStamina = 10f;
    [Tooltip("Stamina gasta por segundo correndo")]
    [SerializeField] private float staminaDrainRate = 0.75f;
    [Tooltip("Stamina recuperada por segundo depois do Regen Delay")]
    [SerializeField] private float staminaRegenRate = 0.5f;
    [Tooltip("Mínimo para COMEÇAR a correr; quem já está correndo continua até zerar")]
    [SerializeField] private float minStaminaToRun = 0.01f;
    [Tooltip("Segundos parado de correr antes de começar a recuperar")]
    [SerializeField] private float regenDelay = 2f;

    public float CurrentStamina { get; private set; }
    public float MaxStamina => maxStamina;

    private float regenTimer;
    private bool wasDraining;

    private void Awake()
    {
        Instance = this;
        CurrentStamina = maxStamina;
    }

    // Histerese: exige minStaminaToRun para começar, mas só para quando zera. Evita o liga-desliga no limite.
    public bool CanSprint(bool isAlreadySprinting)
    {
        return isAlreadySprinting ? CurrentStamina > 0f : CurrentStamina > minStaminaToRun;
    }

    // Chamado uma vez por passo de física pelo PlayerMovement.
    public void Tick(bool isSprinting, float deltaTime)
    {
        float previousStamina = CurrentStamina;

        if (isSprinting)
            Drain(deltaTime);
        else
            Regen(deltaTime);

        CurrentStamina = Mathf.Clamp(CurrentStamina, 0f, maxStamina);

        // Só avisa quando algo mudou; com a barra cheia e o player andando não há evento nenhum.
        if (!Mathf.Approximately(previousStamina, CurrentStamina) || wasDraining != isSprinting)
        {
            float normalized = maxStamina > 0f ? CurrentStamina / maxStamina : 0f;
            OnStaminaChanged?.Invoke(this, new StaminaChangedEventArgs(normalized, isSprinting));
        }

        wasDraining = isSprinting;
    }

    private void Drain(float deltaTime)
    {
        CurrentStamina -= staminaDrainRate * deltaTime;
        regenTimer = 0f;
    }

    private void Regen(float deltaTime)
    {
        if (CurrentStamina >= maxStamina)
            return;

        regenTimer += deltaTime;
        if (regenTimer >= regenDelay)
            CurrentStamina += staminaRegenRate * deltaTime;
    }
}
