using System;
using UnityEngine;

public class CardSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerCardBase cardPrefab;
    [SerializeField] private HeroToolkitDatabase heroToolkitDatabase;
    [SerializeField] private Transform rightPeak, leftPeak;
    [Header("Settings")]
    [SerializeField] private float shuffleAnimationDuration = 0.5f;
    private PlayerCardBase[] cards;
    private int currentHeroIndex = 0, cardsCount;
    public event Action<CharacterToolkit> NewCardSelected;
    public event Action<float> CardShuffleProgressed, CardShuffleStarted;

    private Transform bottomCard, topCard;

    private bool coroutineRunning = false;

    private void Awake()
    {
        cardsCount = heroToolkitDatabase.HeroToolkits.Length;
        cards = new PlayerCardBase[cardsCount];
        for (int i = 0; i < cardsCount; i++)
        {
            var heroToolkit = heroToolkitDatabase.HeroToolkits[i];
            var card = Instantiate(cardPrefab, transform);
            card.gameObject.name = $"{heroToolkit.HeroName} Card";
            card.Init(heroToolkit.CharacterVisuals);
            cards[i] = card;
        }

        currentHeroIndex = DataStorage.Instance.GetInt(DataKeyInt.PickedHero);
    }

    private void Start()
    {
        Invoker.Instance.ExecuteAfterOneFrame(() =>
        {
            UpdateCardOrder();
            NewCardSelected?.Invoke(
                heroToolkitDatabase.HeroToolkits[currentHeroIndex]
            );
        });
    }

    public void NextCard()
    {
        AnimateShuffle(true);
    }

    public void PreviousCard()
    {
        AnimateShuffle(false);
    }

    private void UpdateCardOrder()
    {
        var last = cardsCount - 1;

        for (int i = 0; i < cardsCount; i++)
        {
            var index = (last - i + currentHeroIndex) % cardsCount;
            if (index < 0)
            {
                index += cardsCount;
            }

            var transform = cards[i].transform;
            transform.SetSiblingIndex(index);

            if (index == 0)
            {
                bottomCard = transform;
            }
            if (index == last)
            {
                topCard = transform;
                cards[i].Selected();
            }
        }
    }

    private void AnimateShuffle(bool next)
    {
        if (coroutineRunning) return;

        var card = next ? topCard : bottomCard;
        var peak = next ? rightPeak : leftPeak;

        var change = next ? 1 : -1;
        currentHeroIndex = (currentHeroIndex + change) % cardsCount;
        if (currentHeroIndex < 0)
        {
            currentHeroIndex += cardsCount;
        }

        var startPos = card.transform.position;
        var startScale = card.transform.localScale;
        var startRot = card.transform.rotation;
        
        coroutineRunning = true;
        CardShuffleStarted?.Invoke(shuffleAnimationDuration);

        Tweener.Tween(this, 0f, 1f, shuffleAnimationDuration,
            TweenStyle.sinusPingPong,
            value =>
            {
                CardShuffleProgressed?.Invoke(value);
                card.Lerp(startPos, startScale, startRot, peak, value);
            },
            onExit: () => coroutineRunning = false
            );

        // halfway through the animation update the card order and invoke the event for the new card
        Invoker.Instance.ExecuteAfterDelay(shuffleAnimationDuration * 0.5f,
            () => {
                UpdateCardOrder();
                var toolkit = heroToolkitDatabase.HeroToolkits[currentHeroIndex];
                NewCardSelected?.Invoke(toolkit);
            });
    }

    private void OnDestroy()
    {
        DataStorage.Instance.SetInt(DataKeyInt.PickedHero, currentHeroIndex);
    }
}
