using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using static DataKeyInt;
using static DataKeyString;

public class MainMenuManager : SingletonMonoBehaviour<MainMenuManager>
{
    [SerializeField] private TMP_Text career, lastMatchResults, fullScreenText;
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private Button findAMatchButton, trainingButton, altarDefenseButton, quitButton, fullScreenButton;

    [SerializeField] private PopUpWindowBase matchResultsWindow;
    protected override void OverriddenAwake()
    {
        var storage = DataStorage.Instance;

        career.text = GetCareerText(storage);

        if (storage.lastScoreBoardState != string.Empty && storage.lastScoreBoardState != null)
        {
            lastMatchResults.text = $"{storage.disconnectReason}\nLast Match Results:\n{storage.lastScoreBoardState}";
        }
        else if (storage.disconnectReason != null)
        {
            lastMatchResults.text = storage.disconnectReason;
        }
        else
        {
            UpdateHighScore();
        }

        playerNameInput.text = storage.GetString(Name);

        findAMatchButton.onClick.AddListener(() =>
        {
            SceneManager.StartGameplay(GameMode.MultiPlayer);
        });
        trainingButton.onClick.AddListener(() =>
        {
            SceneManager.StartGameplay(GameMode.Training);
        });

        altarDefenseButton.onClick.AddListener(() =>
        {
            SceneManager.StartGameplay(GameMode.AltarDefense);
        });

        quitButton.onClick.AddListener (() =>
        {
            Application.Quit();
        });

        if (ScoreBoard.playerEntries is not null)
        {
            matchResultsWindow.GetActivated();
        }
    }

    private string GetCareerText(DataStorage storage)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<b>Total stats:</b>");
        sb.AppendLine($"Round wins: {storage.GetInt(Wins)}");
        sb.AppendLine($"Round losses: {storage.GetInt(Losses)}");
        sb.AppendLine($"Kills: {storage.GetInt(Kills)}");
        sb.AppendLine($"Deaths: {storage.GetInt(Deaths)}");
        return sb.ToString();
    }

    public void UpdateHighScore()
    {
        var storage = DataStorage.Instance;
        var highScore = storage.GetIntHeroSpecific(HighScore, null, null);
        lastMatchResults.text = $"Training HighScore: {highScore}";
    }

    private void OnDestroy()
    {
        DataStorage.Instance.SetString(Name, playerNameInput.text);
    }
}
