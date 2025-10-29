public sealed class ObjectiveVitalityModifier : ObjectiveModifier
{
    private const int healthPerStack = 25;

    protected override void AdjustEffect(int stackCount)
    {
        owner.HealthComponent.AdjustMaxHealth(stackCount * healthPerStack, true);
    }

    protected override string GetDescriptionInternal() => "Increase max health";
}
