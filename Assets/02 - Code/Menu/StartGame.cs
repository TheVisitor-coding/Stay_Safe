using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "House_Level";

    public void PlayGame()
    {
        Debug.Log("[StartGame] Loading game scene...");
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("[StartGame] Quitting game...");
        Application.Quit();
    }
}
