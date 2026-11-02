using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

public class DialogCombatManager : MonoBehaviour
{
    public static DialogCombatManager Instance { get; private set; }

    public event EventHandler OnCombatStarted;
    public event EventHandler OnStateChanged;
    public event EventHandler OnTypingMistake;
    // Mesmo formato do DialogManager.OnLetterTyped: o DialogVoiceSounds toca o mumble das duas fontes sem saber qual é.
    public event EventHandler<DialogManager.OnLetterTypedEventArgs> OnLetterTyped;
    public event EventHandler<OnRoundStartedEventArgs> OnRoundStarted;
    public class OnRoundStartedEventArgs : EventArgs
    {
        public DialogCharacter character;
        public string npcLine;
        public List<DialogCombatSO.Option> options;
    }
    public event EventHandler<OnTypingProgressChangedEventArgs> OnTypingProgressChanged;
    public class OnTypingProgressChangedEventArgs : EventArgs
    {
        public int optionIndex;
        public int typedLength;
        public bool isEliminated;
    }
    public event EventHandler<OnStressChangedEventArgs> OnStressChanged;
    public class OnStressChangedEventArgs : EventArgs
    {
        public float stressNormalized;
    }
    public event EventHandler<OnCombatEndedEventArgs> OnCombatEnded;
    public class OnCombatEndedEventArgs : EventArgs
    {
        public bool playerWon;
    }

    private enum State
    {
        Inactive,
        // "Ready? Set TYPE!": a barra não cai e as teclas são ignoradas, evitando erro por tecla apertada ao entrar.
        Intro,
        Typing,
    }

    [SerializeField] private float introDuration = 2.4f;
    [SerializeField] private DialogVoiceSO playerVoice;

    private State state;
    private DialogCombatSO dialogCombatSO;
    private List<DialogCombatSO.Option> currentOptions = new List<DialogCombatSO.Option>();
    // Progresso por opção: frases com o mesmo começo ("Não tenho..." / "Não sei...") avançam juntas até divergirem.
    private int[] typedLengths;
    private bool[] eliminatedOptions;
    private float stress;
    private float introTimer;
    // Cada round sai uma vez só; fila vazia = o player sobreviveu à conversa e vence.
    private readonly Queue<DialogCombatSO.Round> remainingRounds = new Queue<DialogCombatSO.Round>();

    private void Awake()
    {
        Instance = this;
        state = State.Inactive;
    }

    private void Start()
    {
        GameInput.Instance.OnTextInput += GameInput_OnTextInput;
    }

    // Time.deltaTime é 0 com o jogo pausado, então intro e barra congelam no pause sem checagem extra.
    private void Update()
    {
        switch (state)
        {
            case State.Intro:
                introTimer -= Time.deltaTime;
                if (introTimer <= 0f)
                {
                    SetState(State.Typing);
                    StartNextRound();
                }
                break;
            case State.Typing:
                SetStress(stress - dialogCombatSO.decayPerSecond * Time.deltaTime);
                break;
        }
    }

    public void StartCombat(DialogCombatSO dialogCombatSO)
    {
        if (IsCombatActive())
            return;

        this.dialogCombatSO = dialogCombatSO;
        introTimer = introDuration;

        List<DialogCombatSO.Round> shuffledRounds = new List<DialogCombatSO.Round>(dialogCombatSO.rounds);
        Shuffle(shuffledRounds);
        remainingRounds.Clear();
        foreach (DialogCombatSO.Round round in shuffledRounds)
            remainingRounds.Enqueue(round);

        SetState(State.Intro);
        OnCombatStarted?.Invoke(this, EventArgs.Empty);
        SetStress(dialogCombatSO.startStress);
    }

