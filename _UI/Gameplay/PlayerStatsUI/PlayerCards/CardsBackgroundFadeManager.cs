using UnityEngine;

public class CardsBackgroundFadeManager : MonoBehaviour
{
    [SerializeField] private CanvasGroup soloCG, allPlayersCG;

    private void Start()
    {
        allPlayersCG.alpha = 0f;
        ToggleGO(allPlayersCG, false);
        ToggleGO(soloCG, true);
    }
    public void EnableCardsAndSwitchFade(float duration)
    {
        ToggleGO(allPlayersCG, true);
        Fade(soloCG, 0f, duration);
        Fade(allPlayersCG, 1f, duration);
    }

    private void Fade(CanvasGroup canvas, float targetAlpha, float fadeDuration)
    {
        Tweener.Tween(this, canvas.alpha, targetAlpha, fadeDuration, TweenStyle.quadratic,
            value => canvas.alpha = value,
            () => { if (targetAlpha == 0f) ToggleGO(canvas, false); }
        );
    }

    private void ToggleGO(CanvasGroup canvas, bool toggle)
    {
        canvas.gameObject.SetActive(toggle);
    }
}
