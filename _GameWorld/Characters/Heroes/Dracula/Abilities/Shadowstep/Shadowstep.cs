using UnityEngine;

[CreateAssetMenu(fileName = "Shadowstep", menuName = "Abilities/Movement/Shadowstep")]
public class Shadowstep : MovementAbility
{
    [SerializeField] private float range = 10f;
    [SerializeField] private float channelStart = 0.5f,
        channelStandingStill = 0.25f,
        channelEnd = 0.5f;
    protected override void OnKeyDown(Vector2 position)
    {
        ShowRangeIndicator(range);
    }

    protected override void OnKeyUp(Vector2 position)
    {
        if (channelingManager.Channeling) return;

        HideRangeIndicator();

        var destination = GetDestination(position, range, true);

        channelingManager.StartChannelingStandingStill(channelStart, () => Teleport(destination), owner, false);
        PlaySound(0);
        OnCast();
    }

    private void Teleport(Vector2 destination)
    {
        owner.MovementController.SetPosition(destination);
        PlaySound(1);
        channelingManager.StartChannelingStandingStill(channelStandingStill, Materialize, owner, false);
    }
    private void Materialize()
    {
        channelingManager.StartChanneling(channelEnd, null);
    }

    protected override string _GetAbilitySpecificStats()
    {
        return $"Range: {range}\nTotal channel time: {channelStart + channelStandingStill + channelEnd}";
    }
}
