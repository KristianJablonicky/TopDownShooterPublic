using System;
using System.Collections;
using TMPro;
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
    public AttackersTimer AttackersTimer { get; private set; }

    private TeamData attackers, defenders;


    private bool defenders1stPosBlocked, attackers1stPosBlocked;

    private CharacterManager manager;
    protected override void OverriddenAwake() => enabled = false;

    public void AllPlayersPickedATeam(ulong[] sortedIDs)
    {
        RPCs.RequestGameStartRPC(sortedIDs);
    }

    public void StartNewGame(CharacterManager manager)
    {
        this.manager = manager;
        GameInProgress = true;
        foreach (var player in manager.Players)
        {
            player.Value.Killed += OnPlayerDeath;
        }

        AttackersTimer = new(roundTime);
        AttackersTimer.TimeRanOut += OnTimeRanOut;
        roundTimerUI.Bind(AttackersTimer.TimeRemaining);

        attackers = manager.Teams[Team.Orange];
        defenders = manager.Teams[Team.Cyan];

        SetUpColors();

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

    private void Update() => AttackersTimer.IUpdate(Time.deltaTime);

    private void OnPlayerDeath(CharacterMediator killedPlayer, CharacterMediator killer)
    {
        var team = killedPlayer.playerData.Team;
        var teamMate = team.GetTeamMate(killedPlayer.playerData);
        if (!teamMate.Mediator.IsAlive)
        {
            // both players are dead, round lost for this team
            RoundOver(team, manager.LocalPlayer.Team == team);
        }

        UpdateScores(killedPlayer, killer, teamMate.Mediator);
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
    private void RoundOver(TeamData losingTeam, bool localPlayerLost)
    {
        var winningTeam = manager.Teams[losingTeam.GetTheEnemyTeam()];
        winningTeam.AddWin();
        var color = winningTeam.Name == Team.Cyan ? Colors.Cyan : Colors.Orange;
        if (localPlayerLost)
        {
            GameStateNotifications.Instance.ShowMessage("Round lost!", color);
        }
        else
        {
            GameStateNotifications.Instance.ShowMessage("Round won!", color);
        }
        StartCoroutine(EndRound());
    }
    public IEnumerator EndRound()
    {
        enabled = false;
        RoundEnded?.Invoke();
        yield return new WaitForSeconds(roundStartDelay);
        StartNewRound(true);
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
            player.Mediator.MovementController.SetPosition(spawns[index].position);
        }

        if (player.Mediator.IsLocalPlayer)
        {
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
                UnityEngine.Random.insideUnitCircle * radius
            );
        }
    }
    public void SetRPCs(GameStateManagerRPCs RPCs) => this.RPCs = RPCs;
}

public enum Role
{
    Attacker,
    Defender
}
