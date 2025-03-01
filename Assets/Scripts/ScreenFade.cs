using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ScreenFade : MonoBehaviour
{
    public Canvas fadeCanvas;            // Canvas to hold the fade image
    public CanvasGroup canvasGroup;      // CanvasGroup to control the alpha
    private Image blackImage;             // Image to cover the screen

    void Awake()
    {
        // Create a new Canvas
        fadeCanvas = new GameObject("FadeCanvas").AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = int.MaxValue; // Ensure it's on top

        // Create a CanvasGroup for the Canvas
        canvasGroup = fadeCanvas.gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 1f; // Start fully opaque

        // Create a black Image to cover the screen
        blackImage = new GameObject("BlackImage").AddComponent<Image>();
        blackImage.color = new Color(0, 0, 0);
        blackImage.rectTransform.SetParent(fadeCanvas.transform, false);
        blackImage.rectTransform.anchorMin = Vector2.zero;
        blackImage.rectTransform.anchorMax = Vector2.one;
        blackImage.rectTransform.sizeDelta = Vector2.zero;
    }

    // Call this method to fade to black over the specified duration
    public void FadeToBlack(float duration, System.Action onComplete = null)
    {
        DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 1f, duration)
               .SetEase(Ease.InOutQuad)
               .OnComplete(() => onComplete?.Invoke());
    }

    // Optional: to fade from black to transparency
    public void FadeFromBlack(float duration, System.Action onComplete = null)
    {
        DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 0f, duration)
               .SetEase(Ease.InOutQuad)
               .OnComplete(() => onComplete?.Invoke());
    }
}
