using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCardTextsUpdater : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CardSpawner cardManager;
    [SerializeField] private CanvasGroup canvasGroup;
    [Header("Hero Name")]
    [SerializeField] private GameObject The;
    [SerializeField] private TMP_Text heroName;
    [SerializeField] private JaggedOutline firstLetterBackground;
    [SerializeField] private Image heroLetter;
    [Header("Other texts")]
    [SerializeField] private TMP_Text flavorText;

    private void Awake()
    {
        cardManager.NewCardSelected += UpdateTexts;
        cardManager.CardShuffleProgressed += progress => canvasGroup.alpha = 1f - progress;
    }

    private void UpdateTexts(CharacterToolkit toolkit)
    {
        The.SetActive(toolkit.CharacterVisuals.The);
        heroName.text = toolkit.HeroName[1..];

        var color = toolkit.CharacterVisuals.PrimaryColor;
        color.a = 0.75f;
        firstLetterBackground.SetColor(color);
        
        heroLetter.sprite = toolkit.CharacterVisuals.Letter;

        flavorText.text = toolkit.HeroDescription;
    }
}
