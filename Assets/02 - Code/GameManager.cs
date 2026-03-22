using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public BarricadePoint[] barricadePoints;
    public float policeArrivalTime = 300f;
    public float timeForExploration = 60f;
    
    [SerializeField] private GameState currentGameState;

    public enum GameState { Exploration, Tutorial, Playing, Won, Lost };


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;    
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Invoke("StartTutorial", timeForExploration);
    }

    public GameState GetGameState()
    {
        return currentGameState;
    }

    private void StartTutorial()
    {
        currentGameState = GameState.Tutorial;
    }

    private void StartPlaying()
    {
        currentGameState = GameState.Playing;
        Invoke("PoliceArrive", policeArrivalTime);
    }
}
