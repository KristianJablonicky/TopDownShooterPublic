public class TrainingScoreManager : IResettable
{
    private const int ScorePerTakeDown = 1;

    public ObservableValue<int> Score { get; private set; }

    public TrainingScoreManager(ObservableVariableBinder binder)
    {
        Score = new(0);
        binder.Bind(Score, true);
    }

    public void AddScoreForTakeDown()
    {
        AddScore(ScorePerTakeDown);
    }
    public void AddScore(int scoreToAdd)
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
