using UnityEngine;

[CreateAssetMenu(fileName = "ReleaseDjinn", menuName = "Abilities/Utility/ReleaseDjinn")]
public class ReleaseDjinn : UtilityAbility
{
    [field: SerializeField] public float VisionRadiusStart { get; private set; } = 1.5f;
    [field: SerializeField] public float VisionRadiusEnd { get; private set; } = 3f;
    [field: SerializeField] public float Duration { get; private set; } = 3f;
    [field: SerializeField] public float FlyBackDuration { get; private set; } = 0.5f;

    protected override void OnKeyDown(Vector2 position) { }

    protected override void OnKeyUp(Vector2 position)
    {
        if (channelingManager.Channeling) return;

        channelingManager.StartChannelingStandingStill(Duration, null, owner, false);

        TryInvokeRPC<DjinnRPCs>(rpcs =>
        {
            rpcs.RequestReleaseDjinnRPC(owner.PlayerId, false);
        });
        OnCast();
    }

    protected override string _GetAbilitySpecificStats()
    {
        return $"Duration: {Duration}s\nVision radius: {VisionRadiusStart} - {VisionRadiusEnd}";
    }
}
