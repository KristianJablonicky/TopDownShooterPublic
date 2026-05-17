using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public class AnimationBase : MonoBehaviour
{
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] protected Image image;
    [SerializeField] protected Sprite[] frames;
    [SerializeField] protected float duration;
    [SerializeField] protected bool destroyOnEnd = false;

    public Action AnimationEnded;
    public void PlayAnimation(float? durationOverwrite = null, bool reverseOrder = false)
    {
        if (!gameObject.activeInHierarchy)
        {
            Destroy(gameObject);
            return;
        }
        var animationDuration = durationOverwrite ?? duration;
        StopAllCoroutines();
        StartCoroutine(PlayAnimation(animationDuration, reverseOrder));
    }

    private IEnumerator PlayAnimation(float animationDuration, bool reverseOrder)
    {
        var frameLength = animationDuration / frames.Length;
        var usedFrames = reverseOrder ? frames.Reverse() : frames;

        foreach (var frame in usedFrames)
        {
            SetSprite(frame);
            yield return new WaitForSeconds(frameLength);
        }
        AnimationEnded?.Invoke();
        if (destroyOnEnd)
        {
            Destroy(gameObject);
        }
    }

    public void SetSprite(Sprite sprite)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = sprite;
        }
        if (image != null)
        {
            image.sprite = sprite;
        }
    }
}
