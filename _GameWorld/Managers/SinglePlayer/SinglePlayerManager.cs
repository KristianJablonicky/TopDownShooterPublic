using System.Collections;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.SinglePlayer;
using UnityEngine;

public class SinglePlayerManager : SingletonMonoBehaviour<SinglePlayerManager>
{
    [SerializeField] private SinglePlayerTransport singlePlayerTransport;
    [SerializeField] private PracticeManager practiceManager;

    [Header("UI references")]
    [SerializeField] private GameObject highScores;
    [SerializeField] private ObservableVariableBinder scoreBinder;
    [SerializeField] private TMP_Text highScore;

    [Header("Training Settings")]
    [SerializeField] private float delayBeforeStart = 3f;
    [SerializeField] private float trainingDuration = 60f;
    [SerializeField] private Vector2 startingPosition;

    private TrainingScoreManager scoreManager;
    protected override void OverriddenAwake()
    {
        if (DataStorage.Instance.GetGameMode() != GameMode.SinglePlayer)
        {
            //gameObject.SetActive(false);
            Destroy(gameObject);
        }
        else
        {
            StartCoroutine(StartHostAfterDelay());
        }
    }

    private IEnumerator StartHostAfterDelay()
    {
        yield return new WaitForEndOfFrame();
        NetworkManager.Singleton.NetworkConfig.NetworkTransport = singlePlayerTransport;
        NetworkHeroSpawner.StartHost();

        scoreManager = new(scoreBinder);
        practiceManager.Init(scoreManager);

        highScores.SetActive(true);
        UpdateHighScore();
    }

    public async void StartTraining()
    {
        StopTraining(true);

        
        var player = CharacterManager.Instance.LocalPlayerMediator;
        player.Reset();
        player.MovementController.SetPosition(startingPosition);
        player.Gun.ChannelingManager.StartChannelingStandingStill(delayBeforeStart, null, player, false);

        await TaskExtensions.Delay(delayBeforeStart);

        if (this == null) return;
        if (!isActiveAndEnabled) return;

        GameStateManager.Instance.StartTraining(trainingDuration, this);
        practiceManager.StartTraining();
    }

    public bool? StopTraining(bool interrupted)
    {
        practiceManager.DisableDummies();
        GameStateManager.Instance.StopTraining();

        if (interrupted)
        {
            scoreManager.Reset();
            return null;
        }
        else
        {
            var newHighScore = scoreManager.UpdateAndCheckIsHighScore();
            if (newHighScore) UpdateHighScore();
            return newHighScore;
        }
    }

    private void UpdateHighScore()
    {
        highScore.text = $"High Score: {DataStorage.Instance.GetIntHeroSpecific(DataKeyInt.HighScore, null)}";
    }
}
