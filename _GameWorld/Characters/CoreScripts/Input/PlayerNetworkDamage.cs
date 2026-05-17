using System;
using Unity.Netcode;
using UnityEngine;

public partial class PlayerNetworkInput : NetworkBehaviour
{
    public Action<int, DamageTag> OnDamageDealt;
    public Action<int, DamageTag, CharacterMediator> OnDamageDealtToMediator;

    public void DealDamage(int damage, DamageTag tag, CharacterMediator targetMediator, CharacterMediator sourceMediator, bool dealDamagePostMortem)
    {
        if (!mediator.IsAlive && !dealDamagePostMortem)
        {
            Debug.Log($"Would have dealt {damage} to {targetMediator.PlayerId}, but I am dead :/");
            return;
        }

        if (!targetMediator.IsTrainingDummy) DealDamageRequestRpc(damage, tag, targetMediator.PlayerId, sourceMediator.PlayerId);
        else
        {
            targetMediator.HealthComponent.TakeDamage(damage, tag, mediator);
            NotifySubscribers(damage, tag, targetMediator);
        }
    }
    public void TakeLethalDamage()
    {
        if (!mediator.HealthComponent.CanTakeDamage) return;
        DealDamageRequestRpc(mediator.HealthComponent.CurrentHealth, DamageTag.Neutral, mediator.PlayerId, mediator.PlayerId);
    }
    public void TakeLethalDamage(CharacterMediator damager, DamageTag tag)
    {
        if (!mediator.HealthComponent.CanTakeDamage) return;
        damager.NetworkInput.DealDamage(mediator.HealthComponent.CurrentHealth, tag, mediator, damager, true);
    }

    public void TakeDamage(int damage, DamageTag tag)
    {
        DealDamageRequestRpc(damage, tag, mediator.PlayerId, mediator.PlayerId);
    }

    [Rpc(SendTo.Server)]
    private void DealDamageRequestRpc(int damage, DamageTag tag, ulong targetID, ulong sourceID)
    {
        NotifyDealDamageRpc(damage, tag, targetID, sourceID);
    }

    [Rpc(SendTo.Everyone)]
    private void NotifyDealDamageRpc(int damage, DamageTag tag, ulong targetID, ulong sourceID)
    {
        var manager = CharacterManager.Instance;
        var hitMediator = manager.Mediators[targetID];

        hitMediator.HealthComponent.TakeDamage(damage, tag, mediator);

        var sourceMediator = manager.Mediators[sourceID];
        if ((sourceMediator.IsOwner || DataStorage.IsSinglePlayer)
            && sourceMediator != hitMediator)
        {
            NotifySubscribers(damage, tag, hitMediator);
        }
    }

    private void NotifySubscribers(int damage, DamageTag tag, CharacterMediator hitMediator)
    {
        OnDamageDealt?.Invoke(damage, tag);
        OnDamageDealtToMediator?.Invoke(damage, tag, hitMediator);
    }

    [Rpc(SendTo.Server)]
    public void RequestHealRpc(int healAmount, bool overHeal)
    {
        if (mediator.IsAlive)
        {
            NotifyHealRpc(healAmount, overHeal);
        }
    }
    [Rpc(SendTo.Everyone)]
    private void NotifyHealRpc(int healAmount, bool overHeal)
    {
        var health = mediator.HealthComponent;
        if (!overHeal)
        {
            health.CurrentHealth.Adjust(healAmount, ceiling: health.MaxHealth);
        }
        else
        {
            health.AdjustMaxHealth(healAmount, true);
        }
    }
}
