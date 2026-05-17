using UnityEngine;

[CreateAssetMenu(fileName = "ShadowWavePostMortem", menuName = "Abilities/PostMortem/ShadowWave")]
public class ShadowWavePostMortem : AbilityPostMortem
{
    [SerializeField] private ShadowWaveEffect shadowWave;

    public override AbilityHotKeys KeyCode { get; protected set; } = AbilityHotKeys.Utility;

    protected override void OnKeyDownSecure(Vector2 position) { }

    protected override void OnKeyUpSecure(Vector2 position)
    {
        var destination = GetDestination(position, shadowWave.Range, true, teamMate);
        if (!destination.HasValue) return;
        shadowWave.Cast(owner, destination.Value, (DraculaRPCs)characterRPCs);
        OnCast();
    }

    public override string _GetSpecificAttributes() => "";
}
