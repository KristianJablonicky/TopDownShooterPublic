using System;
using UnityEngine;

public class HealthComponent : MonoBehaviour, IResettable
{
    [field: SerializeField] public CharacterMediator Mediator { get; private set; }
    [field: SerializeField] public int MaxHealth { get; private set; } = 100;
    [field: SerializeField] public bool NPC { get; private set; } = false;
    public ObservableValue<int> CurrentHealth { get; private set; }

    public event Action DamageTaken;
    public event Action<DamageTag> DamageTakenWithTags;
    public event Action<int, CharacterMediator> DamageTakenFromMediator;
    public event Action<int, CharacterMediator, CharacterMediator> M1TookDamageFromM2;

    private int baseMaxHealth;
    private void Awake()
    {
        CurrentHealth = new(MaxHealth);
        baseMaxHealth = MaxHealth;
    }
    public bool CanTakeDamage => CurrentHealth > 0;
    public void TakeLethalDamage()
    {
        Mediator.NetworkInput.TakeLethalDamage();
    }
    public void TakeLethalDamage(CharacterMediator damager, DamageTag tag)
    {
        Mediator.NetworkInput.TakeLethalDamage(damager, tag);
    }

    public void TakeDamage(int damage, DamageTag tag, CharacterMediator killer)
    {
        if (CurrentHealth <= 0) return;

        damage = Mathf.Min(damage, CurrentHealth);
        CurrentHealth.Adjust(-damage, 0);

        DamageTaken?.Invoke();

        DamageTakenFromMediator?.Invoke(damage, killer);

        M1TookDamageFromM2?.Invoke(damage, Mediator, killer);

        DamageTakenWithTags?.Invoke(tag);


        if (CurrentHealth <= 0)
        {
            Mediator.Die(killer);
        }
    }

    public void AdjustMaxHealth(int adjustment, bool adjustCurrentHealth)
    {
        MaxHealth += adjustment;
        if (adjustCurrentHealth) CurrentHealth.Adjust(adjustment);
        else CurrentHealth.Adjust(0); // Notify observers of max health change
    }

    public void Reset()
    {
        MaxHealth = baseMaxHealth;
        CurrentHealth.Set(baseMaxHealth);
    }
}

public enum DamageTag
{
    Shot,
    HeadShot,
    Ability,
    Neutral
}