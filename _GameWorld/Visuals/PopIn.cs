using UnityEngine;

public class PopIn : MonoBehaviour
{
    [SerializeField] private bool popInOnStart = true;
    [SerializeField] private float popInDuration = 1f;
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private bool popInSize = true;
    [SerializeField] private TweenStyle tweenStyle = TweenStyle.quadratic;
    [SerializeField] private float targetScale = 0;
    [SerializeField] private float initialScale = 0f;
    
    private void Start()
    {
        if (popInOnStart)
        {
            PlayAnimation();
        }
    }

    public void PlayAnimation()
    {
        if (popInSize)
        {
            targetScale = targetScale == 0 ? transform.localScale.x : targetScale;
            Tweener.Tween(this, 0, 1, popInDuration, tweenStyle,
                value => {
                    value = Mathf.Lerp(initialScale, targetScale, value);
                    transform.localScale = Vector2.one * value;
                });
        }

        if (sr != null)
        {
            Tweener.Tween(this, 0f, 1f, popInDuration, tweenStyle,
                value => sr.SetAlpha(value));
        }
    }

    public float GetDuration() => popInDuration;
}
