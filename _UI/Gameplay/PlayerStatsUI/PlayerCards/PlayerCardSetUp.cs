using UnityEngine;

public class PlayerCardSetUp : MonoBehaviour
{
    [SerializeField] private PlayerCardUI playerCard, teamMateCard;
    [SerializeField] private PlayerCardUI[] enemyCards;
    [SerializeField] private CardsBackgroundFadeManager fadeManager;

    [Header("Fade In")]
    [SerializeField] private CanvasGroup[] cardsCanvasGroups;
    [SerializeField] private float fadeInDuration = 0.5f;
    private void Start()
    {
        PlayerNetworkInput.PlayerSpawned += OnPlayerSpawn;
        GameStateManager.Instance.GameStarted += OnGameStarted;
    }
    private void OnPlayerSpawn(CharacterMediator mediator)
    {
        gameObject.SetActive(true);
        playerCard.Init(mediator);
        playerCard.RotateForFun();
    }
    private void OnGameStarted()
    {
        fadeManager.EnableCardsAndSwitchFade(fadeInDuration);

        var localPlayer = CharacterManager.Instance.LocalPlayer;
        teamMateCard.Init(localPlayer.GetTeamMate().Mediator);

        var enemies = localPlayer.Team.EnemyTeamData.Players;
        for (int i = 0; i < enemies.Length; i++)
        {
            enemyCards[i].Init(enemies[i].Mediator);
        }
        PlayIntroAnimation();
    }

    private void PlayIntroAnimation()
    {
        teamMateCard.RotateForFun();
        foreach (var enemyCard in enemyCards)
        {
            enemyCard.RotateForFun();
        }

    }
}
