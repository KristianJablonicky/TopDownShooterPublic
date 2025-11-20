using System;
using System.Collections;
using UnityEngine;

public class GameStateManager : SingletonMonoBehaviour<GameStateManager>
{
    [SerializeField] private Transform[] spawnLocationsDefenders, spawnLocationsAttackers;
    [SerializeField] private float roundTime = 90f, roundStartDelay = 3f;

    private GameStateManagerRPCs RPCs;
    private int roundNumber = 0;

    public bool GameInProgress { get; private set; }
    public event Action GameStarted, NewRoundStarted, RoundEnded;
    public event Action<bool> RoundWon, GameWon;
    public event Action<int, TeamData> RoundNumberWonByTeam;
    public AttackersTimer AttackersTimer { get; private set; }

    private TeamData attackers, defenders;


    private bool defenders1stPosBlocked, attackers1stPosBlocked;
    public bool RoundDecided { get; private set; } = false;

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

        defenders = manager.Teams[Team.Orange];
        attackers = manager.Teams[Team.Cyan];

        GameStarted?.Invoke();
        StartNewRound();
    }


    private void OnPlayerDeath(CharacterMediator killedPlayer, CharacterMediator killer)
    {
        var losingTeam = killedPlayer.playerData.Team;
        var teamMate = losingTeam.GetTeamMate(killedPlayer.playerData);

        UpdateScores(killedPlayer, killer, teamMate.Mediator);
        if (!RoundDecided && !teamMate.Mediator.IsAlive)
        {
            // both players are dead, round lost for this team
            RoundOver(losingTeam.EnemyTeamData);
        }
    }

    private void UpdateScores(CharacterMediator killedPlayer, CharacterMediator killer, CharacterMediator killedPlayerTeamMate)
    {
        if (killer != null && killer != killedPlayer)
        {
            int adjustment = killer != killedPlayerTeamMate ? 1 : -1;
            killer.playerData.PlayerScore.Kills.Adjust(adjustment);
        }
        killedPlayer.playerData.PlayerScore.Deaths++;

    }

    private void OnTimeRanOut()
    {
        RoundOver(defenders);
        enabled = false;
    }

    private async void RoundOver(TeamData winningTeam)
    {
        winningTeam.AddWin();
        
        var localPlayerWon = winningTeam.LocalPlayersTeam;

        ShowNotification(winningTeam, localPlayerWon, "Round won!", "Round lost!");

        RoundDecided = true;

        enabled = false;
        RoundEnded?.Invoke();
        RoundWon?.Invoke(winningTeam.LocalPlayersTeam);
        RoundNumberWonByTeam?.Invoke(roundNumber, winningTeam);

        if (HasGameEnded(winningTeam, localPlayerWon)) return;

        await TaskExtensions.Delay(roundStartDelay);
        StartNewRound();
    }

    public void ObjectiveCaptured(TeamData teamData)
    {
        if (RoundDecided) return;
        RoundOver(teamData);
    }

    private void ShowNotification(TeamData team, bool won, string winMessage, string lossMessage)
    {
        var color = team.Name == Team.Cyan ? Colors.Cyan : Colors.Orange;
        GameStateNotifications.Instance.ShowMessage(
            won ? winMessage : lossMessage,
            color
        );
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

    public void StartNewRound()
    {
        enabled = true;
        AttackersTimer.Reset();

        if (roundNumber == Constants.roundsToWinMatch - 1)
        {
            (defenders, attackers) = (attackers, defenders);
        }
        roundNumber++;
        
        attackers.CurrentRole = Role.Attacker;
        defenders.CurrentRole = Role.Defender;

        defenders1stPosBlocked = false;
        attackers1stPosBlocked = false;

        foreach (var player in manager.PlayerData)
        {
            SetUpPlayer(player);
        }
        RoundDecided = false;
        NewRoundStarted?.Invoke();
    }

    private void SetUpPlayer(PlayerData player)
    {
        player.Mediator.Reset();
        bool isDefender = player.Team == defenders;

        ref bool firstPosBlocked = ref (isDefender ? ref defenders1stPosBlocked : ref attackers1stPosBlocked);
        var spawns = isDefender ? spawnLocationsDefenders : spawnLocationsAttackers;
        var role = isDefender ? Role.Defender : Role.Attacker;

        player.Mediator.SetRole(role);
        var index = firstPosBlocked ? 1 : 0;
        firstPosBlocked = true;

        if (player.Mediator.IsLocalPlayer)
        {
            player.Mediator.MovementController.SetPosition(spawns[index].position, null);
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
