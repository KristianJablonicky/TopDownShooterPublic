using UnityEngine;

[CreateAssetMenu(fileName = "SmokePostMortem", menuName = "Abilities/PostMortem/Smoke")]
public class SmokePostMortem : AbilityPostMortem
{
    [SerializeField] private SmokeGameObject smokePrefab;
    public override AbilityHotKeys KeyCode { get; protected set; } = AbilityHotKeys.Utility;

    protected override void OnKeyDownSecure(Vector2 position) { }

    protected override void OnKeyUpSecure(Vector2 position)
    {
        var destination = GetDestination(position, smokePrefab.Range, true, teamMate);
        if (!destination.HasValue) return;

        OnCast();
        TryInvokeRPC<RecruitAbilityRPCs>(rpcs => rpcs.RequestSmokeRPC(destination.Value));
    }

    public override string _GetSpecificAttributes() => "";
}
