using System;
using UnityEngine;

public class BarricadePoint : MonoBehaviour
{
    public enum BarricadeState { Open, Barricaded }

    public static event Action<BarricadePoint> OnBarricaded;

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
            OnBarricaded?.Invoke(this);
        }
    }

    public BarricadeState GetBarricadeState() => barricadeState;
}
