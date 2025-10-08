using UnityEngine;

[CreateAssetMenu(fileName = "AdrenalineRush", menuName = "Abilities/Passive/AdrenalineRush")]
public class AdrenalineRush : Ability
{
    [SerializeField] private int maxMoveBonus = 50;
    private float currentMovementBonus = 0;
    protected override void AbstractReset() { }

    protected override void SetUp()
    {
        owner.HealthComponent.CurrentHealth.OnValueSet += OnHealthChange;
    }

    private void OnHealthChange(int newHp)
    {
        SetNewBonus(maxMoveBonus * (1f - (float)newHp / owner.HealthComponent.MaxHealth) / 100f);
    }

    private void SetNewBonus(float newBonus)
    {
        owner.MovementController.movementMultiplier += (newBonus - currentMovementBonus);
        currentMovementBonus = newBonus;
    }
}
