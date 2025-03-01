using System.Collections;
using UnityEngine;

public class Dialog_ch0_1 : CinematicStarter
{
    protected override IEnumerator StartCinematic()
    {
        dialogueRunner.StartDialogue("Gigi_Monologue");
        yield return new WaitUntil(() => !dialogueRunner.IsDialogueRunning);

        gameMediator.FadeToBlack(0.5f);
        yield return new WaitForSeconds(5f);

        dialogueRunner.StartDialogue("Gigi_Monologue_2");
        yield return new WaitUntil(() => !dialogueRunner.IsDialogueRunning);

        gameMediator.TeleportPlayerAndCameraInstantly(new Vector2(-10f, -68.5f));
        gameMediator.FadeFromBlack(5f);
        yield return new WaitForSeconds(7f);

        dialogueRunner.StartDialogue("Gigi_Monologue_3");
        yield return new WaitUntil(() => !dialogueRunner.IsDialogueRunning);

        gameMediator.SetPlayerSpeed(8);
    }
}
