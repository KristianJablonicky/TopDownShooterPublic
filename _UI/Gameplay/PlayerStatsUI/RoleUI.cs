using TMPro;
using UnityEngine;

public class RoleUI : MonoBehaviour
{
    [SerializeField] private TMP_Text roleTMP;
    private const string attackerPrefix = "Attacker ",
                         defenderPrefix = "Defender ",
                         defenderObjectiveText = defenderPrefix + "(Channel the objective)",
                         attackerText = attackerPrefix + "(Channel the objective)",
                         defenderText = defenderPrefix + "(Obtain blood)";
                         
    private void Start()
    {
        PlayerNetworkInput.PlayerSpawned += OnPlayerSpawned;
        GameStateManager.Instance.GameStarted += OnGameStart;
    }

    private void OnGameStart()
    {
        var owner = CharacterManager.Instance.LocalPlayer;
        roleTMP.color = CommonColors.GetTeamColor(
            owner.Team.Name
        );
        
        owner.Mediator.ScoredAKill -= OnKill;
        owner.Mediator.Died -= OnDeath;
    }

    private void OnPlayerSpawned(CharacterMediator owner)
    {
        owner.NewRoleAssigned += OnRoleSet;
        owner.BloodManager.OnBloodPickedUp += OnBloodPickedUp;

        owner.ScoredAKill += OnKill;
        owner.Died += OnDeath;
    }

    private void OnRoleSet(Role role)
    {
        roleTMP.text = GetInstructionText(role);
    }

    private void OnBloodPickedUp()
    {
        roleTMP.text = defenderObjectiveText;
    }

    private string GetInstructionText(Role role)
    {
        return role switch
        {
            Role.Defender => defenderText,
            Role.Attacker => attackerText,
            _ => throw new System.NotImplementedException()
        };
    }


    private int kills = 0, deaths = 0;
    private void OnKill(CharacterMediator victim, CharacterMediator player)
    {
        kills++;
        UpdateWarmUp();
    }
    private void OnDeath(CharacterMediator mediator)
    {
        deaths++;
        UpdateWarmUp();
    }
    private void UpdateWarmUp()
    {
        roleTMP.text = $"K/D : {kills}/{deaths}";
    }
}
