using System;
using System.Text;
using UnityEngine;
using static DataKeyInt;

public class ScoreBoard : SingletonMonoBehaviour<ScoreBoard>
{
    //[SerializeField] private bool boardDuringGameplay;
    
    [SerializeField] private float fadeInDuration = 0.25f;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RoundHistory history;
    [SerializeField] private ScoreBoardTeamSection[] teamSections;

    [Header("Outside Gameplay")]
    [SerializeField] private PopUpWindowBase mainMenuPopUp;

    public static PlayerEntryData[] playerEntries;

    private CharacterManager manager;
    private GameStateManager gameState;
    private Coroutine fadeCoroutine;
    public event Action<bool> Shown;
    public void ChangeState(bool visible)
    {
        if (visible) Show();
        else Hide();

        Shown?.Invoke(visible);
    }

    public void Show()
    {
        if (!gameState.GameInProgress) return;
        gameObject.SetActive(true);
        Fade(1f, null);
    }
    public void Hide()
    {
        if (!gameState.GameInProgress) return;
        Fade(0f, () => gameObject.SetActive(false));
    }


    private void Fade(float targetAlpha, Action actionOnExit)
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(
            Tweener.TweenCoroutine(this, canvasGroup.alpha, targetAlpha, fadeInDuration, TweenStyle.quadraticEaseOut,
                value => canvasGroup.alpha = value, actionOnExit
            )
        );
    }
    protected override void OverriddenAwake()
    {
        if (mainMenuPopUp != null)
        {
            mainMenuPopUp.AfterStart += MainMenuMatchSummary;
        }
    }

    private void MainMenuMatchSummary()
    {
        if (playerEntries is null) return;

        gameObject.SetActive(true);
        mainMenuPopUp.GetActivated();
        teamSections[0].Init(playerEntries[0], playerEntries[1]);
        teamSections[1].Init(playerEntries[2], playerEntries[3]);
        history.InitGameEnd();
    }

    private void Start() // gameplay setup
    {
        if (mainMenuPopUp != null) return;
        manager = CharacterManager.Instance;
        gameState = GameStateManager.Instance;

        canvasGroup.alpha = 0f;
        gameState.GameStarted += InitialSetUp;
        gameObject.SetActive(false);
    }

    private void InitialSetUp()
    {
        history.Init();
        foreach (var section in teamSections) section.Init();
    }


    private void OnDestroy()
    {
        if (gameState == null) return;
        if (!gameState.GameInProgress) return;

        DataStorage.Instance.lastScoreBoardState = SaveAsString();
        StorePlayerStats();
    }

    public void StoreData()
    {
        playerEntries = new PlayerEntryData[manager.PlayerData.Length];
        playerEntries[0] = GetPlayerEntry(0, 0);
        playerEntries[1] = GetPlayerEntry(0, 1);
        playerEntries[2] = GetPlayerEntry(1, 0);
        playerEntries[3] = GetPlayerEntry(1, 1);
    }

    private PlayerEntryData GetPlayerEntry(int teamNumber, int playerNumber)
        => new(teamSections[teamNumber].GetPlayerEntry(playerNumber).Player);

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
        
        storage.Increment(Wins, (int)localPlayer.Team.Wins);
        storage.Increment(Losses, (int)localPlayer.Team.EnemyTeamData.Wins);
        storage.Increment(Kills, localPlayer.PlayerScore.Kills);
        storage.Increment(Deaths, localPlayer.PlayerScore.Deaths);
    }
}
