using UnityEngine;

[CreateAssetMenu(fileName = "ThrowSmoke", menuName = "Abilities/Utility/ThrowSmoke")]
public class ThrowSmoke : UtilityAbility
{
    [SerializeField] private SmokeGameObject smokePrefab;
    protected override void OnKeyDown(Vector2 position)
    {
        ShowRangeIndicator(smokePrefab.Range);
    }

    protected override void OnKeyUp(Vector2 position)
    {
        var destination = GetDestination(position, smokePrefab.Range, true);
        if (!destination.HasValue) return;
        
        HideRangeIndicator();
        OnCast();
        TryInvokeRPC<RecruitAbilityRPCs>(rpcs => rpcs.RequestSmokeRPC(destination.Value));
    }

    protected override string _GetAbilitySpecificStats()
    {
        return $"Range: {smokePrefab.Range}\nDuration: {smokePrefab.Duration}s";
    }
}
