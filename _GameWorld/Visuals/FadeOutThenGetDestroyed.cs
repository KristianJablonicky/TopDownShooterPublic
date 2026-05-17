using UnityEngine;

public class FadeOutThenGetDestroyed : MonoBehaviour
{
    public float duration = 1f, fadeOutTime = 0.5f;
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private bool shrinkSize = false;
    [SerializeField] private bool playOnAwake = true;
    [SerializeField] private bool destroyOnEnd = true;
    [SerializeField] private TweenStyle tweenStyle = TweenStyle.linear;
    
    private bool alreadyPlayed = false;

    private void OnEnable()
    {
        if (alreadyPlayed)
        {
            var color = sr.color;
            color.a = 1f;
            sr.color = color;
        }
        
        if (playOnAwake) PlayAnimation(null);
    }


    public void PlayAnimation(float? durationArg)
    {
        alreadyPlayed = true;
        if (durationArg.HasValue)
        {
            duration = durationArg.Value;
        }

        if (sr != null)
        {
            Tweener.Tween(this, 1f, 0f, fadeOutTime, tweenStyle,
                value => sr.SetAlpha(value), OnAnimationEnd, duration);
            //StartCoroutine(FadeOut());
        }
        if (shrinkSize)
        {
            Tweener.Tween(this, transform.localScale, Vector2.zero, fadeOutTime, tweenStyle,
                value => transform.localScale = value, OnAnimationEnd, duration);
        }
    }

    private void OnAnimationEnd()
    {
        if (destroyOnEnd)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
