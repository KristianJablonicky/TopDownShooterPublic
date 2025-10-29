using UnityEngine;

public class PopIn : MonoBehaviour
{
    [SerializeField] private float popInDuration = 1f;
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private bool popInSize = true;
    [SerializeField] private TweenStyle tweenStyle = TweenStyle.quadratic;
    private void Start()
    {
        if (popInSize)
        {
            Tweener.Tween(this, Vector2.zero, transform.localScale, popInDuration, tweenStyle,
                value => transform.localScale = Vector2.one * value);
        }

        if (sr != null)
        {
            Tweener.Tween(this, 0f, 1f, popInDuration, tweenStyle,
                value => sr.SetAlpha(value));
        }
    }
}
