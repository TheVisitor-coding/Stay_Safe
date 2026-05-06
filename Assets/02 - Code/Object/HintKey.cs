using Unity.VisualScripting;
using UnityEngine;

public class HintKey : MonoBehaviour
{
    [SerializeField] private GameObject keypressHint;

    private bool _isGrabbed;

    public void SetGrabbed(bool grabbed)
    {
        _isGrabbed = grabbed;
        if (grabbed)
            keypressHint.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!_isGrabbed && other.CompareTag("Player") && (GameManager.Instance.GetGameState() == GameManager.GameState.Playing || GameManager.Instance.GetGameState() == GameManager.GameState.Tutorial))
        {
            keypressHint.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && (GameManager.Instance.GetGameState() == GameManager.GameState.Playing || GameManager.Instance.GetGameState() == GameManager.GameState.Tutorial))
        {
            keypressHint.SetActive(false);
        }
    }
}
