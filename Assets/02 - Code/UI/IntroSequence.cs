using System.Collections;
using UnityEngine;
using TMPro;

public class IntroSequence : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup overlayCanvasGroup;
    [SerializeField] private TextMeshProUGUI narrativeText;
    [SerializeField] private GameObject firstPersonController;
    [SerializeField] private GameObject cameraCanvas;

    [Header("Narrative")]
    [SerializeField] [TextArea(3, 10)] private string[] narrativeLines;

    [Header("Timing")]
    [SerializeField] private float typewriterSpeed = 0.05f;
    [SerializeField] private float pauseBetweenLines = 1.5f;
    [SerializeField] private float fadeOutDuration = 2f;
    [SerializeField] private float delayBeforeFade = 1f;
    [SerializeField] private bool skipIntro = false;

    [Header("Audio")]
    [SerializeField] private AudioSource typewriterAudioSource;

    void Start()
    {
        overlayCanvasGroup.alpha = 1f;
        narrativeText.text = "";
        firstPersonController.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (skipIntro)
        {
            cameraCanvas.SetActive(false);
            overlayCanvasGroup.gameObject.SetActive(false);
            firstPersonController.SetActive(true);
            GameManager.Instance.StartExploration();
        }
        else
        {
            StartCoroutine(PlayIntro());
        }
    }

    private IEnumerator PlayIntro()
    {
        yield return new WaitForSeconds(0.5f);

        foreach (string line in narrativeLines)
        {
            narrativeText.text = "";

            foreach (char c in line)
            {
                narrativeText.text += c;

                if (typewriterAudioSource != null && c != ' ')
                    typewriterAudioSource.PlayOneShot(typewriterAudioSource.clip);

                yield return new WaitForSeconds(typewriterSpeed);
            }

            yield return new WaitForSeconds(pauseBetweenLines);
        }

        yield return new WaitForSeconds(delayBeforeFade);

        // Fade out the black overlay
        float elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            overlayCanvasGroup.alpha = 1f - (elapsed / fadeOutDuration);
            yield return null;
        }

        overlayCanvasGroup.alpha = 0f;
        overlayCanvasGroup.gameObject.SetActive(false);

        // Enable player and start the game
        cameraCanvas.SetActive(false);
        firstPersonController.SetActive(true);
        GameManager.Instance.StartExploration();
    }
}
