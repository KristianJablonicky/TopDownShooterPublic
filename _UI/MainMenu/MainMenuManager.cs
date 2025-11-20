using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using static DataKeyInt;
using static DataKeyString;

public class MainMenuManager : SingletonMonoBehaviour<MainMenuManager>
{
    [SerializeField] private TMP_Text career, lastMatchResults, fullScreenText, version;
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private Button findAMatchButton, trainingButton, quitButton, fullScreenButton;

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
            SceneManager.StartGameplay(GameMode.SinglePlayer);
        });
        quitButton.onClick.AddListener (() =>
        {
            Application.Quit();
        });
        fullScreenButton.onClick.AddListener(() =>
        {
            Screen.fullScreen = !Screen.fullScreen;
            fullScreenText.text = Screen.fullScreen ? "Windowed" : "Full Screen";
        });


        fullScreenText.text = Screen.fullScreen ? "Windowed" : "Full Screen";

        version.text = $"Version: {Application.version}";
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
        var highScore = storage.GetIntHeroSpecific(HighScore, null);
        lastMatchResults.text = $"Training HighScore: {highScore}";
    }

    private void OnDestroy()
    {
        DataStorage.Instance.SetString(Name, playerNameInput.text);
    }
}
