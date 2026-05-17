public interface IModifierStrategy
{
    public void Apply(CharacterMediator owner, Modifier modifier);
    public void Expire(CharacterMediator owner);
    public string GetDescription();
    public bool RealTimeDuration();
    public bool ExpireOnRoundEnd();
    public ModifierType ModifierType { get; }
}
public enum ModifierType
{
    Buff,
    Debuff
}
