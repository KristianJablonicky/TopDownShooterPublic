using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GlobalVolumeAdjustments : MonoBehaviour
{
    [SerializeField] private Volume globalVolume;
    [SerializeField] private float saturationOnDeath = -50f, saturationOnRoundStart = -25f,
        fadeDuration = 0.5f;
    private ColorAdjustments colorAdjustments;

    void Start()
    {
        globalVolume.profile.TryGet(out colorAdjustments);
        PlayerNetworkInput.PlayerSpawned += OnPlayerSpawn;

        RoundStartWait.Instance.OnRoundStartWait += duration =>
        {
            colorAdjustments.saturation.value = saturationOnRoundStart;
            FadeToSaturation(0f, duration);
        };

    }

    private void OnPlayerSpawn(CharacterMediator player)
    {
        player.Died += (_) => FadeToSaturation(saturationOnDeath, fadeDuration);
        player.Respawned += (_) => FadeToSaturation(0f, fadeDuration);
    }

    private void FadeToSaturation(float saturation, float duration)
    {
        var start = colorAdjustments.saturation.value;
        Tweener.Tween(this, start, saturation, duration, TweenStyle.linear,
            value => colorAdjustments.saturation.value = value);
    }
}
