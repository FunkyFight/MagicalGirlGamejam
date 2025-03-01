using System.Collections;
using UnityEngine;

public class Dialog : CinematicStarter
{
    [SerializeField] public string DialogName;
    protected override IEnumerator StartCinematic()
    {
        dialogueRunner.StartDialogue(DialogName);
        yield return new WaitUntil(() => !dialogueRunner.IsDialogueRunning);
    }
}
