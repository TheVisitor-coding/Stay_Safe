using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PoliceTimer : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject canvasTimer;
    [SerializeField] private float glitchIntensity = 1.0f;

    private Material _material; 
    private Image _targetImage;


    void Awake()
    {
        _targetImage = canvasTimer.GetComponentInChildren<Image>();
        _material = _targetImage.material;

        GameManager.OnTimerUpdated += UpdateTimer;
        GameManager.OnGameStateChanged += OnGameStateChanged;
    }

    void OnDestroy()
    {
        GameManager.OnTimerUpdated -= UpdateTimer;
        GameManager.OnGameStateChanged -= OnGameStateChanged;
    }

    private void UpdateTimer(float timeRemaining)
    {
        float minutes = Mathf.FloorToInt(timeRemaining / 60); 
        float seconds = Mathf.FloorToInt(timeRemaining % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void OnGameStateChanged(GameManager.GameState state)
    {
        canvasTimer.SetActive(state == GameManager.GameState.Playing);
    }

    public void TriggerGlitchEffect(float glitchDuration)
    {
        StartCoroutine(GlitchRoutine(glitchDuration));
    }

    private IEnumerator GlitchRoutine(float glitchDuration)
    {
        float elapsed = 0f;

        while (elapsed < glitchDuration)
        {
            float intensity = Mathf.Lerp(glitchIntensity, 0f, elapsed / glitchDuration);
            _material.SetFloat("_GlitchIntensity", intensity);
            elapsed += Time.deltaTime;
            yield return null;
        }

        _material.SetFloat("_GlitchIntensity", 0f);
    }
}
