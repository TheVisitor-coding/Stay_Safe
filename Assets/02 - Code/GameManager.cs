using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public TextMeshProUGUI gameOverText;
    public Button restartButton;
    public static GameManager Instance;

    [SerializeField] private BarricadePoint[] barricadePoints;
    [SerializeField] private KidnapperAI kidnapper;
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 2f;
    public float policeArrivalTime = 300f;
    [SerializeField] private float timeForExploration = 60f;
    [SerializeField] private GameState currentGameState;

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
    }

    void OnDestroy()
    {
        KidnapperAI.OnAccessBreached -= OnAccessBreached;
        KidnapperAI.OnAttackStarted -= OnAttackStarted;
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
        OnGameStateChanged?.Invoke(currentGameState);
        Debug.Log($"[GameManager] Game state: {currentGameState}");
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
        if (currentGameState == GameState.Lost) return;
        currentGameState = GameState.Lost;
        kidnapper.StopAttack();
        Debug.Log("[GameManager] GAME OVER");
        OnGameStateChanged?.Invoke(currentGameState);
        
        StartCoroutine(GameOverSequence());

    }

    private System.Collections.IEnumerator GameOverSequence()
    {
       if(fadeImage != null)
       {
        fadeImage.gameObject.SetActive(true);
        float elapsedTime = 0f;
        Color color = fadeImage.color;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

       }
       if (gameOverText != null) gameOverText.gameObject.SetActive(true);
       if (restartButton != null) restartButton.gameObject.SetActive(true);

        Time.timeScale = 0f; 
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void GameOver()
    {
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true);
            restartButton.gameObject.SetActive(true);
        }

        Time.timeScale = 0f; 
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

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

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        
    }
}
