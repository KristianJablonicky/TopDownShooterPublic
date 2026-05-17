public sealed class ObjectiveVisionModifier : ObjectiveModifier
{
    private const float visionPerStack = 1f;

    protected override void AdjustEffect(int stackCount)
    {
        owner.VisionRange.ModifiableValue.AddOrChangeMultiplier(this, stackCount * visionPerStack);
    }

    protected override string GetDescriptionInternal() => "Increase vision range";
}
