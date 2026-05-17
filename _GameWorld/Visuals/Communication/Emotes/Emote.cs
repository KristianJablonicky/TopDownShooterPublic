using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Emote : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image emoteImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TMP_Text emoteText;
    [Header("Animation settings")]
    [SerializeField] private float duration = 2f;
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private float maxPositionBonus = 10f;

    private CharacterVisualToolkit visualToolkit;
    private Vector2 basePosition;
    private Color teamColor;
    public void Init(CharacterMediator owner)
    {
        owner.NetworkInput.Emoted += OnEmotePublic;
        owner.NetworkInput.EmotedPrivately += OnEmotePrivate;
        visualToolkit = owner.Toolkit.CharacterVisuals;
        basePosition = transform.localPosition;

        GameStateManager.Instance.GameStarted += () => OnGameStart(owner);
    }

    private void OnGameStart(CharacterMediator owner)
    {
        teamColor = CommonColors.GetTeamColorLight(owner.playerData.Team.Name);
    }

    private void OnEmotePublic(EmoteType type)
    {
        backgroundImage.color = Color.white;
        
        OnEmote(type);
    }
    private void OnEmotePrivate(EmoteType type)
    {
        backgroundImage.color = teamColor;
        OnEmote(type);
    }
    private void OnEmote(EmoteType type)
    {
        emoteImage.sprite = visualToolkit.GetEmote(type);
        emoteText.text = visualToolkit.GetEmoteText(type);
        Tween(0f, 1f, 0f);
        Tween(1f, 0f, duration + animationDuration);
    }

    private void Tween(float start, float end, float startDelay)
    {
        Tweener.Tween(this, start, end, animationDuration, TweenStyle.quadraticEaseOut,
            value =>
            {
                transform.localPosition = basePosition + Vector2.up * (value * maxPositionBonus);
                transform.localScale = Vector3.one * value;
            }, initialDelay: startDelay);
    }

}

public enum EmoteType
{
    positive,
    negative
}
