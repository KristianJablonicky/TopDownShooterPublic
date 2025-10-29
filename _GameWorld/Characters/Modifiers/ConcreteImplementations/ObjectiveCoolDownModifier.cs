public sealed class ObjectiveCoolDownModifier : ObjectiveModifier
{
    private const float cdMultPerStack = 0.7f;

    protected override void AdjustEffect(int stackCount)
    {
        UnityEngine.Debug.Log($"{this} -> {stackCount}");
        for (int i = 0; i < stackCount; i++)
        {
            owner.AbilityManager.MultiplyCoolDowns(cdMultPerStack);
        }
    }

    protected override string GetDescriptionInternal() => "Reduce cooldowns";
}
