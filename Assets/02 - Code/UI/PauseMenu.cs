using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuCanvas;
    [SerializeField] private Button resumeButton;

    private bool _isPaused;
    private CursorLockMode _previousLockState;
    private bool _previousCursorVisible;

    void Start()
    {
        pauseMenuCanvas.SetActive(false);
    }

    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;

        GameManager.GameState state = GameManager.Instance.GetGameState();
        if (state == GameManager.GameState.Won || state == GameManager.GameState.Lost) return;

        if (_isPaused) Resume();
        else Pause();
    }

    private void Pause()
    {
        _previousLockState = Cursor.lockState;
        _previousCursorVisible = Cursor.visible;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        pauseMenuCanvas.SetActive(true);
        Time.timeScale = 0f;
        _isPaused = true;
    }

    public void Resume()
    {
        Debug.Log("Resuming game...");
        pauseMenuCanvas.SetActive(false);
        Time.timeScale = 1f;

        Cursor.lockState = _previousLockState;
        Cursor.visible = _previousCursorVisible;

        _isPaused = false;
    }
}
