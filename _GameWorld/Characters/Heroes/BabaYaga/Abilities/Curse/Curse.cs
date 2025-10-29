using UnityEngine;

[CreateAssetMenu(fileName = "Curse", menuName = "Abilities/Utility/Curse")]
public class Curse : UtilityAbility
{
    [field: SerializeField] public float Duration { get; private set; } = 3f;
    [field: SerializeField] public float Range { get; private set; } = 10f;
    [field: SerializeField] public float NearSightedMultiplier { get; private set; } = 0.5f;
    [field: SerializeField] public float ArcDegrees { get; private set; } = 45f;
    [field: SerializeField] public PopIn SweepGO { get; private set; }

    [SerializeField] private float castAnimationTime = 0.25f;

    protected override void OnKeyDown(Vector2 position)
    {
        ShowRangeIndicator(Range);
    }

    protected override void OnKeyUp(Vector2 position)
    {
        if (channelingManager.Channeling) return;
        channelingManager.StartChanneling(castAnimationTime, CastEffect);
    }

    private void CastEffect()
    {
        HideRangeIndicator();
        TryInvokeRPC<BabaYagaRPCs>(rpcs =>
        {
            rpcs.RequestSweepCurseRPC(owner.PlayerId);
            OnCast();
        });
    }

    protected override string _GetAbilitySpecificStats()
    {
        return $"Duration: {Duration}\nRange: {Range}\nNearsighted vision: {Mathf.RoundToInt(100f * NearSightedMultiplier)}%\nCast delay: {castAnimationTime}";
    }
}
