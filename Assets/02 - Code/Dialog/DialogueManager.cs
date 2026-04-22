using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [SerializeField] private DialogueDatabase database;
    public DialogueDatabase Database => database;
    [SerializeField] private TextMeshProUGUI dialogueText;
    // [SerializeField] private CanvasGroup dialogueCanvasGroup;
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float typewriterSpeed = 0.05f;
    [SerializeField] private AudioSource typewriterAudioSource;

    private Queue<DialogueLine> _queue = new();
    private Coroutine _displayCoroutine;
    private bool _isPlaying;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnEnable()
    {
        GameManager.OnGameStateChanged += OnGameStateChanged;
    }

    void OnDisable()
    {
        GameManager.OnGameStateChanged -= OnGameStateChanged;
    }

    public void Enqueue(DialogueLine[] lines)
    {
        Debug.Log($"[DialogueManager] Enqueue {lines.Length} lines");
        if (lines == null || lines.Length == 0) return;

        foreach (var line in lines)
            _queue.Enqueue(line);

        if (!_isPlaying)
            _displayCoroutine = StartCoroutine(PlayQueue());
    }

    public void EnqueuePriority(DialogueLine[] lines)
    {
        Debug.Log($"[DialogueManager] EnqueuePriority {lines.Length} lines");
        if (_displayCoroutine != null)
            StopCoroutine(_displayCoroutine);

        _queue.Clear();
        _isPlaying = false;

        Enqueue(lines);
    }

    private IEnumerator PlayQueue()
    {
        _isPlaying = true;

        try
        {
            while (_queue.Count > 0)
            {
                var line = _queue.Dequeue();

                if (line.delayBefore > 0)
                    yield return new WaitForSeconds(line.delayBefore);

                yield return StartCoroutine(DisplayLine(line));
            }
        }
        finally
        {
            _isPlaying = false;
            dialogueText.text = string.Empty;
        }
    }

    private IEnumerator DisplayLine(DialogueLine line)
    {
        dialogueText.text = string.Empty;

        foreach (char c in line.text)
        {
            dialogueText.text += c;

            if (typewriterAudioSource != null && typewriterAudioSource.clip != null && c != ' ')
                typewriterAudioSource.PlayOneShot(typewriterAudioSource.clip);

            yield return new WaitForSeconds(typewriterSpeed);
        }

        // yield return StartCoroutine(FadeIn());
        yield return new WaitForSeconds(line.displayDuration);

        dialogueText.text = string.Empty;
    }

    // private IEnumerator FadeIn()
    // {
    //     float t = 0;
    //     while (t < fadeDuration)
    //     {
    //         dialogueCanvasGroup.alpha = t / fadeDuration;
    //         t += Time.deltaTime;
    //         yield return null;
    //     }
    //     dialogueCanvasGroup.alpha = 1;
    // }

    // private IEnumerator FadeOut()
    // {
    //     float t = fadeDuration;
    //     while (t > 0)
    //     {
    //         dialogueCanvasGroup.alpha = t / fadeDuration;
    //         t -= Time.deltaTime;
    //         yield return null;
    //     }
    //     dialogueCanvasGroup.alpha = 0;
    // }

    private void OnGameStateChanged(GameManager.GameState state)
    {
        switch (state)
        {
            case GameManager.GameState.Exploration:
                Enqueue(database.onGameStart);
                break;
            case GameManager.GameState.Won:
                EnqueuePriority(database.onVictory);
                break;
            case GameManager.GameState.Lost:
                EnqueuePriority(database.onGameOver);
                break;
        }
    }
}