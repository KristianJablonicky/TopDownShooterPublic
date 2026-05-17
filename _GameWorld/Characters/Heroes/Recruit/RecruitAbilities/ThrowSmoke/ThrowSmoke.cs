using UnityEngine;

[CreateAssetMenu(fileName = "ThrowSmoke", menuName = "Abilities/Utility/ThrowSmoke")]
public class ThrowSmoke : UtilityAbility
{
    [SerializeField] private SmokeGameObject smokePrefab;
    [SerializeField] private float castTime = 1.0f;
    [SerializeField] private float animationDurationMultiplier = 1.25f;
    protected override void OnKeyDown(Vector2 position)
    {
        ShowRangeIndicator(smokePrefab.Range);
    }

    protected override void OnKeyUp(Vector2 position)
    {
        var destination = GetDestination(position, smokePrefab.Range, true);
        if (!destination.HasValue) return;
        if (channelingManager.Channeling
        || !destination.HasValue)
        {
            HideRangeIndicator();
            return;
        }
        channelingManager.StartChanneling(castTime,
            () => TryInvokeRPC<RecruitAbilityRPCs>(rpcs => rpcs.RequestSmokeRPC(destination.Value))
        );
        AlsoPlayAnimation(durationMultiplier: animationDurationMultiplier);

        HideRangeIndicator();
        OnCast();
    }

    public override string _GetSpecificAttributes()
    {
        return $"Range: {smokePrefab.Range}\nDuration: {smokePrefab.Duration}s\nCast time: {castTime}s";
    }
}
