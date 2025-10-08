using System.Collections.Generic;
using System.Diagnostics;

public class TeamData : IResettable
{
    public PlayerData[] Players { get; private set; }
    private Dictionary<PlayerData, PlayerData> teamMateDict;
    public Team Name { get; private set; }
    public ObservableValue<int> Wins { get; private set; } = new(0);
    public void Reset()
    {
        Wins = new(0);
    }
    public void AddWin() => Wins.Adjust(1);
    public TeamData(Team team)
    {
        Name = team;
        Players = new PlayerData[Constants.maxPlayerCount / 2];
    }
    public void RegisterPlayer(PlayerData player)
    {
        UnityEngine.Debug.Log($"registering a player ({player.Mediator.PlayerId})");
        if (Players[0] is null)
        {
            UnityEngine.Debug.Log("at index 0");
            Players[0] = player;
        }
        else
        {
            UnityEngine.Debug.Log("at index 1, building the dictionary");
            Players[1] = player;
            teamMateDict = new()
            {
                { Players[0], Players[1] },
                { Players[1], Players[0] }
            };
        }
    }

    public PlayerData GetTeamMate(PlayerData player)
    {
        if (Players.Length == 1)
        {
            return player;
        }
        return teamMateDict[player];
    }

    public Team GetTheEnemyTeam() => Name == Team.Orange ? Team.Cyan : Team.Orange;
    public TeamData GetTheEnemyTeamData() => CharacterManager.Instance.Teams[GetTheEnemyTeam()];
}