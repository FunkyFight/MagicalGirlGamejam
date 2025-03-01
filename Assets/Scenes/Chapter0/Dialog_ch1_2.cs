using System.Collections;
using UnityEngine;

public class Dialog_ch1_2 : CinematicStarter
{
    protected override IEnumerator StartCinematic()
    {
        dialogueRunner.StartDialogue("Gigi_monologue");
        yield return new WaitUntil(() => !dialogueRunner.IsDialogueRunning);
    }
}
