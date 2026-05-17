using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "Shadowstep", menuName = "Abilities/Movement/Shadowstep")]
public class Shadowstep : MovementAbility
{
    [field: SerializeField] public float SpookRadius { get; private set; } = 4f;
    [SerializeField] private float range = 10f;
    [SerializeField] private float channelStart = 0.5f,
        channelStandingStill = 0.25f,
        channelEnd = 0.5f;
    [SerializeField] private MoveToPosition batPrefab;
    protected override void OnKeyDown(Vector2 position)
    {
        ShowRangeIndicator(range);
    }

    protected override void OnKeyUp(Vector2 position)
    {
        var destination = GetDestination(position, range, true);
        if ((channelingManager.Channeling && !channelingManager.Interruptible)
        ||   !destination.HasValue)
        {
            HideRangeIndicator();
            return;
        }
        HideRangeIndicator();
        owner.Gun.ShootManager.Reset();
        channelingManager.RequestInterrupt();


        channelingManager.StartChannelingStandingStill(channelStart, () => Teleport(destination.Value), owner, false);
        AlsoPlayAnimation();
        OnCast();

        SummonBat(owner.GetPosition(), destination.Value);
    }

    private void Teleport(Vector2 destination)
    {
        var dRpcs = (DraculaRPCs)characterRPCs;
        owner.MovementController.SetPosition(destination);
        channelingManager.StartChannelingStandingStill(channelStandingStill, Materialize, owner, false);
        AlsoPlayAnimation(specialIndex: 0, durationBonus: channelEnd);
        // extra animation (reversed order), bonus duration of channelEnd to cover both channels with one animation
        
        dRpcs.MaterializeRPC(destination);

        // spook nearby enemies
        var teamMate = GameStateManager.Instance.GameInProgress ?
            owner.GetTeamMate() : null;

        foreach (var character in CharacterManager.Instance.Mediators.Values)
        {
            if (character == owner
            ||  !character.IsAlive) continue;
            if (owner.GetDistance(character) <= SpookRadius)
            {
                if (teamMate == null       // match not started yet
                ||  teamMate != character) // not teammate
                {
                    dRpcs.SpookHeroRpc(character.PlayerId);
                }
            }
        }
    }
    private void Materialize()
    {
        channelingManager.StartChanneling(channelEnd, null);
    }

    private void SummonBat(Vector2 position, Vector2 destination)
    {
        var bat = Instantiate(batPrefab, owner.GetPosition(), Quaternion.identity);
        var batDestination = FloorUtilities.TranslateToTheOtherFloorIfNeeded(
            destination - position,
            owner.GetPosition());
        bat.SetTargetPosition(batDestination, false);
    }

    public override string _GetSpecificAttributes()
    {
        return $"Range: {range}\nTotal channel time: {channelStart + channelStandingStill + channelEnd}\nSpook radius: {SpookRadius}m";
    }
}
