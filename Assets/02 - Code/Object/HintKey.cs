using UnityEngine;

public class HintKey : MonoBehaviour
{
    [SerializeField] private GameObject keypressHint;

    public void Hide()
    {
        keypressHint.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            keypressHint.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            keypressHint.SetActive(false);
        }
    }
}
