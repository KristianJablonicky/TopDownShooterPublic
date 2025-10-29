using System.Collections;
using UnityEngine;

public class FadeOutThenGetDestroyed : MonoBehaviour
{
    public float duration = 1f, fadeOutTime = 0.5f;
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private bool shrinkSize = false;

    private void Awake()
    {
        if (sr != null)
        {
            Tweener.Tween(this, 1f, 0f, fadeOutTime, TweenStyle.linear,
                value => sr.SetAlpha(value), () => Destroy(gameObject), duration);
            //StartCoroutine(FadeOut());
        }
        if (shrinkSize)
        {
            Tweener.Tween(this, transform.localScale, Vector2.zero, fadeOutTime, TweenStyle.linear,
                value => transform.localScale = value, () => Destroy(gameObject), duration);
        }
    }
}
