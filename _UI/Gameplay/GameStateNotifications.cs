using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Colors;

public class GameStateNotifications : SingletonMonoBehaviour<GameStateNotifications>
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text content;
    [SerializeField] private Image background;
    [SerializeField] private float fadeInTime = 0.2f, duration = 1.5f, fadeOutTime = 0.5f;

    private void Start()
    {
        gameObject.SetActive(false);
    }
    public void ShowMessage(string text, Colors color = Black)
    {
        StopAllCoroutines();
        gameObject.SetActive(true);
        content.text = text;
        background.color = CommonColors.Instance.GetColor(color);
        FadeIn();
    }

    private void FadeIn()
    {
        StartCoroutine(
            Tweener.TweenCoroutine(this, 0f, 1f, fadeInTime, TweenStyle.quadratic,
            value => canvasGroup.alpha = value,
            () => StartCoroutine(Wait()))
        );
    }

    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(duration);
        FadeOut();
    }

    private void FadeOut()
    {
        Tweener.Tween(this, 1f, 0f, fadeOutTime, TweenStyle.quadratic,
            value => canvasGroup.alpha = value, () => gameObject.SetActive(false));
    }

}
