using System;
using UnityEngine;

public class HealthComponent : MonoBehaviour, IResettable
{
    public CharacterMediator mediator;
    [field: SerializeField] public int MaxHealth { get; private set; } = 100;
    [field: SerializeField] public bool NPC { get; private set; } = false;
    public ObservableValue<int> CurrentHealth { get; private set; }

    public event Action<int, CharacterMediator> DamageTaken;
    public event Action<int, CharacterMediator, CharacterMediator> M1TookDamageFromM2;

    private void Awake()
    {
        CurrentHealth = new(MaxHealth);
    }
    public bool CanTakeDamage => CurrentHealth > 0;
    public void TakeLethalDamage()
    {
        mediator.NetworkInput.TakeLethalDamage();
    }

    public void TakeDamage(int damage, CharacterMediator killer)
    {
        if (CurrentHealth <= 0) return;

        damage = Mathf.Min(damage, CurrentHealth);
        CurrentHealth.Adjust(-damage, 0);

        if (mediator.IsLocalPlayer)
        {
            DamageTaken?.Invoke(damage, killer);
        }
        else
        {
            M1TookDamageFromM2?.Invoke(damage, mediator, killer);
        }
        
        if (CurrentHealth <= 0)
        {
            mediator.Die(killer);
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
        CurrentHealth.Set(MaxHealth);
    }
}
