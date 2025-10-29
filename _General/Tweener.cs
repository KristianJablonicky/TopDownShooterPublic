using System;
using System.Collections;
using UnityEngine;

public class Tweener : MonoBehaviour
{
    private static Tweener instance;
    private void Awake() => instance = this;
    public static void Tween(
        object invoker,
        float start,
        float end,
        float duration,
        TweenStyle style,
        Action<float> onUpdate,
        Action onExit = null,
        float initialDelay = 0f)
    {
        instance.StartCoroutine(TweenCoroutine(invoker, start, end, duration, style, onUpdate, onExit, initialDelay));
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
            float progress = Mathf.Clamp01(timeElapsed / duration);
            float value = function(start, end, progress);

            onUpdate?.Invoke(value);

            yield return null;
        }

        onUpdate?.Invoke(end);
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

        onUpdate?.Invoke(end);
        onExit?.Invoke();
    }

    private static float Linear(float start, float end, float t)
        => Mathf.Lerp(start, end, t);

    private static float Quadratic(float start, float end, float t)
        => Mathf.Lerp(start, end, t * t);

    private static float QuadraticEaseOut(float start, float end, float t)
        => Mathf.Lerp(start, end, 1 - (1 - t) * (1 - t));

    private static Func<float, float, float, float> GetFunc(TweenStyle style)
    {
        return style switch
        {
            TweenStyle.linear => Linear,
            TweenStyle.quadratic => Quadratic,
            TweenStyle.quadraticEaseOut => QuadraticEaseOut,
            _ => Linear
        };
    }
}

public enum TweenStyle
{
    linear,
    quadratic,
    quadraticEaseOut
}