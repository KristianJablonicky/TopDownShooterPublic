using UnityEngine;

[CreateAssetMenu(fileName = "ShadowWave", menuName = "Abilities/Utility/ShadowWave")]
public class ShadowWave : UtilityAbility
{
    [SerializeField] private float castTime = 0.5f, animationDurationMultiplier = 1.5f;
    [SerializeField] private ShadowWaveEffect shadowWave;
    protected override void OnKeyDown(Vector2 position)
    {
        ShowRangeIndicator(shadowWave.Range);
    }

    protected override void OnKeyUp(Vector2 position)
    {
        var destination = GetDestination(position, shadowWave.Range, true);
        if ((channelingManager.Channeling && !channelingManager.Interruptible)
        ||   !destination.HasValue)
        { 
            HideRangeIndicator();
            return;
        }
        OnCast();
        HideRangeIndicator();
        owner.Gun.ShootManager.Reset();
        channelingManager.RequestInterrupt();
        

        channelingManager.StartChanneling(castTime,
            () => shadowWave.Cast(destination.Value, (DraculaRPCs)characterRPCs)
        );
        channelingManager.AlsoPlayAnAnimation(Animations.AbilityUtility, animationDurationMultiplier);

        FadeOutThenGetDestroyed preWaveGO = Instantiate(shadowWave.PreWaveIndicator, destination.Value, Quaternion.identity);
        preWaveGO.duration = 0f;
        preWaveGO.fadeOutTime = castTime;
    }

    protected override string _GetAbilitySpecificStats()
    {
        return $"Range: {shadowWave.Range}\nArea of effect: {shadowWave.Area}\nMax velocity: {shadowWave.ForceMax}\nCast time: {castTime}";
    }
}