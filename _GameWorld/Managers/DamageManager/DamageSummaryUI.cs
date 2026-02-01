using System.Collections.Generic;
using UnityEngine;

public class DamageSummaryUI : MonoBehaviour
{

    [SerializeField] private PlayerCardUI[] playerCards;

    private Dictionary<ulong, PlayerCardUI> playerCardDict;

    private void Awake()
    {
        playerCardDict = new();
        foreach (var card in playerCards)
        {
            card.CardSetUp += OnCardSetUp;
        }
    }

    private void OnCardSetUp(ulong playerId, PlayerCardUI card)
    {
        playerCardDict.Add(playerId, card);
    }

    public void ShowSummary(Dictionary<CharacterMediator, DamageRecord> dictionary)
    {
        PlayerCardUI cardUI;
        foreach (var entry in dictionary)
        {
            cardUI = playerCardDict[entry.Key.PlayerId];
            cardUI.DamageSummaryEntryManager.ShowSummary(entry.Value);
        }
    }

    public void HideSummary(float initialDelay)
    {
        foreach (var card in playerCards)
        {
            card.DamageSummaryEntryManager.HideSummary(initialDelay);
        }
    }
}
