using System.Collections.Generic;
using UnityEngine;

public class RoundHistory : MonoBehaviour
{
    [SerializeField] private Sprite[] teamBackgrounds;
    [SerializeField] private Sprite[] roles;
    [SerializeField] private RoundHistoryEntry entryPrefab;

    private RoundHistoryEntry[] rounds;
    public static List<RoundResults> LastMatchRoundResults { get; private set; }

    public void Init()
    {
        InstantiateRoundEntries(Constants.roundsToWinMatch * 2 - 1);
        LastMatchRoundResults = new();
        GameStateManager.Instance.RoundNumberWonByTeam += OnRoundEnd;
    }

    public void InitGameEnd()
    {
        if (LastMatchRoundResults is null) return;

        InstantiateRoundEntries(LastMatchRoundResults.Count);
        for (int i = 0; i < LastMatchRoundResults.Count; i++)
        {
            rounds[i].SetUp
            (
                teamBackgrounds[LastMatchRoundResults[i].Team],
                roles[LastMatchRoundResults[i].Role]
            );
        }
        LastMatchRoundResults = null;
    }

    private void InstantiateRoundEntries(int roundsCount)
    {
        rounds = new RoundHistoryEntry[roundsCount];
        for (int i = 0; i < roundsCount; i++)
        {
            rounds[i] = Instantiate(entryPrefab, transform);
        }
    }

    private void OnRoundEnd(int roundNumber, TeamData winningTeam)
    {
        int name = (int)winningTeam.Name,
            role = (int)winningTeam.CurrentRole;
        rounds[roundNumber - 1].SetUp
        (
            teamBackgrounds[name],
            roles[role]
        );
        LastMatchRoundResults.Add(new(name, role));
    }

    public class RoundResults
    {
        public RoundResults(int team, int role)
        {
            Team = team;
            Role = role;
        }
        public int Team { get; private set; }
        public int Role { get; private set; }
    }
}
