using System.Collections;
using DG.Tweening;
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

    [YarnCommand("spotlight")]
    public static void Spotlight(float intensity, float outerRadius, float innerRadius)
    {
        DOTween.To(() => mediator.GetPlayerSpotlight().intensity, x => mediator.GetPlayerSpotlight().intensity = x, intensity, 1f);
        DOTween.To(() => mediator.GetPlayerSpotlight().pointLightOuterRadius, x => mediator.GetPlayerSpotlight().pointLightOuterRadius = x, outerRadius, 1f);
        DOTween.To(() => mediator.GetPlayerSpotlight().pointLightInnerRadius, x => mediator.GetPlayerSpotlight().pointLightInnerRadius = x, innerRadius, 1f);
    }

    [YarnCommand("globallight")]
    public static void GlobalLight(float intensity)
    {
        DOTween.To(() => mediator.GetGlobalLight().intensity, x => mediator.GetGlobalLight().intensity = x, intensity, 1f);
    }

    private static IEnumerator ApplyVelocityForDuration(Rigidbody2D controller, float xVelocity, float yVelocity, float duration)
    {
        Vector2 originalVelocity = controller.linearVelocity;
        controller.linearVelocity = new Vector2(xVelocity, yVelocity);

        yield return new WaitForSeconds(duration);

        controller.linearVelocity = originalVelocity;
    }
}
