using System;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private BarricadePoint[] barricadePoints;
    [SerializeField] private KidnapperAI kidnapper;
    [SerializeField] private float policeArrivalTime = 300f;
    [SerializeField] private float timeForExploration = 60f;
    [SerializeField] private GameState currentGameState;
    [SerializeField] private TutorialManager tutorialManager;
    [SerializeField] private AudioClip tensionMusic;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private float musicFadeDuration = 2f;
    [SerializeField] private AudioSource yawnSFX;

    public enum GameState { Intro, Exploration, Tutorial, Playing, Won, Lost }

    public static event Action<float> OnTimerUpdated;
    public static event Action<GameState> OnGameStateChanged;
    public static event Action OnWakeUpPhase;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        KidnapperAI.OnAccessBreached += OnAccessBreached;
        KidnapperAI.OnAttackStarted += OnAttackStarted;
        KidnapperAI.OnTutorialAttackStarted += OnTutorialAttackReady;
        KidnapperAI.OnTutorialCompleted += OnTutorialCompleted;
        TutorialManager.OnTutorialCinematicFinished += OnCinematicFinished;
        BedTrigger.OnPlayerWokeUp += OnPlayerWokeUp;
    }

    void OnDestroy()
    {
        KidnapperAI.OnAccessBreached -= OnAccessBreached;
        KidnapperAI.OnAttackStarted -= OnAttackStarted;
        KidnapperAI.OnTutorialAttackStarted -= OnTutorialAttackReady;
        KidnapperAI.OnTutorialCompleted -= OnTutorialCompleted;
        TutorialManager.OnTutorialCinematicFinished -= OnCinematicFinished;
        BedTrigger.OnPlayerWokeUp -= OnPlayerWokeUp;
    }

    void Start()
    {
        currentGameState = GameState.Intro;
        Debug.Log($"[GameManager] Game state: {currentGameState}");
        OnGameStateChanged?.Invoke(currentGameState);
    }

    void Update()
    {
        if (currentGameState == GameState.Playing)
        {
            float timeRemaining = Mathf.Max(0, policeArrivalTime - Time.timeSinceLevelLoad);
            OnTimerUpdated?.Invoke(timeRemaining);
        }
    }

    public void StartExploration()
    {
        yawnSFX.Play();
        currentGameState = GameState.Exploration;
        OnGameStateChanged?.Invoke(currentGameState);
    }

    public GameState GetGameState() => currentGameState;

    public void StartTutorial()
    {
        currentGameState = GameState.Tutorial;
        Debug.Log($"[GameManager] Game state: {currentGameState}");
        OnGameStateChanged?.Invoke(currentGameState);
        StartCoroutine(StartTutorialAttackAfterDelay());
        DialogueManager.Instance.EnqueuePriority(
            DialogueManager.Instance.Database.onTutorialStart
        );

        IEnumerator StartTutorialAttackAfterDelay()
        {
            yield return new WaitForSeconds(5f);
            kidnapper.StartTutorialAttack();
        }
    }

    private void OnTutorialAttackReady()
    {
        Debug.Log("[GameManager] Kidnapper arrivé — lancement cinématique tutoriel");
        tutorialManager.PlayTutorialCinematic();
        StartCoroutine(CrossfadeMusic(tensionMusic));
    }

    private void OnCinematicFinished()
    {
        Debug.Log("[GameManager] Cinématique terminée — attente réveil joueur");
        OnWakeUpPhase?.Invoke();
    }

    private void OnPlayerWokeUp()
    {
        Debug.Log("[GameManager] Joueur réveillé — lancement forçage tutoriel");
        kidnapper.StartTutorialForcing();
    }

    private void OnTutorialCompleted()
    {
        Debug.Log("[GameManager] Tutoriel terminé — passage en Playing");
        StartPlaying();
    }

    private void StartPlaying()
    {
        currentGameState = GameState.Playing;
        Debug.Log($"[GameManager] Game state: {currentGameState}");
        OnGameStateChanged?.Invoke(currentGameState);
        kidnapper.Initialize(barricadePoints);
        kidnapper.StartAttack();
        Invoke(nameof(PoliceArrive), policeArrivalTime);
    }

    private void OnAttackStarted(BarricadePoint target)
    {
        Debug.Log($"[GameManager] Alerte sur {target.name}");
    }

    private void OnAccessBreached()
    {
        currentGameState = GameState.Lost;
        kidnapper.StopAttack();
        Debug.Log("[GameManager] GAME OVER");
        OnGameStateChanged?.Invoke(currentGameState);
    }

    private IEnumerator CrossfadeMusic(AudioClip newClip)
    {
        float startVolume = musicSource.volume;

        for (float t = 0; t < musicFadeDuration; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0f, t / musicFadeDuration);
            yield return null;
        }

        musicSource.clip = newClip;
        musicSource.Play();

        for (float t = 0; t < musicFadeDuration; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(0f, startVolume, t / musicFadeDuration);
            yield return null;
        }

        musicSource.volume = startVolume;
    }

    private void PoliceArrive()
    {
        if (currentGameState != GameState.Playing) return;
        currentGameState = GameState.Won;
        kidnapper.StopAttack();
        Debug.Log("[GameManager] Victoire !");
        OnGameStateChanged?.Invoke(currentGameState);
    }
}
