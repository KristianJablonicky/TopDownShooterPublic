using UnityEngine;

public class AbilityManager : MonoBehaviour, IResettable
{
    [SerializeField] private CharacterMediator mediator;
    [SerializeField] private CharacterToolkit toolkit;

    public ActiveAbility MovementAbility { get; private set; }
    public ActiveAbility UtilityAbility { get; private set; }
    public Ability PassiveAbility { get; private set; }
    public AbilityPostMortem AbilityPostMortem { get; private set; }

    private Ability[] abilities;
    private ActiveAbility[] activeAbilities;
    private int abilityCount, activeAbilityCount;
    private void Awake()
    {
        (MovementAbility, UtilityAbility, PassiveAbility, AbilityPostMortem)
            = toolkit.CreateAbilities(mediator);

        abilities = new[] { MovementAbility, UtilityAbility, PassiveAbility, AbilityPostMortem };
        abilityCount = abilities.Length;

        activeAbilities = new[] { MovementAbility, UtilityAbility, AbilityPostMortem };
        activeAbilityCount = activeAbilities.Length;

        enabled = true;
    }
    public void SetCharacterRPCs(AbilityRPCs rpcs)
    {
        for (int i = 0; i < abilityCount; i++)
        {
            abilities[i].SetAbilityRPC(rpcs);
        }
    }
    public void AssignTeamMate(CharacterMediator teamMate)
    {
        AbilityPostMortem.Init(teamMate);
    }

    private void Update()
    {
        for (int i = 0; i < activeAbilityCount; i++)
        {
            activeAbilities[i].IUpdate(Time.deltaTime);
        }
    }

    public void Reset()
    {
        for (int i = 0; i < abilityCount; i++)
        {
            abilities[i].Reset();
        }
    }

    public void DisableAbilities(float abilityDisableTime)
    {
        for (int i = 0; i < activeAbilityCount; i++)
        {
            activeAbilities[i].SetCoolDown(abilityDisableTime);
        }
    }
}
