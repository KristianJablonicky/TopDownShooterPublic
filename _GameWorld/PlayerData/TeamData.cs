using System.Collections.Generic;
public class TeamData : IResettable
{
    public PlayerData[] Players { get; private set; }
    private Dictionary<PlayerData, PlayerData> teamMateDict;
    public Team Name { get; private set; }
    public ObservableValue<int> Wins { get; private set; } = new(0);
    public int attackerWins, defenderWins;
    public void Reset()
    {
        Wins = new(0);
    }
    public void AddWin(Role role)
    {
        Wins.Adjust(1);
        if (role == Role.Attacker)
        {
            attackerWins++;
        }
        else
        {
            defenderWins++;
        }
    }

    public TeamData(Team team)
    {
        Name = team;
        Players = new PlayerData[Constants.maxPlayerCount / 2];
    }
    public void RegisterPlayer(PlayerData player)
    {
        if (Players[0] is null)
        {
            Players[0] = player;
        }
        else
        {
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
    public TeamData EnemyTeamData => CharacterManager.Instance.Teams[GetTheEnemyTeam()];
}