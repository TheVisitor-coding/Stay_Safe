using UnityEngine;
using UnityEngine.Playables;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private PlayableDirector playableDirector;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera cinematicCamera;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            PlayTutorialCinematic();
        }
    }

    public void PlayTutorialCinematic()
    {
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
    }
}
