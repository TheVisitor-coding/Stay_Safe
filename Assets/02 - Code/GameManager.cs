using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public static GameManager Instance;
    public Button restartButton;
    [SerializeField] private float timeForExploration = 60f;
    [SerializeField] private GameState currentGameState;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private BarricadePoint[] barricadePoints;
    [SerializeField] private KidnapperAI kidnapper;
    [SerializeField] private float policeArrivalTime = 300f;

    [Header("Game Over Settings")]
    public TextMeshProUGUI gameOverText;
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 2f;
    [SerializeField] private AudioSource creepyMusicSource;
    [SerializeField] private float maxDetectionDistance = 20f;
    [SerializeField] private float minDistanceForMaxVolume = 2f;


    [Header("Victory Settings")]
    public TextMeshProUGUI winText;
    [SerializeField] private AudioSource victoryMusicSource;
    [SerializeField] private Light[] houseLights;
    [SerializeField] private float normalLightIntensity = 1.5f;

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
        if (creepyMusicSource != null)
        {
            creepyMusicSource.volume = 0f;
            creepyMusicSource.Play();
        }

       if(fadeImage != null)
       {
        fadeImage.gameObject.SetActive(true);
        float elapsedTime = 0f;
        Color color = fadeImage.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / fadeDuration);
            color.a = progress;
            fadeImage.color = color;

            if (creepyMusicSource != null)
            {
                creepyMusicSource.volume = progress;
            }

            yield return null;
        }

       }

       if (gameOverText != null) gameOverText.gameObject.SetActive(true);
       if (restartButton != null) restartButton.gameObject.SetActive(true);

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

        StartCoroutine(VictorySequence());
    }

    private System.Collections.IEnumerator VictorySequence()
    {
        float victoryFadeDuration = 4f;
        float elapsedTime = 0f;
        float[] startIntensities = new float[houseLights.Length];
        for (int i = 0; i < houseLights.Length; i++)
        {
            if(houseLights[i] != null) startIntensities[i] = houseLights[i].intensity;
            
        }

        if (victoryMusicSource != null)
        {
        victoryMusicSource.volume = 0f;
        victoryMusicSource.Play();
        }

    while (elapsedTime < victoryFadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / victoryFadeDuration);

            if (victoryMusicSource != null) victoryMusicSource.volume = progress;

            for (int i = 0; i < houseLights.Length; i++)
            {
                if(houseLights[i] != null) 
                {
                    houseLights[i].intensity = Mathf.Lerp(startIntensities[i], normalLightIntensity, progress);
                }
            }

            yield return null;
        }

        if (winText != null) winText.gameObject.SetActive(true);
        if (restartButton != null) restartButton.gameObject.SetActive(true);

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;   
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        if (creepyMusicSource != null)
        {
            creepyMusicSource.Stop();
        }

        if (victoryMusicSource != null)
        {
            victoryMusicSource.Stop();
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);    
    }
}
