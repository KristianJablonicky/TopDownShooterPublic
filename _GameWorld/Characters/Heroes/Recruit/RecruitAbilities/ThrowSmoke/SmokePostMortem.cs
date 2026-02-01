using UnityEngine;

[CreateAssetMenu(fileName = "SmokePostMortem", menuName = "Abilities/PostMortem/Smoke")]
public class SmokePostMortem : AbilityPostMortem
{
    [SerializeField] private SmokeGameObject smokePrefab;
    public override AbilityHotKeys KeyCode { get; protected set; } = AbilityHotKeys.Utility;

    protected override void OnKeyDownSecure(Vector2 position) { }

    protected override void OnKeyUpSecure(Vector2 position)
    {
        var destination = GetDestination(position, smokePrefab.Range, true);
        if (!destination.HasValue) return;

        OnCast();
        TryInvokeRPC<RecruitAbilityRPCs>(rpcs => rpcs.RequestSmokeRPC(destination.Value));
    }

    protected override string _GetAbilitySpecificStats() => "";
}
