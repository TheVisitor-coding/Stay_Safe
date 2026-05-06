using System;
using TMPro;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public static GameManager Instance;
    [SerializeField] private Button restartButton;
    [SerializeField] private float timeForExploration = 60f;
    [SerializeField] private GameState currentGameState;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private BarricadePoint[] barricadePoints;
    [SerializeField] private BarricadePoint firstAccessPoint;
    [SerializeField] private KidnapperAI kidnapper;
    [SerializeField] private float policeArrivalTime = 300f;
    [SerializeField] private AudioSource introMusicSource;

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

    // TEMPORAIRE — pour tester, à supprimer ensuite
    [ContextMenu("Test Game Over")]
    public void DEBUG_GameOver()
    {
        currentGameState = GameState.Playing; // force l'état
        OnAccessBreached();
    }

    [ContextMenu("Test You Win")]
    public void DEBUG_YouWin()
    {
        currentGameState = GameState.Playing; // bypass le guard
        PoliceArrive();
    }
    [SerializeField] private TutorialManager tutorialManager;
    [SerializeField] private AudioClip tensionMusic;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private float musicFadeDuration = 2f;
    [SerializeField] private AudioSource yawnSFX;
    [SerializeField] private AudioSource assaultAlertSFX;

    private float _playingStartTime;

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
        BarricadePoint.OnBarricaded += OnBarricadePointSecured;
    }

    void OnDestroy()
    {
        KidnapperAI.OnAccessBreached -= OnAccessBreached;
        KidnapperAI.OnAttackStarted -= OnAttackStarted;
        KidnapperAI.OnTutorialAttackStarted -= OnTutorialAttackReady;
        KidnapperAI.OnTutorialCompleted -= OnTutorialCompleted;
        TutorialManager.OnTutorialCinematicFinished -= OnCinematicFinished;
        BedTrigger.OnPlayerWokeUp -= OnPlayerWokeUp;
        BarricadePoint.OnBarricaded -= OnBarricadePointSecured;
    }

    void Start()
    {
        currentGameState = GameState.Intro;
        Debug.Log($"[GameManager] Game state: {currentGameState}");
        OnGameStateChanged?.Invoke(currentGameState);

        foreach (BarricadePoint bp in barricadePoints)
        {
            Collider col = bp.GetComponent<Collider>();
            if (col != null) col.enabled = false;
        }

        if (firstAccessPoint != null)
        {
            Collider firstCol = firstAccessPoint.GetComponent<Collider>();
            if (firstCol != null) firstCol.enabled = true;
        }
    }

    void Update()
    {
        if (currentGameState == GameState.Playing)
        {
            float timeRemaining = Mathf.Max(0, policeArrivalTime - (Time.time - _playingStartTime));
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

        foreach (BarricadePoint bp in barricadePoints)
        {
            Collider col = bp.GetComponent<Collider>();
            if (col != null) col.enabled = true;
        }

        kidnapper.Initialize(barricadePoints);
        kidnapper.StartAttack();
        _playingStartTime = Time.time;
        Invoke(nameof(PoliceArrive), policeArrivalTime);
    }

    private void OnAttackStarted(BarricadePoint target)
    {
        Debug.Log($"[GameManager] Alerte sur {target.name}");
        if (assaultAlertSFX != null) assaultAlertSFX.Play();
    }

    private void OnBarricadePointSecured(BarricadePoint point)
    {
        if (currentGameState != GameState.Playing) return;
        if (!AreAllBarricaded()) return;

        Debug.Log("[GameManager] Toutes les barricades sont en place — victoire anticipée");
        CancelInvoke(nameof(PoliceArrive));
        kidnapper.StopAttack();
        DialogueManager.Instance.EnqueuePriority(
            DialogueManager.Instance.Database.onAllBarricaded
        );
        StartCoroutine(EarlyVictoryAfterDialogue());
    }

    private bool AreAllBarricaded()
    {
        foreach (BarricadePoint bp in barricadePoints)
        {
            if (bp.GetBarricadeState() == BarricadePoint.BarricadeState.Open) return false;
        }
        return true;
    }

    private IEnumerator EarlyVictoryAfterDialogue()
    {
        yield return new WaitForSeconds(3f);
        PoliceArrive();
    }

    private void OnAccessBreached()
    {
        if (currentGameState == GameState.Lost) return;
        DialogueManager.Instance.EnqueuePriority(
            DialogueManager.Instance.Database.onAccessBreached
        );
        Debug.Log("[GameManager] Accès compromis — lancement séquence de game over");
        currentGameState = GameState.Lost;
        kidnapper.StopAttack();
        CancelInvoke(nameof(PoliceArrive));
        if (musicSource != null && musicSource.isPlaying) musicSource.Stop();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        OnGameStateChanged?.Invoke(currentGameState);
        Debug.Log("[GameManager] Lancement séquence de game over");
        StartCoroutine(GameOverSequence());
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

    private System.Collections.IEnumerator GameOverSequence()
    {
        foreach (AudioSource source in GetComponents<AudioSource>())
        {
            source.Stop();
        }
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
        if (musicSource != null && musicSource.isPlaying) musicSource.Stop();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
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

        foreach (AudioSource source in GetComponents<AudioSource>())
        {
            source.Stop();
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
                    Debug.Log($"[GameManager] Adjusting light {houseLights[i].name} intensity: {houseLights[i].intensity} -> {Mathf.Lerp(startIntensities[i], normalLightIntensity, progress)}");
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
        Debug.Log("[GameManager] RestartGame appelé !"); // ← temporaire
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
