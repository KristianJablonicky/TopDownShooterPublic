using System;
using Unity.Netcode;
using UnityEngine;

public class DefenderObjectiveManager : SingletonMonoBehaviour<DefenderObjectiveManager>
{
    [SerializeField] private DefenderObjective[] objectives;

    public event Action<DefenderObjective> ObjectiveActivated;

    private DefenderObjective currentlyActiveObjective;

    private GameStateManager manager;

    private void Start()
    {
        if (DataStorage.IsSinglePlayer())
        {
            CleanUp();
            return;
        }

        manager = GameStateManager.Instance;
        manager.NewRoundStarted += OnNewRoundStarted;

        // TODO: reconsider and potentially delete the whole reward buff/bonus system.
        //manager.RoundWon += OnRoundEnded;
        manager.RoundEnded += () => currentlyActiveObjective.GetDeactivated();
    }

    private async void OnNewRoundStarted()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            // TODO: replace artificial delay with proper objective selection logic
            await TaskExtensions.Delay(0.1f);
            manager.PickObjective((uint)UnityEngine.Random.Range(0, objectives.Length));
        }
    }

    public void ObjectivePicked(uint pickedObjective)
    {
        currentlyActiveObjective = objectives[pickedObjective];
        currentlyActiveObjective.GetActivated();
        ObjectiveActivated?.Invoke(currentlyActiveObjective);
    }

    private void OnRoundEnded(bool localPlayerWon)
    {
        var localMediator = CharacterManager.Instance.LocalPlayerMediator;
        var localPlayerTeam = localMediator.playerData.Team;
        if (localPlayerWon)
        {
            var stacks = localMediator.role == Role.Defender ?
                Constants.defendersObjectiveRewardStacks :
                Constants.attackersObjectiveRewardStacks;

            GrantBuff(localPlayerTeam, stacks);
        }
        else
        {
            var stacks = localMediator.role == Role.Defender ?
                Constants.attackersObjectiveRewardStacks :
                Constants.defendersObjectiveRewardStacks;
            GrantBuff(localPlayerTeam.EnemyTeamData, stacks);
        }

        currentlyActiveObjective.GetDeactivated();
    }

    private void GrantBuff(TeamData team, int stacksCount)
    {
        foreach (var player in team.Players)
        {
            var (icon, reward) = currentlyActiveObjective.GetBuff();
            new Modifier(player.Mediator,
                BuffFactory(reward),
                1f,
                stacksCount,
                icon);
        }
    }

    private IModifierStrategy BuffFactory(DefenderObjectiveReward reward)
    {
        return reward switch
        {
            DefenderObjectiveReward.Vision => new ObjectiveVisionModifier(),
            DefenderObjectiveReward.ReloadSpeed => new ObjectiveReloadSpeedModifier(),
            DefenderObjectiveReward.Vitality => new ObjectiveVitalityModifier(),
            DefenderObjectiveReward.CoolDown => new ObjectiveCoolDownModifier(),
            _ => null
        };
    }


    private void CleanUp()
    {
        for (int i = objectives.Length - 1; i >= 0; i--)
        {
            Destroy(objectives[i].gameObject);
        }
        Destroy(gameObject);
    }

}

public enum DefenderObjectiveReward
{
    Vision,
    ReloadSpeed,
    Vitality,
    CoolDown
}