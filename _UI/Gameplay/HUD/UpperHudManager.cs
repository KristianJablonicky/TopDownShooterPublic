using UnityEngine;
using UnityEngine.UI;

public class UpperHudManager : MonoBehaviour
{
    [SerializeField] private ObservableVariableBinder timer, teamOrange, teamCyan;
    [SerializeField] private Image orangeImage, cyanImage;

    private void Start()
    {
        var manager = GameStateManager.Instance;
        manager.GameStarted += OnGameStart;
        manager.TrainingStarted += OnTrainingStart;

        orangeImage.color = Color.clear;
        cyanImage.color = Color.clear;
    }

    private void OnTrainingStart()
    {
        BindTimer();
    }

    private void OnGameStart()
    {
        BindTimer();
        var characterManager = CharacterManager.Instance;

        SetUpTeam(teamOrange, orangeImage, characterManager.Orange);
        SetUpTeam(teamCyan, cyanImage, characterManager.Cyan);
    }

    private void BindTimer()
    {
        timer.Bind(GameStateManager.Instance.AttackersTimer.TimeRemaining, false);
    }

    private void SetUpTeam(ObservableVariableBinder binder, Image image, TeamData team)
    {
        binder.Bind(team.Wins, true);
        image.color = CommonColors.GetTeamColor(team.Name);
        
    }
}
