using UnityEngine;

public class DialogueDatabase : MonoBehaviour
{
    [Header("Phase 1 - Exploration")]
    public DialogueLine[] onGameStart;
    public DialogueLine[] onFirstRoomDiscovered;

    [Header("Phase 2 - Premier assaut")]
    public DialogueLine[] onFirstAttack;
    public DialogueLine[] onFirstBarricadeSuccess;

    [Header("Phase 3-5 - Playing")]
    public DialogueLine[] onBarricadeBreached;
    public DialogueLine[] onAccessBreached;

    [Header("Fin")]
    public DialogueLine[] onVictory;
    public DialogueLine[] onGameOver;
}