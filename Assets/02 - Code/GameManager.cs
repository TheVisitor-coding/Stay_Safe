using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private BarricadePoint[] barricadePoints;
    [SerializeField] private KidnapperAI kidnapper;
    [SerializeField] private float policeArrivalTime = 300f;
    [SerializeField] private float timeForExploration = 60f;
    [SerializeField] private GameState currentGameState;

    public enum GameState { Exploration, Tutorial, Playing, Won, Lost }

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
    }

    void OnDestroy()
    {
        KidnapperAI.OnAccessBreached -= OnAccessBreached;
        KidnapperAI.OnAttackStarted -= OnAttackStarted;
    }

    void Start()
    {
        currentGameState = GameState.Exploration;
        Invoke(nameof(StartTutorial), timeForExploration);
    }

    public GameState GetGameState() => currentGameState;

    private void StartTutorial()
    {
        currentGameState = GameState.Tutorial;
        StartPlaying();
    }

    private void StartPlaying()
    {
        currentGameState = GameState.Playing;
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
        // TODO : écran de défaite
    }

    private void PoliceArrive()
    {
        if (currentGameState != GameState.Playing) return;
        currentGameState = GameState.Won;
        kidnapper.StopAttack();
        Debug.Log("[GameManager] Victoire !");
        // TODO : écran de victoire
    }
}
