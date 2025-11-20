using UnityEngine;

public class RoundHistory : MonoBehaviour
{
    [SerializeField] private Sprite[] teamBackgrounds;
    [SerializeField] private Sprite[] roles;
    [SerializeField] private RoundHistoryEntry entryPrefab;

    private RoundHistoryEntry[] rounds;

    public void Init()
    {
        rounds = new RoundHistoryEntry[Constants.roundsToWinMatch * 2 - 1];
        for (int i = 0; i < rounds.Length; i++)
        {
            rounds[i] = Instantiate(entryPrefab, transform);
        }
        GameStateManager.Instance.RoundNumberWonByTeam += OnRoundEnd;
    }

    private void OnRoundEnd(int roundNumber, TeamData winningTeam)
    {
        rounds[roundNumber - 1].SetUp
        (
            teamBackgrounds[(int)winningTeam.Name],
            roles[(int)winningTeam.CurrentRole]
        );
    }
}
