using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class CharacterManager : SingletonMonoBehaviour<CharacterManager>
{
    [SerializeField] private CameraController playerCamera, allyCamera;
    public Dictionary<ulong, CharacterMediator> Players { get; private set; }
    public PlayerData T1P1, T1P2, T2P1, T2P2;
    public PlayerData LocalPlayer { get; private set; }
    public CharacterMediator LocalPlayerMediator { get; private set; }
    public PlayerData[] PlayerData { get; private set; }

    public Dictionary<Team, TeamData> Teams { get; set; }
    public TeamData Orange { get; set; }
    public TeamData Cyan { get; set; }

    public event Action<CharacterMediator> CharacterRegistered;
    protected override void OverriddenAwake()
    {
        Players = new();
        PlayerData = new[] { T1P1, T1P2, T2P1, T2P2 };
        //PlayerData = new[] { T1P1, T2P1 };
        Orange = new(Team.Orange);
        Cyan = new(Team.Cyan);
        Teams = new()
        {
            { Team.Orange,  Orange },
            { Team.Cyan,    Cyan   }
        };
    }

    private void Start()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
    }

    private bool alreadyDisconnected = false;
    private void OnClientDisconnect(ulong disconnectedId)
    {
        if (alreadyDisconnected) return;

        alreadyDisconnected = true;
        string disconnectString;
        if (Players[disconnectedId].playerData is null)
        {
            disconnectString = "A player has disconnected before the match started.";
        }
        else
        {
            disconnectString = $"{Players[disconnectedId].playerData.Name} has disconnected";
        }
        Debug.Log(disconnectString);
        DataStorage.Instance.disconnectReason = disconnectString;

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
            Destroy(NetworkManager.Singleton.gameObject);
        }
        SceneManager.GoToTheMainMenu();
    }

    public void RegisterCharacter(CharacterMediator character)
    {
        var uID = character.PlayerId;
        Debug.Log($"Registering {uID}");
        Players.Add(uID, character);

        CharacterRegistered?.Invoke(character);

        if (character.IsLocalPlayer)
        {
            playerCamera.InitialSetTarget(character);

            var connectedPlayers = NetworkManager.Singleton.ConnectedClientsList.Count;
            var x = 6f;
            var direction = connectedPlayers % 2 == 0 ? -1f : 1f;
            var y = connectedPlayers < 3 ? -1f : 1f;
            var spawnPos = new Vector2(x * direction, y);
            Debug.Log(connectedPlayers);
            Debug.Log(spawnPos);
            character.MovementController.SetPosition(spawnPos);

            LocalPlayerMediator = character;
        }

        if (Players.Count == Constants.maxPlayerCount)
        {
            AllPlayersJoined();
        }
    }

    private void AllPlayersJoined()
    {
        GameStateNotifications.Instance.ShowMessage("Join a team...");
        TeamSelector.Instance.AllPlayersConnected(this);
    }

    public void AllPlayersPickedATeam(ulong[] playerIDs)
    {
        
        foreach (var id in playerIDs)
        {
            var mediator = Players[id];

            var teamEnum = mediator.GetPosition().x < 0f ? Team.Orange : Team.Cyan;
            var team = Teams[teamEnum];

            Debug.Log($"Player s objektivnym indexom {id} patri timu {team.Name}");
            UnityEngine.Debug.Log($"Jeho pozicia jest {mediator.GetPosition()}");

            var newPlayerData = new PlayerData(
                mediator: mediator,
                team: team,
                name: mediator.PlayerName,
                owner: mediator.IsLocalPlayer
            );

            if (mediator.IsLocalPlayer)
            {
                LocalPlayer = newPlayerData;
                Debug.Log($"Tento objektivny indexovy muz je lokalny hrac, pozor.");
            }

            PlayerData[id] = newPlayerData;
            mediator.playerData = newPlayerData;
            team.RegisterPlayer(newPlayerData);
        }

        TeamSelector.Instance.DestroyAreas();

        var teamMate = LocalPlayer.Team.GetTeamMate(LocalPlayer);
        allyCamera.InitialSetTarget(teamMate.Mediator);
        teamMate.Mediator.PlayerVision.GetEnabled(true);
        LocalPlayer.Mediator.AbilityManager.AssignTeamMate(teamMate.Mediator);

        GameStateManager.Instance.StartNewGame(this);
    }
    /// <summary>
    /// Get an array of all local player's enemies.
    /// </summary>
    public PlayerData[] Enemies => LocalPlayer.Team.GetTheEnemyTeamData().Players;

    public PlayerData[] EnemiesOf(CharacterMediator mediator) => mediator.playerData.Team.GetTheEnemyTeamData().Players;

    /// <summary>
    /// Get an array of the local player, as well as all their team mates.
    /// </summary>
    public PlayerData[] AlliedUnits => LocalPlayer.Team.Players;
}