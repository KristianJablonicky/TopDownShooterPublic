using System;
using UnityEngine;

public class CanvasFaderBase : MonoBehaviour
{
    [SerializeField] protected float animationDuration = 0.25f;
    [SerializeField] protected CanvasGroup canvasGroup;
    protected Coroutine coroutine;

    protected void TweenState(bool increasing, Action onExit = null)
    {
        gameObject.SetActive(true);
        Tweener.RequestEndCoroutine(coroutine);
        coroutine = Tweener.TweenCanvasGroupAtRate(
            canvasGroup,
            animationDuration,
            TweenStyle.quadraticEaseOut,
            increasing,
            onExit: onExit
        );
    }
}
