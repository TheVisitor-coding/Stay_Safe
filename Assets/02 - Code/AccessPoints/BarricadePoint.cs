using UnityEngine;

public class BarricadePoint : MonoBehaviour
{
    public enum BarricadeState { Open, Barricaded }

    [SerializeField] private BarricadeState barricadeState = BarricadeState.Open;
    [SerializeField] private Transform snapPosition;

    public void Barricade(GameObject barricadeObject)
    {
        if (barricadeState == BarricadeState.Open)
        {
            barricadeObject.transform.position = snapPosition.position;
            barricadeObject.transform.rotation = snapPosition.rotation;
            barricadeObject.transform.SetParent(transform);
            barricadeState = BarricadeState.Barricaded;
        }
    }

    public BarricadeState GetBarricadeState() => barricadeState;
}