    // Resposta já completa de uma vez (ex: reconhecedor de voz no futuro). A digitação passa por GameInput_OnTextInput.
    public void SubmitPhrase(string phrase)
    {
        if (state != State.Typing)
            return;

        foreach (DialogCombatSO.Option option in currentOptions)
        {
            if (string.Equals(phrase.Trim(), option.phrase.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                ChooseOption(option);
                return;
            }
        }
    }

    public bool IsCombatActive()
    {
        return state != State.Inactive;
    }

    public bool IsIntroActive()
    {
        return state == State.Intro;
    }

    // 0 no começo da intro, 1 quando o "TYPE!" termina; a UI usa para saber qual palavra mostrar.
    public float GetIntroProgressNormalized()
    {
        return 1f - Mathf.Clamp01(introTimer / introDuration);
    }

    private void SetState(State newState)
    {
        state = newState;
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void GameInput_OnTextInput(object sender, char typedChar)
    {
        if (state != State.Typing || GameManager.Instance.IsGamePaused() || char.IsControl(typedChar))
            return;

        bool anyOptionMatches = false;
        for (int i = 0; i < currentOptions.Count; i++)
        {
            if (IsNextLetter(i, typedChar))
                anyOptionMatches = true;
        }

        // Letra errada não entra no texto: o player nunca precisa apagar nada.
        if (!anyOptionMatches)
        {
            HandleTypingMistake();
            return;
        }

        // Opções ainda vivas têm o mesmo texto até aqui, então qualquer uma serve de referência para a voz.
        int spokenOptionIndex = -1;
        int spokenLetterIndex = 0;

        for (int i = 0; i < currentOptions.Count; i++)
        {
            if (eliminatedOptions[i])
                continue;

            if (IsNextLetter(i, typedChar))
            {
                if (spokenOptionIndex == -1)
                {
                    spokenOptionIndex = i;
                    spokenLetterIndex = typedLengths[i];
                }
                typedLengths[i] = SkipPunctuation(currentOptions[i].phrase, typedLengths[i] + 1);
            }
            else
            {
                eliminatedOptions[i] = true;
            }

            InvokeTypingProgressChanged(i);
        }

        // Antes do ChooseOption: a última letra também fala, e a próxima rodada troca o currentOptions.
        InvokeLetterTyped(currentOptions[spokenOptionIndex].phrase, spokenLetterIndex);

        for (int i = 0; i < currentOptions.Count; i++)
        {
            if (!eliminatedOptions[i] && typedLengths[i] >= currentOptions[i].phrase.Length)
            {
                ChooseOption(currentOptions[i]);
                return;
            }
        }
    }

    // Usa a letra da frase, não a digitada: "nao" digitado ainda fala o "ã" com a entonação certa.
    private void InvokeLetterTyped(string phrase, int letterIndex)
    {
        OnLetterTyped?.Invoke(this, new DialogManager.OnLetterTypedEventArgs
        {
            voice = playerVoice,
            letter = phrase[letterIndex],
            line = phrase,
            letterIndex = letterIndex,
            sentenceEnding = DialogManager.GetSentenceEnding(phrase, letterIndex),
            speakAllVowels = true,
        });
    }

    private void HandleTypingMistake()
    {
        OnTypingMistake?.Invoke(this, EventArgs.Empty);
        SetStress(stress + dialogCombatSO.typingMistakePenalty);
    }

    private bool IsNextLetter(int optionIndex, char typedChar)
    {
        string phrase = currentOptions[optionIndex].phrase;
        int typedLength = typedLengths[optionIndex];
        return !eliminatedOptions[optionIndex]
            && typedLength < phrase.Length
            && NormalizeLetter(typedChar) == NormalizeLetter(phrase[typedLength]);
    }

    // Sem maiúsculas e sem acento: "nao" vale para "Não" e "c" vale para "ç".
    private char NormalizeLetter(char letter)
    {
        // FormD separa a letra base do acento ('ã' → 'a' + '~'); o primeiro caractere é a letra sem acento.
        return char.ToLowerInvariant(letter).ToString().Normalize(NormalizationForm.FormD)[0];
    }

    // Vírgula, ponto, "?" etc. são preenchidos sozinhos; o player só digita letras, números e espaços.
    private int SkipPunctuation(string phrase, int index)
    {
        while (index < phrase.Length && !char.IsLetterOrDigit(phrase[index]) && !char.IsWhiteSpace(phrase[index]))
            index++;
        return index;
    }

    private void ChooseOption(DialogCombatSO.Option option)
    {
        SetStress(stress + option.stressDelta);
        if (state == State.Typing)
            StartNextRound();
    }

    // Fisher-Yates: toda ordem tem a mesma chance, sem viés para o começo da lista.
    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private void StartNextRound()
    {
        if (remainingRounds.Count == 0)
        {
            EndCombat(true);
            return;
        }

        DialogCombatSO.Round round = remainingRounds.Dequeue();

        // Embaralha para a melhor resposta não ficar sempre na mesma posição da tela.
        currentOptions = new List<DialogCombatSO.Option>(round.options);
        Shuffle(currentOptions);

        typedLengths = new int[currentOptions.Count];
        eliminatedOptions = new bool[currentOptions.Count];
        for (int i = 0; i < currentOptions.Count; i++)
            typedLengths[i] = SkipPunctuation(currentOptions[i].phrase, 0);

        OnRoundStarted?.Invoke(this, new OnRoundStartedEventArgs
        {
            character = dialogCombatSO.character,
            npcLine = round.npcLine,
            options = currentOptions,
        });

        for (int i = 0; i < currentOptions.Count; i++)
            InvokeTypingProgressChanged(i);
    }

    private void InvokeTypingProgressChanged(int optionIndex)
    {
        OnTypingProgressChanged?.Invoke(this, new OnTypingProgressChangedEventArgs
        {
            optionIndex = optionIndex,
            typedLength = typedLengths[optionIndex],
            isEliminated = eliminatedOptions[optionIndex],
        });
    }

    private void SetStress(float value)
    {
        stress = Mathf.Clamp01(value);
        OnStressChanged?.Invoke(this, new OnStressChangedEventArgs { stressNormalized = stress });

        if (stress <= 0f)
            EndCombat(false);
        else if (stress >= 1f)
            EndCombat(true);
    }

    private void EndCombat(bool playerWon)
    {
        SetState(State.Inactive);
        OnCombatEnded?.Invoke(this, new OnCombatEndedEventArgs { playerWon = playerWon });

        if (playerWon)
            PlayerMovement.Instance.SetInteracting(false);
        else
            PlayerDeathManager.Instance.Kill(PlayerDeathManager.DeathCause.NpcAttack);
    }
}
