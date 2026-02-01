using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "HeroToolkit", menuName = "Abilities/Toolkit")]
public class CharacterToolkit : ScriptableObject
{
    [field: SerializeField] public HeroDatabase DatabaseEntry { get; private set; }
    [field: SerializeField] public string HeroName {  get; private set; }
    [field: SerializeField, TextArea] public string HeroDescription { get; private set; }

    [SerializeField] private MovementAbility movementAbility;
    [SerializeField] private UtilityAbility utilityAbility;
    [SerializeField] private PassiveAbility passiveAbility;
    [SerializeField] private AbilityPostMortem abilityPostMortem;
    [field: SerializeField] public GunConfig GunConfig { get; private set; }

    [Header("Visuals")]
    [field: SerializeField] public Color PrimaryColor { get; private set; }
    [field: SerializeField] public Sprite SplashArt { get; private set; }

    public (MovementAbility movementAbility,
        UtilityAbility utilityAbility,
        PassiveAbility passiveAbility,
        AbilityPostMortem abilityPostMortem)
        CreateAbilities(
        CharacterMediator mediator)
    {
        return
        (
            (MovementAbility)movementAbility.Factory(mediator),
            (UtilityAbility)utilityAbility.Factory(mediator),
            (PassiveAbility)passiveAbility.Factory(mediator),
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

    public string GetGunDescription()
    {
        return $"<b>{GunConfig.GunName}</b>\n{GunConfig.damage} damage\n{GunConfig.headshotDamage} HS damage\n{GunConfig.capacity} capacity\n{GunConfig.RPM} RPM\n{GunConfig.reloadDuration}s reload time\nRange: {GunConfig.bulletRange}";
    }

}

public enum HeroDatabase
{
    Recruit = 0,
    BabaYaga = 1,
    Dracula = 2,
    Djinn = 3
}