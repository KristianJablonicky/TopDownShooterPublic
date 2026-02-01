using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CharacterManager : SingletonMonoBehaviour<CharacterManager>
{
    [SerializeField] private CameraController playerCamera, allyCamera;
    public Dictionary<ulong, CharacterMediator> Mediators { get; private set; }
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
        Mediators = new();
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
        if (Mediators[disconnectedId].playerData is null)
        {
            disconnectString = "A player has disconnected before the match started.";
        }
        else
        {
            disconnectString = $"{Mediators[disconnectedId].playerData.Name} has disconnected";
        }
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
        Mediators.Add(uID, character);

        CharacterRegistered?.Invoke(character);

        if (character.IsLocalPlayer && !character.IsNPC)
        {
            LocalPlayerMediator = character;
            playerCamera.InitialSetTarget(character, false);
            
            if (DataStorage.Instance.GetGameMode() == GameMode.MultiPlayer)
            {
#if UNITY_EDITOR
                var connectedPlayers = NetworkManager.Singleton.ConnectedClientsList.Count;
                var x = 4f;
                var direction = connectedPlayers % 2 == 0 ? -1f : 1f;
                var y = connectedPlayers < 3 ? -1f : 1f;
                var spawnPos = new Vector2(x * direction, y * 5f - 75f);

#else
                var spawnPos = RespawnManager.Instance.GetSpawnPoint(uID);
#endif
                character.MovementController.SetPosition(spawnPos);
            }
            else
            {
                character.MovementController.SetPosition(
                    RespawnManager.Instance.GetSpawnPointTraining());
            }
        }

        if (DataStorage.Instance.GetGameMode() == GameMode.MultiPlayer &&
            Mediators.Count == Constants.maxPlayerCount)
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
        for (int i = 0; i < playerIDs.Length; i++)
        {
            var id = playerIDs[i];
            var mediator = Mediators[id];

            // IDs are sorted by TeamSelector (orange1, orange2, cyan1, cyan2)
            var teamEnum = i < Constants.maxPlayerCount / 2 ? Team.Orange : Team.Cyan;
            var team = Teams[teamEnum];

            var newPlayerData = new PlayerData(
                mediator: mediator,
                team: team,
                name: mediator.PlayerName,
                owner: mediator.IsLocalPlayer
            );

            if (mediator.IsLocalPlayer)
            {
                LocalPlayer = newPlayerData;
                team.LocalPlayersTeam = true;
            }

            PlayerData[id] = newPlayerData;
            mediator.playerData = newPlayerData;
            team.RegisterPlayer(newPlayerData);
        }

        TeamSelector.Instance.DestroyAreas();

        var teamMate = LocalPlayer.GetTeamMate();
        allyCamera.InitialSetTarget(teamMate.Mediator, true);
        teamMate.Mediator.PlayerVision.GetEnabled(true);

        LocalPlayer.Mediator.AbilityManager.AssignTeamMate(teamMate.Mediator);
        LocalPlayer.Mediator.InputHandler.SetTeamMateCamera(allyCamera.Camera);

        GameStateManager.Instance.StartNewGame(this);
    }
    /// <summary>
    /// Get an array of all local player's enemies.
    /// </summary>
    public PlayerData[] Enemies => LocalPlayer.Team.EnemyTeamData.Players;

    public PlayerData[] EnemiesOf(CharacterMediator mediator) => mediator.playerData.Team.EnemyTeamData.Players;

    /// <summary>
    /// Get an array of the local player, as well as all their team mates.
    /// </summary>
    public PlayerData[] AlliedUnits => LocalPlayer.Team.Players;

    public PlayerData[] GetAlliedUnitsOf(CharacterMediator mediator) => mediator.playerData.Team.Players;


    public void UnregisterCharacter(CharacterMediator mediator)
    {
        Mediators.Remove(mediator.PlayerId);
    }
}