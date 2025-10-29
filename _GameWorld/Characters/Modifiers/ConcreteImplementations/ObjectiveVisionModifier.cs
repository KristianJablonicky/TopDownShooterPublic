public sealed class ObjectiveVisionModifier : ObjectiveModifier
{
    private const float visionPerStack = 1f;

    protected override void AdjustEffect(int stackCount)
    {
        owner.PlayerVision.AdjustBaseVisionRange(stackCount * visionPerStack);
    }

    protected override string GetDescriptionInternal() => "Increase vision range";
}
