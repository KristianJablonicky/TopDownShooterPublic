using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "BloodDrinker", menuName = "Abilities/Passive/BloodDrinker")]
public class BloodDrinker : Ability
{
    [SerializeField] private int healthThreshold = 30;
    [SerializeField] private int healOnKill = 50;
    [SerializeField] private float beforeKillDelay = 0.5f;
    private Dictionary<CharacterMediator, bool> mediatorDrained;

    protected override void SetUpRPCsReady()
    {
        if (!owner.IsLocalPlayer) return;

        mediatorDrained = new();
        var manager = CharacterManager.Instance;
        foreach (var player in manager.Players.Values)
        {
            SubscribeCharacter(player);
        }
        manager.CharacterRegistered += SubscribeCharacter;
    }

    private void SubscribeCharacter(CharacterMediator character)
    {
        if (character == owner) return;
        mediatorDrained.Add(character, false);
        character.HealthComponent.M1TookDamageFromM2 += OnDamageTaken;
    }

    private void OnDamageTaken(int damage, CharacterMediator hitMediator, CharacterMediator hittingMediator)
    {
        // someone else hit the player
        if (hittingMediator != owner) return;

        // this player has already been drained
        if (mediatorDrained[hitMediator]) return;
        var hc = hitMediator.HealthComponent;
        if (hc.CurrentHealth <= healthThreshold && hc.CurrentHealth > 0)
        {
            mediatorDrained[hitMediator] = true;
            ExecuteAfterDelay(hitMediator);
        }
    }

    private async void ExecuteAfterDelay(CharacterMediator hitMediator)
    {
        await Task.Delay((int)(beforeKillDelay * 1000f));
        hitMediator.HealthComponent.TakeLethalDamage();
        owner.NetworkInput.RequestHealRpc(healOnKill);
    }

    protected override void SetUp() { }
    protected override void AbstractReset()
    {
        if (mediatorDrained is null) return;
        foreach (var key in mediatorDrained.Keys.ToList())
        {
            mediatorDrained[key] = false;
        }
    }
}
