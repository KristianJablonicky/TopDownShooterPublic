public sealed class ObjectiveReloadSpeedModifier : ObjectiveModifier
{
    private const float speedPerStackMultiplier = 0.75f;

    protected override void AdjustEffect(int stackCount)
    {
        for (int i = 0; i < stackCount; i++)
        {
            owner.Gun.ShootManager.MultiplyMultiplier(speedPerStackMultiplier);
        }
    }

    protected override string GetDescriptionInternal() => "Increase reload speed";
}
