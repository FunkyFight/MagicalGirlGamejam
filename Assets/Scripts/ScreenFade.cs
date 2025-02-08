using UnityEngine;
using DG.Tweening;

public class ScreenFade : MonoBehaviour
{
    private float alpha = 1f;             // Opacité actuelle (0 = transparent, 1 = noir complet)
    private Texture2D blackTexture;       // Texture noire utilisée pour couvrir l'écran

    void Awake()
    {
        // Création d'une texture noire 1x1 pixel
        blackTexture = new Texture2D(1, 1);
        blackTexture.SetPixel(0, 0, Color.black);
        blackTexture.Apply();
    }

    void OnGUI()
    {
        // Définit la couleur avec l'opacité actuelle
        GUI.color = new Color(0, 0, 0, alpha);
        // Dessine la texture noire sur tout l'écran
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), blackTexture);
    }

    // Appelle cette méthode pour lancer un fondu au noir sur la durée spécifiée
    public void FadeToBlack(float duration, System.Action onComplete = null)
    {
        DOTween.To(() => alpha, x => alpha = x, 1f, duration)
               .SetEase(Ease.InOutQuad)
               .OnComplete(() => onComplete?.Invoke());
    }

    // Optionnel : pour faire un fondu inverse (du noir vers la transparence)
    public void FadeFromBlack(float duration, System.Action onComplete = null)
    {
        DOTween.To(() => alpha, x => alpha = x, 0f, duration)
               .SetEase(Ease.InOutQuad)
               .OnComplete(() => onComplete?.Invoke());
    }
}
