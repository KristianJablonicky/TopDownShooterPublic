using UnityEngine;

[CreateAssetMenu(fileName = "ShadowWave", menuName = "Abilities/Utility/ShadowWave")]
public class ShadowWave : UtilityAbility
{
    [SerializeField] private float castTime = 0.5f;
    [SerializeField] private ShadowWaveEffect shadowWave;
    protected override void OnKeyDown(Vector2 position)
    {
        ShowRangeIndicator(shadowWave.Range);
    }

    protected override void OnKeyUp(Vector2 position)
    {
        if (channelingManager.Channeling && !channelingManager.Interruptible)
        { 
            HideRangeIndicator();
            return;
        }
        OnCast();
        HideRangeIndicator();
        owner.Gun.ShootManager.Reset();
        channelingManager.RequestInterrupt();
        

        var destination = GetDestination(position, shadowWave.Range, true);
        channelingManager.StartChanneling(castTime,
            () => shadowWave.Cast(destination, (DraculaRPCs)characterRPCs)
        );
        FadeOutThenGetDestroyed preWaveGO = Instantiate(shadowWave.PreWaveIndicator, destination, Quaternion.identity);
        preWaveGO.duration = 0f;
        preWaveGO.fadeOutTime = castTime;
    }

    protected override string _GetAbilitySpecificStats()
    {
        return $"Range: {shadowWave.Range}\nArea of effect: {shadowWave.Area}\nMax velocity: {shadowWave.ForceMax}\nCast time: {castTime}";
    }
}