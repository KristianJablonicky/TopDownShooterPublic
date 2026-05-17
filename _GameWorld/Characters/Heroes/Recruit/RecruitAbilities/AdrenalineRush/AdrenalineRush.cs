using UnityEngine;

[CreateAssetMenu(fileName = "AdrenalineRush", menuName = "Abilities/Passive/AdrenalineRush")]
public class AdrenalineRush : PassiveAbility
{
    [SerializeField][Range(0, 100)] private int maxMoveBonus = 50;
    protected override void AbstractReset() { }

    protected override void SetUp()
    {
        owner.HealthComponent.CurrentHealth.OnValueSet += OnHealthChange;
    }


    private void OnHealthChange(int newHp)
    {
        SetNewBonus(maxMoveBonus / 100f * (1f - (float)newHp / owner.HealthComponent.MaxHealth));
    }

    private void SetNewBonus(float newBonus)
    {
        if (newBonus > maxMoveBonus / 100f
            || newBonus < 0f)
        {
            return;
        }
        owner.MovementController.MovementModifiers.AddOrChangeMultiplier(this, newBonus);
    }
    public override string _GetSpecificAttributes()
    {
        return $"Max bonus speed: {maxMoveBonus}%";
    }

    protected override void SafeSetUpWithRPCsReady() { }
}
