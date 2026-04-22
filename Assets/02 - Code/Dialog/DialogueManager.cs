using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [SerializeField] private DialogueDatabase database;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private CanvasGroup dialogueCanvasGroup;
    [SerializeField] private float fadeDuration = 0.3f;

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
        if (lines == null || lines.Length == 0) return;

        foreach (var line in lines)
            _queue.Enqueue(line);

        if (!_isPlaying)
            _displayCoroutine = StartCoroutine(PlayQueue());
    }

    public void EnqueuePriority(DialogueLine[] lines)
    {
        if (_displayCoroutine != null)
            StopCoroutine(_displayCoroutine);

        _queue.Clear();
        _isPlaying = false;

        Enqueue(lines);
    }

    private IEnumerator PlayQueue()
    {
        _isPlaying = true;

        while (_queue.Count > 0)
        {
            var line = _queue.Dequeue();

            if (line.delayBefore > 0)
                yield return new WaitForSeconds(line.delayBefore);

            yield return StartCoroutine(DisplayLine(line));
        }

        _isPlaying = false;
        yield return StartCoroutine(FadeOut());
    }

    private IEnumerator DisplayLine(DialogueLine line)
    {
        dialogueText.text = line.text;
        yield return StartCoroutine(FadeIn());
        yield return new WaitForSeconds(line.displayDuration);
        yield return StartCoroutine(FadeOut());
    }

    private IEnumerator FadeIn()
    {
        float t = 0;
        while (t < fadeDuration)
        {
            dialogueCanvasGroup.alpha = t / fadeDuration;
            t += Time.deltaTime;
            yield return null;
        }
        dialogueCanvasGroup.alpha = 1;
    }

    private IEnumerator FadeOut()
    {
        float t = fadeDuration;
        while (t > 0)
        {
            dialogueCanvasGroup.alpha = t / fadeDuration;
            t -= Time.deltaTime;
            yield return null;
        }
        dialogueCanvasGroup.alpha = 0;
    }

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