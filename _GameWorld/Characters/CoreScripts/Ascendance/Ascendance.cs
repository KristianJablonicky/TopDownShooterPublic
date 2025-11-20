using System;

public class Ascendance : IResettable
{
    public event Action TeamMateAscended;
    public event Action<CharacterMediator> SpiritLeft, OnAscendance;

    public bool HasAscended { get; private set; } = false;
    
    public const float timeToAscend = 0.75f;
    private const int healthOnAscension = 30;
    private readonly CharacterMediator owner;

    public Ascendance(CharacterMediator mediator)
    {
        owner = mediator;

        if (owner.IsNPC) return;
        mediator.Died += OnDeath;
    }

    public void Ascend()
    {
        if (owner.IsLocalPlayer)
        {
            owner.NetworkInput.RequestHealRpc(healthOnAscension, true);
        }
        HasAscended = true;
        owner.SpriteRenderer.MultiplyColor(0.1f, 1f, 1f);

        OnAscendance?.Invoke(owner);
    }

    private async void OnDeath(CharacterMediator owner)
    {
        var player = owner.playerData;
        if (player is not null)
        {
            await TaskExtensions.Delay(timeToAscend);
            var teamMate = player.GetTeamMate();
            teamMate.Mediator.Ascendance.Ascend();
            TeamMateAscended?.Invoke();
            SpiritLeft?.Invoke(owner);
        }
    }

    public void Reset()
    {
        HasAscended = false;
    }
}
