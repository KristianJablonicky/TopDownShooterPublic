using System;
using System.Collections.Generic;
using System.Linq;

public class ModifiersList
{
    public List<Modifier> Modifiers { get; private set; }
    public event Action <Modifier> ModifierAdded;
    public ModifiersList()
    {
        Modifiers = new();
    }

    public bool ModifierExistsOfType(Type searchedForType)
        => Modifiers.Any(m => m.ModifierSystemType == searchedForType);
    public bool CheckModifierAndAdjustIfAlreadyOwned(Type searchedForType, int stacks, float duration)
    {
        var alreadyExists = Modifiers.FirstOrDefault(m => m.ModifierSystemType == searchedForType);
        if (alreadyExists is not null)
        {
            alreadyExists.Stacks.Adjust(stacks);
            alreadyExists.Duration.Set(duration);
            return true;
        }
        return false;
    }
    public void AddModifier(Modifier modifier)
    {
        Modifiers.Add(modifier);
        ModifierAdded?.Invoke(modifier);

        modifier.Expired += () =>
        {
            Modifiers.Remove(modifier);
        };
    }
}
