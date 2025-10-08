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
        Func<float, float, float, float> function = style switch
        {
            TweenStyle.linear => Linear,
            TweenStyle.quadratic => Quadratic,
            _ => Linear
        };

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
        Func<float, float, float, float> function = style switch
        {
            TweenStyle.linear => Linear,
            TweenStyle.quadratic => Quadratic,
            _ => Linear
        };

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
}

public enum TweenStyle
{
    linear,
    quadratic
}