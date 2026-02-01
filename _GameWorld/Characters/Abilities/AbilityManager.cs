using UnityEngine;

public class AbilityManager : MonoBehaviour, IResettable
{
    [SerializeField] private CharacterMediator mediator;
    [SerializeField] private CharacterToolkit toolkit;
    [SerializeField] private GameObject rangeIndicator;
    [field: SerializeField] public SoundPlayer SoundPlayer {  get; private set; }

    public MovementAbility MovementAbility { get; private set; }
    public UtilityAbility UtilityAbility { get; private set; }
    public PassiveAbility PassiveAbility { get; private set; }
    public AbilityPostMortem AbilityPostMortem { get; private set; }

    private Ability[] abilities;
    private ActiveAbility[] activeAbilities;
    private int abilityCount, activeAbilityCount;
    private void Awake()
    {
        (MovementAbility, UtilityAbility, PassiveAbility, AbilityPostMortem)
            = toolkit.CreateAbilities(mediator);

        abilities = new[] { (Ability)MovementAbility, UtilityAbility, PassiveAbility, AbilityPostMortem };
        abilityCount = abilities.Length;

        activeAbilities = new[] { (ActiveAbility)MovementAbility, UtilityAbility, AbilityPostMortem };
        activeAbilityCount = activeAbilities.Length;

        for (int i = 0; i < activeAbilityCount; i++)
        {
            activeAbilities[i].SetRangeIndicatorReference(rangeIndicator);
        }

        for (int i = 0; i < abilityCount; i++)
        {
            abilities[i].SetSoundPlayer(SoundPlayer);
        }

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
    public void DisableAbilities(float abilityDisableTime, float longerDisableTime)
    {
        float disableTime;
        for (int i = 0; i < activeAbilityCount; i++)
        {
            if (activeAbilities[i].availableBeforeRoundStart)
            {
                disableTime = abilityDisableTime;
            }
            else
            {
                disableTime = longerDisableTime;
            }
            activeAbilities[i].SetCoolDown(disableTime);
        }
    }

    public void MultiplyCoolDowns(float multiplier)
    {
        for (int i = 0; i < activeAbilityCount; i++)
        {
            activeAbilities[i].CoolDown *= multiplier;
        }
    }

    public Ability GetAbility(AbilityType type) => abilities[(int)type];
    public CharacterToolkit Toolkit => toolkit;
}
