using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using static DataKeyInt;
using static DataKeyString;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private TMP_Text career, lastMatchResults, fullScreenText;
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private Button startButton, quitButton, fullScreenButton;

    private void Awake()
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
            lastMatchResults.text = string.Empty;
        }

        playerNameInput.text = storage.GetString(Name);

        startButton.onClick.AddListener(() =>
        {
            SceneManager.FindAMatch();
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

    private void OnDestroy()
    {
        DataStorage.Instance.SetString(Name, playerNameInput.text);
    }
}
