using UnityEngine;
using UnityEngine.UI;

public class HitMarker : MonoBehaviour, IResettable
{
    [Header("References")]
    [SerializeField] private CanvasGroup indicatorCanvasGroup;
    [SerializeField] private Image indicatorImage;
    [SerializeField] private RectTransform indicatorRectTransform;

    [Header("Settings")]
    [SerializeField] private int damageToFullIntensityThreshold = 80;
    [SerializeField] private float indicatorFadeDuration = 0.5f,
        initialPopDuration = 0.25f;
    [SerializeField, Range(0f, 1f)] private float minIntensity = 0.25f;
    [SerializeField, Range(0f, 1f)] private float headShotRedGBValues = 0.25f;

    private int damageAccumulated = 0;
    private Vector2 maxDamageIndicatorSize;
    private float timeRemaining, intensity;

    private void Start()
    {
        maxDamageIndicatorSize = indicatorRectTransform.localScale;
        Reset();
    }

    public void DamageDealt(int damage, bool headShot)
    {
        if (headShot) OnHeadShot();
        damageAccumulated += damage;
        SetIntensity(damageAccumulated);
    }

    private Coroutine headshotColorCoroutine;
    private void OnHeadShot()
    {
        if (headshotColorCoroutine != null) StopCoroutine(headshotColorCoroutine);

        indicatorImage.color = new(1f, headShotRedGBValues, headShotRedGBValues);
        headshotColorCoroutine = StartCoroutine(
            Tweener.TweenCoroutine(this, headShotRedGBValues, 1f, indicatorFadeDuration, TweenStyle.quadratic,
            colorValue => indicatorImage.color = new(1f, colorValue, colorValue),
            initialDelay: initialPopDuration)
        );
    }

    private async void SetIntensity(int damage)
    {
        intensity = Mathf.Clamp01((float)damage / damageToFullIntensityThreshold);
        intensity = Mathf.Max(intensity, minIntensity);
        indicatorCanvasGroup.alpha = 1f;
        SetSizeBasedOnIntensity(intensity);
        timeRemaining = indicatorFadeDuration;

        enabled = false;
        await TaskExtensions.Delay(initialPopDuration);
        enabled = true;
    }

    private void Update()
    {
        timeRemaining -= Time.deltaTime;
        if (timeRemaining <= 0f)
        {
            Reset();
            return;
        }
        indicatorCanvasGroup.alpha = Mathf.Pow(timeRemaining / indicatorFadeDuration, 2f);
        SetSizeBasedOnIntensity(intensity * timeRemaining / indicatorFadeDuration);
    }

    private void SetSizeBasedOnIntensity(float intensity)
    {
        indicatorRectTransform.localScale = new(
            maxDamageIndicatorSize.x,
            maxDamageIndicatorSize.y * intensity
        );
    }

    public void Reset()
    {
        damageAccumulated = 0;
        indicatorCanvasGroup.alpha = 0f;
        indicatorRectTransform.localScale = Vector2.zero;
        timeRemaining = 0f;
        indicatorImage.color = Color.white;
        enabled = false;
    }
}
