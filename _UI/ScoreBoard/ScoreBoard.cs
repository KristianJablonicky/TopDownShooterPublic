using System.Text;
using TMPro;
using UnityEngine;
using static DataKeyInt;
public class ScoreBoard : SingletonMonoBehaviour<ScoreBoard>
{
    [SerializeField] private PlayerEntryUI[] playerEntries;
    [SerializeField] private TMP_Text[] binders;
    [SerializeField] private CanvasGroup canvasGroup;

    private CharacterManager manager;
    private GameStateManager gameState;

    public void ChangeState(bool visible)
    {
        if (visible) Show();
        else Hide();
    }

    public void Show()
    {
        if (!gameState.GameInProgress) return;

        gameObject.SetActive(true);
        canvasGroup.alpha = 1f;
        UpdateTexts();
    }
    public void Hide()
    {
        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }
    private void Start()
    {
        manager = CharacterManager.Instance;
        gameState = GameStateManager.Instance;
        Hide();
    }

    private void UpdateTexts()
    {
        for (int teamNumber = 0; teamNumber < 2; teamNumber++)
        {
            var team = manager.Teams[(Team)teamNumber];
            binders[teamNumber].text = team.Wins.ToString();

            for (int playerNumber = 0; playerNumber < team.Players.Length; playerNumber++)
            {
                var player = team.Players[playerNumber];
                var entry = playerEntries[playerNumber + teamNumber * 2];

                entry.SetPlayerData(
                    player.Name,
                    player.PlayerScore.Kills,
                    player.PlayerScore.Deaths
                );
                entry.Highlight(player.Mediator.IsLocalPlayer);
            }
        }
    }

    private void OnDestroy()
    {
        if (!gameState.GameInProgress) return;

        DataStorage.Instance.lastScoreBoardState = SaveAsString();
        StorePlayerStats();
    }

    public string SaveAsString()
    {

        var sb = new StringBuilder();
        for (int teamNumber = 0; teamNumber < 2; teamNumber++)
        {
            var team = manager.Teams[(Team)teamNumber];
            sb.AppendLine($"Team {(Team)teamNumber} - Wins: {team.Wins}");
            for (int playerNumber = 0; playerNumber < team.Players.Length; playerNumber++)
            {
                var player = team.Players[playerNumber];
                sb.AppendLine($"{player.Name} - Kills: {player.PlayerScore.Kills}, Deaths: {player.PlayerScore.Deaths}");
            }
            sb.AppendLine();
        }
        return sb.ToString();
    }

    private void StorePlayerStats()
    {
        var storage = DataStorage.Instance;
        var localPlayer = manager.LocalPlayer;
        
        storage.Increment(Wins, localPlayer.Team.Wins);
        storage.Increment(Losses, localPlayer.Team.GetTheEnemyTeamData().Wins);
        storage.Increment(Kills, localPlayer.PlayerScore.Kills);
        storage.Increment(Deaths, localPlayer.PlayerScore.Deaths);
    }
}
