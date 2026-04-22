using UnityEngine;

public class DialogueDatabase : MonoBehaviour
{
    [Header("Phase 1 - Exploration")]
    public DialogueLine[] onGameStart;

    [Header("Phase 2 - Premier assaut")]
    public DialogueLine[] onTutorialStart;
    public DialogueLine[] onFirstAttack;
    public DialogueLine[] onFirstBarricadeSuccess;

    [Header("Phase 3-5 - Playing")]
    public DialogueLine[] onBarricadeBreached;
    public DialogueLine[] onAccessBreached;

    [Header("Fin")]
    public DialogueLine[] onVictory;
    public DialogueLine[] onGameOver;
}