using UnityEngine;

[CreateAssetMenu(fileName = "ThrowSmoke", menuName = "Abilities/Utility/ThrowSmoke")]
public class ThrowSmoke : UtilityAbility
{
    [field: SerializeField] public float Duration { get; private set; } = 5f;
    [field: SerializeField] public float Radius { get; private set; } = 2.5f;
    [SerializeField] private float range = 5f;
    protected override void OnKeyDown(Vector2 position)
    {
        ShowRangeIndicator(range);
    }

    protected override void OnKeyUp(Vector2 position)
    {
        HideRangeIndicator();
        OnCast();
        var destination = GetDestination(position, range, true);
        TryInvokeRPC<RecruitAbilityRPCs>(rpcs => rpcs.RequestSmokeRPC(destination));
    }

    protected override string _GetAbilitySpecificStats()
    {
        return $"Range: {range}\nDuration: {Duration}s";
    }
}
