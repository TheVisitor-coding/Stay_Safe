using System;
using UnityEngine;

[Serializable]
public class DialogueLine
{
    [TextArea(2, 5)]
    public string text;
    public float displayDuration = 3f;  
    public float delayBefore = 0f;   
}