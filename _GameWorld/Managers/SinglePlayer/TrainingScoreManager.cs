using UnityEngine.SceneManagement;

public class TrainingScoreManager : IResettable
{
    private const float ScorePerTakeDown = 1f;

    public ObservableValue<float> Score { get; private set; }

    public TrainingScoreManager(ObservableVariableBinder binder)
    {
        Score = new(0);
        binder.Bind(Score, true);
    }

    public void AddScoreForTakeDown()
    {
        AddScore(ScorePerTakeDown);
    }
    public void AddScore(float scoreToAdd)
    {
        Score.Adjust(scoreToAdd);
    }

    public void Reset()
    {
        Score.Set(0);
    }

    public bool UpdateAndCheckIsHighScore()
    {
        string suffix;
        bool isHighScore = false;
        var highScore = DataStorage.Instance.GetIntHeroSpecific(
            DataKeyInt.HighScore, null
        );

        if (Score > highScore)
        {
            suffix = "New High Score!";
            DataStorage.Instance.SetIntHeroSpecific(
                DataKeyInt.HighScore, null, (int)Score
            );
            isHighScore = true;
        }
        else
        {
            suffix = $"High Score: {highScore}";
        }

        GameStateNotifications.Instance.ShowMessage($"Score: {Score}\n{suffix}");
        return isHighScore;
    }
}
