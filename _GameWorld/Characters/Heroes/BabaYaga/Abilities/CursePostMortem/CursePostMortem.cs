using UnityEngine;

[CreateAssetMenu(fileName = "CursePostMortem", menuName = "Abilities/PostMortem/Curse")]
public class CursePostMortem : AbilityPostMortem
{
    public override AbilityHotKeys KeyCode { get; protected set; } = AbilityHotKeys.Utility;

    protected override void OnKeyDownSecure(Vector2 position) { }

    protected override void OnKeyUpSecure(Vector2 position)
    {
        TryInvokeRPC<BabaYagaRPCs>(rpcs =>
        {
            rpcs.RequestCurseRPC(owner.PlayerId);
            OnCast();
        });
    }

    protected override string _GetAbilitySpecificStats()
    {
        return $"";
    }
}
