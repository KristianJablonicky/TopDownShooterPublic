using System;
using System.Collections;
using System.Drawing;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameStateManager : SingletonMonoBehaviour<GameStateManager>
{
    [SerializeField] private Transform[] spawnLocationsDefenders, spawnLocationsAttackers;
    [SerializeField] private float roundTime = 90f, roundStartDelay = 3f;

    [SerializeField] private ObservableVariableBinder roundTimerUI;
    [SerializeField] private TMP_Text currentRoleText;

    private GameStateManagerRPCs RPCs;


    public bool GameInProgress { get; private set; }
    public event Action GameStarted, NewRoundStarted, RoundEnded;
    public event Action<bool> RoundWon, GameWon;
    public AttackersTimer AttackersTimer { get; private set; }

    private TeamData attackers, defenders;


    private bool defenders1stPosBlocked, attackers1stPosBlocked;
    private bool roundDecided = false;

    private CharacterManager manager;
    protected override void OverriddenAwake() => enabled = false;

    private void Update() => AttackersTimer.IUpdate(Time.deltaTime);

    #region Multiplayer
    public void AllPlayersPickedATeam(ulong[] sortedIDs)
    {
        RPCs.RequestGameStartRPC(sortedIDs);
    }

    public void StartNewGame(CharacterManager manager)
    {
        this.manager = manager;
        GameInProgress = true;
        foreach (var player in manager.Mediators)
        {
            player.Value.Killed += OnPlayerDeath;
        }

        AttackersTimer = new(roundTime);
        AttackersTimer.TimeRanOut += OnTimeRanOut;
        roundTimerUI.Bind(AttackersTimer.TimeRemaining, false);

        attackers = manager.Teams[Team.Orange];
        defenders = manager.Teams[Team.Cyan];

        SetUpColors();

        SetUpObjectives();

        GameStarted?.Invoke();
        StartNewRound(false);
    }

    private void SetUpColors()
    {
        var colors = CommonColors.Instance;

        currentRoleText.color = CommonColors.GetTeamColor((int)manager.LocalPlayer.Team.Name);
        /*
        static void SetColors(TeamData team, Color color)
        {
            foreach (var player in team.Players)
            {
                player.Mediator.SpriteRenderer.color = color;
            }
        }
        SetColors(attackers, colors.Orange * 1.5f);
        SetColors(defenders, colors.Cyan   * 2.5f);
        */
    }

    private void SetUpObjectives()
    {
        DefenderObjectiveManager.Instance.ObjectiveActivated += objective =>
        {
            currentRoleText.text += $" ({objective.Location})";
        };
    }


    private void OnPlayerDeath(CharacterMediator killedPlayer, CharacterMediator killer)
    {
        var losingTeam = killedPlayer.playerData.Team;
        var teamMate = losingTeam.GetTeamMate(killedPlayer.playerData);

        UpdateScores(killedPlayer, killer, teamMate.Mediator);
        if (!roundDecided && !teamMate.Mediator.IsAlive)
        {
            // both players are dead, round lost for this team
            RoundOver(losingTeam, manager.LocalPlayer.Team != losingTeam);
        }
    }

    private void UpdateScores(CharacterMediator killedPlayer, CharacterMediator killer, CharacterMediator killedPlayerTeamMate)
    {
        if (killer != null && killer != killedPlayer)
        {
            int adjustment = killer != killedPlayerTeamMate ? 1 : -1;
            killer.playerData.PlayerScore.Kills += adjustment;
        }
        killedPlayer.playerData.PlayerScore.Deaths++;

    }

    private void OnTimeRanOut()
    {
        foreach(var player in attackers.Players)
        {
            if (player.Mediator.IsAlive && player.Mediator.IsLocalPlayer)
            {
                player.Mediator.HealthComponent.TakeLethalDamage();
            }
        }
        enabled = false;
    }

    private void RoundOver(TeamData losingTeam, bool localPlayerWon)
    {
        var winningTeam = losingTeam.EnemyTeamData;
        winningTeam.AddWin(winningTeam.Players[0].Mediator.role);

        ShowNotification(winningTeam, localPlayerWon, "Round won!", "Round lost!");

        roundDecided = true;

        if (HasGameEnded(winningTeam, localPlayerWon)) return;

        StartCoroutine(EndRound(localPlayerWon));
    }

    public void ObjectiveCaptured()
    {
        if (roundDecided) return;
        RoundOver(defenders,
            CharacterManager.Instance.LocalPlayerMediator.role == Role.Attacker
        );
    }

    public void PickObjective(uint objectiveId)
    {
        RPCs.PickObjectiveRPC(objectiveId);
    }

    private void ShowNotification(TeamData team, bool won, string winMessage, string lossMessage)
    {
        var color = team.Name == Team.Cyan ? Colors.Cyan : Colors.Orange;
        GameStateNotifications.Instance.ShowMessage(
            won ? winMessage : lossMessage,
            color
        );
    }

    private IEnumerator EndRound(bool won)
    {
        enabled = false;
        RoundEnded?.Invoke();
        RoundWon?.Invoke(won);
        yield return new WaitForSeconds(roundStartDelay);
        StartNewRound(true);
    }
    private bool HasGameEnded(TeamData winningTeam, bool localPlayerWon)
    {
        if (winningTeam.Wins >= Constants.roundsToWinMatch)
        {
            StartCoroutine(EndGame(winningTeam, localPlayerWon));
            return true;
        }
        else
        {
            return false;
        }
    }
    private IEnumerator EndGame(TeamData winningTeam, bool localPlayerWon)
    {
        yield return new WaitForSeconds(roundStartDelay * 1.5f);
        var suffix = $"{winningTeam.Wins} : {winningTeam.EnemyTeamData.Wins}";
        ShowNotification(winningTeam, localPlayerWon, $"-Game won!-\n{suffix}", $"-Game lost!-\n{suffix}");
        GameWon?.Invoke(localPlayerWon);
    }

    public void StartNewRound(bool switchTeams)
    {
        enabled = true;
        AttackersTimer.Reset();

        if (switchTeams)
        {
            (defenders, attackers) = (attackers, defenders);
        }

        defenders1stPosBlocked = false;
        attackers1stPosBlocked = false;

        foreach (var player in manager.PlayerData)
        {
            SetUpPlayer(player);
        }
        roundDecided = false;
        NewRoundStarted?.Invoke();
    }

    private void SetUpPlayer(PlayerData player)
    {
        player.Mediator.Reset();
        bool isDefender = player.Team == defenders;

        ref bool firstPosBlocked = ref (isDefender ? ref defenders1stPosBlocked : ref attackers1stPosBlocked);
        var spawns = isDefender ? spawnLocationsDefenders : spawnLocationsAttackers;
        var role = isDefender ? Role.Defender : Role.Attacker;

        player.Mediator.role = role;
        var index = firstPosBlocked ? 1 : 0;
        firstPosBlocked = true;

        if (player.Mediator.IsLocalPlayer)
        {
            player.Mediator.MovementController.SetPosition(spawns[index].position, null);
            currentRoleText.text = role.ToString();
        }
    }

    public IEnumerator WarmUpRespawn(CharacterMediator mediator)
    {
        if (GameInProgress) yield break;
        yield return new WaitForSeconds(3f);
        if (GameInProgress) yield break;

        if (mediator == null) yield break;

        mediator.Reset();
        var radius = UnityEngine.Random.Range(2f, 5f);

        if (mediator.IsLocalPlayer)
        {
            mediator.MovementController.SetPosition(
                mediator.GetPosition() +
                UnityEngine.Random.insideUnitCircle *
                radius
            );
        }
    }
    public void SetRPCs(GameStateManagerRPCs RPCs) => this.RPCs = RPCs;

    #endregion

    #region Singleplayer
    public event Action TrainingStarted;
    public event Action<bool> TrainingEnded;
    public void StartTraining(float trainingDuration, SinglePlayerManager manager)
    {
        if (AttackersTimer is null)
        {
            AttackersTimer = new(trainingDuration);
            AttackersTimer.TimeRanOut += () => TrainingTimeOut(manager);
            roundTimerUI.Bind(AttackersTimer.TimeRemaining, false);
        }
        
        AttackersTimer.Reset();
        enabled = true;
        TrainingStarted?.Invoke();
    }

    private void TrainingTimeOut(SinglePlayerManager manager)
    {
        enabled = false;
        var newHighScore = manager.StopTraining(false);
        if (newHighScore.HasValue)
        {
            TrainingEnded?.Invoke(newHighScore.Value);
        }
    }

    public void StopTraining()
    {
        AttackersTimer?.Reset();
        enabled = false;
    }

    #endregion

    public TeamData GetTeamDataByRole(Role role) => role == Role.Attacker ? attackers : defenders;

}

public enum Role
{
    Attacker,
    Defender
}
