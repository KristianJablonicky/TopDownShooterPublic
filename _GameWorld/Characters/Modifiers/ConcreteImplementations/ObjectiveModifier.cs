public abstract class ObjectiveModifier : IModifierStrategy
{
    protected CharacterMediator owner;
    public void Apply(CharacterMediator owner, Modifier modifier)
    {
        this.owner = owner;
        AdjustEffect(modifier.Stacks);
        modifier.Stacks.OnAdjusted += (_, newVal) => AdjustEffect(newVal);
    }
    protected abstract void AdjustEffect(int stackCount);

    public void Expire(CharacterMediator owner)
    {
        UnityEngine.Debug.LogWarning("Objective buff expiring");
    }

    protected abstract string GetDescriptionInternal();

    public string GetDescription() => GetDescriptionInternal();
}
