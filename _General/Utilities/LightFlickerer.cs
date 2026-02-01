using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightFlickerer : SinusUpdater
{
    [Header("References")]
    [SerializeField] private Light2D lightToFlicker;
    [Header("Flicker Settings")]
    [SerializeField] private float minIntensityMultiplier = 0.8f;
    [SerializeField] private float maxIntensityMultiplier = 1.2f;
    [SerializeField] private float flickerDuration = 1f;
    [SerializeField] private bool flickerRadius = false;
    private float currentLerpCache;
    private float baseIntensity, baseOuterRadius, baseInnerRadius;

    private void Start()
    {
        baseIntensity = lightToFlicker.intensity;
        baseOuterRadius = lightToFlicker.pointLightOuterRadius;
        baseInnerRadius = lightToFlicker.pointLightInnerRadius;
    }
    protected override void UpdateSinus(float sinusValue, float sinus01)
    {
        currentLerpCache = Mathf.Lerp(
            minIntensityMultiplier,
            maxIntensityMultiplier,
            sinus01
        );

        lightToFlicker.intensity = baseIntensity * currentLerpCache;
        if (flickerRadius)
        {
            lightToFlicker.pointLightOuterRadius = baseOuterRadius * currentLerpCache;
            lightToFlicker.pointLightInnerRadius = baseInnerRadius * currentLerpCache;
        }
    }
}
