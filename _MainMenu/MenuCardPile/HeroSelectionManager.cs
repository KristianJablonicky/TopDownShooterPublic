using System;
using System.Collections;
using UnityEngine;

public class HeroSelectionManager : SingletonMonoBehaviour<HeroSelectionManager>
{
    [SerializeField] private float animationDuration = 1f;
    public event Action HeroCardPickedUp, HeroCardPutDown, HeroCardBeingPutDown, AnimationFinished;
    public event Action<float> AnimationProgressed;

    public bool IsAnimationFinished { get; private set; } = true;

    public void HighlightHeroCard()
    {
        HeroCardPickedUp?.Invoke();
        StartCoroutine(Animate(false));
    }
    public void UnHighlightHeroCard()
    {
        //HeroCardPutDown?.Invoke();
        StartCoroutine(Animate(true));
    }

    private IEnumerator Animate(bool reverse)
    {
        var timeElapsed = 0f;
        IsAnimationFinished = false;
        if (reverse) HeroCardBeingPutDown?.Invoke();
        while (timeElapsed < animationDuration)
        {
            timeElapsed += Time.deltaTime;
            var progress = Mathf.Clamp01(timeElapsed / animationDuration);
            if (reverse)
            {
                AnimationProgressed?.Invoke(1f - progress);
            }
            else
            {
                AnimationProgressed?.Invoke(progress);
            }
            yield return null;
        }
        IsAnimationFinished = true;
        AnimationFinished?.Invoke();
        if (reverse)
        {
            HeroCardPutDown?.Invoke();
        }
    }
}
