using System.Linq;
using UnityEngine;

public class ScoreBoardTeamSection : MonoBehaviour
{
    [SerializeField] private Team team;
    [SerializeField] private PlayerEntryUI[] playerEntries;
    [SerializeField] private GameObject[] entrySpots;

    [SerializeField] private ObservableVariableBinder teamWins;

    [SerializeField] private RectTransform teamBackground;

    public PlayerEntryUI GetPlayerEntry(int index) => playerEntries[index];

    public void Init()
    {
        var teamData = CharacterManager.Instance.Teams[team];
        teamWins.Bind(teamData.Wins, true);

        for (int playerNumber = 0; playerNumber < teamData.Players.Length; playerNumber++)
        {
            var player = teamData.Players[playerNumber];
            var entry = playerEntries[playerNumber];

            entry.BindData(player);
            entry.Highlight(player.Mediator.IsLocalPlayer);

            entry.Promoted += PromoteEntry;
            entry.Demoted += DemoteEntry; 
        }
    }

    public void Init(PlayerEntryData player1, PlayerEntryData player2)
    {
        playerEntries[0].Init(player1);
        playerEntries[1].Init(player2);
        if (RoundHistory.LastMatchRoundResults is null) return; // should not happen.

        var wins = RoundHistory.LastMatchRoundResults
                .Where(r => r.Team == (int)team)
                .Count();

        if (wins > RoundHistory.LastMatchRoundResults.Count / 2)
        {
            teamWins.SetText($"{wins} (won)");
            teamBackground.anchorMax = new Vector2(1f, 1f);
        }
        else
        {
            teamWins.SetText(wins.ToString());
        }
    }

    private void PromoteEntry(PlayerEntryUI entry) => PutEntryAsFirst(entry, true);
    private void DemoteEntry(PlayerEntryUI entry) => PutEntryAsFirst(entry, false);
    private void PutEntryAsFirst(PlayerEntryUI entry, bool promote)
    {
        var isFirstEntry = entry == playerEntries[0];

        if ((isFirstEntry && promote)
            || (!isFirstEntry && !promote))
        {
            SetEntryToSpot(0, 0);
            SetEntryToSpot(1, 1);
        }
        else
        {
            SetEntryToSpot(1, 0);
            SetEntryToSpot(0, 1);
        }
    }

    private void SetEntryToSpot(int entryIndex, int spotIndex)
    {
        var playerEntry = playerEntries[entryIndex];
        playerEntries[entryIndex].transform.SetParent
        (
            entrySpots[spotIndex].transform
        );

        var RT = playerEntry.RectTransform;
        RT.offsetMin = new(RT.offsetMin.x, 0f);
        RT.offsetMax = new(RT.offsetMax.x, 0f);
    }
}
