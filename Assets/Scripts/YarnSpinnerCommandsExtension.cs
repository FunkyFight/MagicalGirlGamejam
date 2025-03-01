using System.Collections;
using UnityEngine;
using Yarn.Unity;

public class YarnSpinnerCommandsExtension : MonoBehaviour
{
    private static GameMediator mediator;

    private void Awake()
    {
        mediator = FindFirstObjectByType<GameMediator>();
    }

    [YarnCommand("movePlayer")]
    public static void MovePlayer(int xVelocity, int yVelocity, float duration)
    {
        Rigidbody2D controller = mediator.GetPlayerController().GetComponent<Rigidbody2D>();
        mediator.StartCoroutine(ApplyVelocityForDuration(controller, xVelocity, yVelocity, duration));
    }

    private static IEnumerator ApplyVelocityForDuration(Rigidbody2D controller, float xVelocity, float yVelocity, float duration)
    {
        Vector2 originalVelocity = controller.linearVelocity;
        controller.linearVelocity = new Vector2(xVelocity, yVelocity);

        yield return new WaitForSeconds(duration);

        controller.linearVelocity = originalVelocity;
    }
}
