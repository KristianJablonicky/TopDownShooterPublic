using UnityEngine;

public class HeroCardToolkitUpdater : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CardSpawner cardManager;
    [SerializeField] private CardSetUp[] abilityUIButtons;
    [SerializeField] private CardSetUp gunCardButton;
    [SerializeField] private GameObject[] cardsGO;

    [Header("Settings")]
    [SerializeField] private float maxAnimationStaggerMult = 0.5f;

    private void Start()
    {
        cardManager.NewCardSelected += UpdateToolkits;
        cardManager.CardShuffleStarted += OnCardShuffle;
    }

    private void UpdateToolkits(CharacterToolkit toolkit)
    {
        for (int i = 0; i < abilityUIButtons.Length; i++)
        {
            abilityUIButtons[i].Init(toolkit, toolkit.GetAbility((AbilityType)i));
        }
        gunCardButton.Init(toolkit, toolkit.GunConfig);
    }

    private void OnCardShuffle(float duration)
    {
        var delay = duration * maxAnimationStaggerMult / cardsGO.Length;
        for (int i = 0; i < cardsGO.Length; i++)
        {
            var card = cardsGO[i];
            var currentDelay = delay * (i + 1);
            Tweener.Tween(card, 1f, 0f, duration * (1f - maxAnimationStaggerMult), TweenStyle.sinusPingPong,
                value => card.transform.localScale = new(value, 1f),
                initialDelay: currentDelay);
        }
    }
}
