using System.Collections;
using UnityEngine;

public class Dialog : CinematicStarter
{
    [SerializeField] public string NodeName;
    protected override IEnumerator StartCinematic()
    {
        dialogueRunner.StartDialogue(NodeName);
        yield return new WaitUntil(() => !dialogueRunner.IsDialogueRunning);
    }
}
