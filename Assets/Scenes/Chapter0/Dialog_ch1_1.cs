using System.Collections;
using UnityEngine;

public class Dialog_ch1_1 : CinematicStarter
{
    protected override IEnumerator StartCinematic()
    {
        dialogueRunner.StartDialogue("Deeper");
        yield return new WaitUntil(() => !dialogueRunner.IsDialogueRunning);
    }
}
