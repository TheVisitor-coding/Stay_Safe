using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private BarricadePoint[] barricadePoints;
    [SerializeField] private KidnapperAI kidnapper;
    public float policeArrivalTime = 300f;
    [SerializeField] private float timeForExploration = 60f;
    [SerializeField] private GameState currentGameState;
    [SerializeField] private TutorialManager tutorialManager;

    public enum GameState { Intro, Exploration, Tutorial, Playing, Won, Lost }

    public static event Action<float> OnTimerUpdated;
    public static event Action<GameState> OnGameStateChanged;

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
        KidnapperAI.OnTutorialAttackStarted += startCinematicTutorial;
    }

    void OnDestroy()
    {
        KidnapperAI.OnAccessBreached -= OnAccessBreached;
        KidnapperAI.OnAttackStarted -= OnAttackStarted;
        KidnapperAI.OnTutorialAttackStarted -= startCinematicTutorial;
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
        currentGameState = GameState.Exploration;
        OnGameStateChanged?.Invoke(currentGameState);
        Debug.Log($"[GameManager] Game state: {currentGameState}");

        Invoke(nameof(StartTutorial), timeForExploration);
    }

    public GameState GetGameState() => currentGameState;

    private void StartTutorial()
    {
        currentGameState = GameState.Tutorial;
        Debug.Log($"[GameManager] Game state: {currentGameState}");
        OnGameStateChanged?.Invoke(currentGameState);
        kidnapper.StartTutorialAttack();
        Debug.Log($"[GameManager] Game state: {currentGameState}");
    }

    private void startCinematicTutorial()
    {
        Debug.Log("[GameManager] Démarrage du tutoriel cinématique");
        tutorialManager.PlayTutorialCinematic();
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
        // TODO : écran de défaite
    }

    private void PoliceArrive()
    {
        if (currentGameState != GameState.Playing) return;
        currentGameState = GameState.Won;
        kidnapper.StopAttack();
        Debug.Log("[GameManager] Victoire !");
        OnGameStateChanged?.Invoke(currentGameState);
        // TODO : écran de victoire
    }
}
