using System;
using System.Collections;
using UnityEngine;

public class Tweener : MonoBehaviour
{
    private static Tweener instance;
    private void Awake() => instance = this;
    public static Coroutine Tween(
        object invoker,
        float start,
        float end,
        float duration,
        TweenStyle style,
        Action<float> onUpdate,
        Action onExit = null,
        float initialDelay = 0f)
    {
        return instance.StartCoroutine(TweenCoroutine(invoker, start, end, duration, style, onUpdate, onExit, initialDelay));
    }
    public static IEnumerator TweenCoroutine(
        object invoker,
        float start,
        float end,
        float duration,
        TweenStyle style,
        Action<float> onUpdate,
        Action onExit = null,
        float initialDelay = 0f)
    {
        float timeElapsed = 0f;
        var function = GetFunc(style);

        if (initialDelay > 0f)
        {
            yield return new WaitForSeconds(initialDelay);
        }

        while (timeElapsed < duration)
        {
            if (invoker == null || invoker.Equals(null)) yield break;

            timeElapsed += Time.deltaTime;
            var progress = Mathf.Clamp01(timeElapsed / duration);
            var value = function(start, end, progress);

            onUpdate?.Invoke(value);

            yield return null;
        }

        onUpdate?.Invoke(function(start, end, 1f));
        onExit?.Invoke();
    }

    public static void Tween(
        object invoker,
        Vector2 start,
        Vector2 end,
        float duration,
        TweenStyle style,
        Action<Vector2> onUpdate,
        Action onExit = null,
        float initialDelay = 0f)
    {
        instance.StartCoroutine(TweenCoroutine(invoker, start, end, duration, style, onUpdate, onExit, initialDelay));
    }
    private static IEnumerator TweenCoroutine(
        object invoker,
        Vector2 start,
        Vector2 end,
        float duration,
        TweenStyle style,
        Action<Vector2> onUpdate,
        Action onExit = null,
        float initialDelay = 0f)
    {
        float timeElapsed = 0f;
        var function = GetFunc(style);

        if (initialDelay > 0f)
        {
            yield return new WaitForSeconds(initialDelay);
        }

        while (timeElapsed < duration)
        {
            if (invoker == null || invoker.Equals(null)) yield break;

            timeElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(timeElapsed / duration);
            Vector2 value = new(
                function(start.x, end.x, t),
                function(start.y, end.y, t)
            );

            onUpdate?.Invoke(value);
            yield return null;
        }
        Vector2 finalValue = new(
            function(start.x, end.x, 1f),
            function(start.y, end.y, 1f)
        );
        onUpdate?.Invoke(finalValue);
        onExit?.Invoke();
    }

    public static Coroutine TweenAtRate(
        object invoker,
        float current,
        float start,
        float end,
        float timeToFullFill,
        TweenStyle style,
        Action<float> onUpdate,
        Action onExit = null,
        float initialDelay = 0f)
    {
        return instance.StartCoroutine(TweenCoroutine(invoker, current, end,
            timeToFullFill * Mathf.InverseLerp(end, start, current),
            style, onUpdate, onExit, initialDelay));
    }

    public static Coroutine TweenCanvasGroupAtRate(
        CanvasGroup canvasGroup,
        float timeToFullFill,
        TweenStyle style,
        bool increasing,
        float? targetAlpha = null,
        Action onExit = null)
    {
        float target;
        if (targetAlpha.HasValue)
        {
            target = targetAlpha.Value;
        }
        else
        {
            target = increasing ? 1f : 0f;
        }

        return TweenAtRate(
            canvasGroup,
            canvasGroup.alpha,
            increasing ? 0f : 1f,
            target,
            timeToFullFill,
            style,
            value => canvasGroup.alpha = value,
            onExit
        );
    }

    public static void RequestEndCoroutine(Coroutine coroutine)
    {
        if (coroutine == null) return;
        instance.StopCoroutine(coroutine);
    }

    public static float GetValue(float start, float end, float t, TweenStyle style)
    {
        var func = GetFunc(style);
        return func(start, end, t);
    }

    public static float Linear(float start, float end, float t)
        => Mathf.Lerp(start, end, t);

    public static float Quadratic(float start, float end, float t)
        => Mathf.Lerp(start, end, t * t);

    public static float QuadraticEaseOut(float start, float end, float t)
        => Mathf.Lerp(start, end, 1 - (1 - t) * (1 - t));
    public static float SinusPingPong(float start, float end, float t)
        => Mathf.Lerp(start, end, Mathf.Sin(Mathf.PI * t));

    private static Func<float, float, float, float> GetFunc(TweenStyle style)
    {
        return style switch
        {
            TweenStyle.linear => Linear,
            TweenStyle.quadratic => Quadratic,
            TweenStyle.quadraticEaseOut => QuadraticEaseOut,
            TweenStyle.sinusPingPong => SinusPingPong,
            _ => Linear
        };
    }

    public static void TweenPosition(GameObject invoker,
        Vector2 start,
        Vector2 end,
        float duration,
        TweenStyle style,
        Action onExit = null,
        float initialDelay = 0f)
    {
        Tween(invoker, start, end, duration, style,
            value => invoker.transform.position = new Vector2(value.x, value.y),
            onExit,
            initialDelay);
    }
}

public enum TweenStyle
{
    linear,
    quadratic,
    quadraticEaseOut,
    sinusPingPong
}