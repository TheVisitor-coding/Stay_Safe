using System;
using UnityEngine;
using UnityEngine.Playables;

public class TutorialManager : MonoBehaviour
{
    public static event Action OnTutorialCinematicFinished;

    [SerializeField] private PlayableDirector playableDirector;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera cinematicCamera;
    [SerializeField] private AudioSource breathingSFX;

    public void PlayTutorialCinematic()
    {
        breathingSFX.PlayDelayed(0.5f);
        mainCamera.gameObject.SetActive(false);
        cinematicCamera.gameObject.SetActive(true);
        playableDirector.Play();
        playableDirector.stopped += OnCinematicFinished;
    }

    private void OnCinematicFinished(PlayableDirector director)
    {
        cinematicCamera.gameObject.SetActive(false);
        mainCamera.gameObject.SetActive(true);
        playableDirector.stopped -= OnCinematicFinished;
        OnTutorialCinematicFinished?.Invoke();
    }
}
