using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "HeroToolkit", menuName = "Abilities/Toolkit")]
public class CharacterToolkit : ScriptableObject
{
    [field: FormerlySerializedAs("enumEntry")]
    [field: SerializeField] public HeroDatabase DatabaseEntry { get; private set; }
    [field: FormerlySerializedAs("heroName")]
    [field: SerializeField] public string HeroName {  get; private set; }
    [field: FormerlySerializedAs("heroDescription")]
    [field: SerializeField, TextArea] public string HeroDescription { get; private set; }

    [SerializeField] private ActiveAbility movementAbility;
    [SerializeField] private ActiveAbility utilityAbility;
    [SerializeField] private Ability passiveAbility;
    [SerializeField] private AbilityPostMortem abilityPostMortem;

    public (ActiveAbility movementAbility,
        ActiveAbility utilityAbility,
        Ability passiveAbility,
        AbilityPostMortem abilityPostMortem)
        CreateAbilities(
        CharacterMediator mediator)
    {
        return
        (
            (ActiveAbility)movementAbility.Factory(mediator),
            (ActiveAbility)utilityAbility.Factory(mediator),
            passiveAbility.Factory(mediator),
            (AbilityPostMortem)abilityPostMortem.Factory(mediator)
        );
    }

    public Ability GetAbility(AbilityType type)
    {
        return type switch
        {
            AbilityType.Movement => movementAbility,
            AbilityType.Utility => utilityAbility,
            AbilityType.Passive => passiveAbility,
            AbilityType.PostMortem => abilityPostMortem,
            _ => null,
        };
    }
}

public enum HeroDatabase
{
    Recruit = 0,
    BabaYaga = 1,
    Dracula = 2
}