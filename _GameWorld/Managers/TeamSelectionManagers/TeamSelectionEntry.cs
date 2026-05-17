using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeamSelectionEntry : CanvasFaderBase
{
    [Header("References")]
    [SerializeField] private Image heroIcon;
    [SerializeField] private TMP_Text playerId, playerName;
    [SerializeField] private Button playWithBotsButton, setPreferenceButton;
    [SerializeField] private Image playWithBotsImage;
    [SerializeField] private TMP_Text teamMatePreference;
    [SerializeField] private GameObject teamMatePreferenceContainer;
    [SerializeField] private Graphic backgroundGraphic;
    [SerializeField] private Color highlightColor = Color.white;
    [SerializeField] private CanvasGroup playWithBotsCanvasGroup;
    [SerializeField] private Sprite checkIcon, crossIcon;

    [Header("Settings")]
    [SerializeField] private float colorMultiplier = 2f;

    private ulong ownerId;
    private CharacterMediator owner;

    private const string indifferentString = "-";
    private static Action preferenceStated;
    private void Awake()
    {
        canvasGroup.alpha = 0f;
        preferenceStated = null;
    }
    public void SetUp(CharacterMediator mediator)
    {
        owner = mediator;
        ownerId = owner.PlayerId;
        heroIcon.sprite = mediator.Toolkit.CharacterVisuals.Letter;

        var textsColor = GetColor(mediator);
        heroIcon.color = textsColor;
        playerId.color = textsColor;

        playerId.text = mediator.PlayerId.ToString();
        playerName.text = mediator.PlayerName;
        if (mediator.IsBot)
        {
            playerName.color = CommonColors.Instance.BotNameColor;
        }
        mediator.NetworkInput.PlayerName.OnValueChanged +=
            (_, newName) => playerName.text = newName.ToString();

        if (!mediator.IsLocalPlayer)
        {
            playWithBotsButton.interactable = false;
            playWithBotsCanvasGroup.alpha = 0.25f;
        }
        else
        {
            backgroundGraphic.color = highlightColor;
            playWithBotsButton.onClick.AddListener(VoteForBots);
        }
        teamMatePreferenceContainer.SetActive(false);

        preferenceStated += CleanUp;
        TweenState(true);
    }
    public void VotedForBots()
    {
        playWithBotsImage.sprite = checkIcon;
    }

    public void SetPreference(ulong preference)
    {
        if (preference == ownerId)
        {
            teamMatePreference.text = indifferentString;
        }
        else
        {
            teamMatePreference.text = preference.ToString();
            teamMatePreference.color = GetColor(CharacterManager.Instance.Mediators[preference]);
        }
    }

    private void VoteForBots()
    {
        playWithBotsButton.interactable = false;
        playWithBotsButton.onClick.RemoveAllListeners();
        owner.NetworkInput.VoteForBots();
    }

    public void StartTeamMateSelectionPhase()
    {
        playWithBotsButton.gameObject.SetActive(false);
        teamMatePreferenceContainer.SetActive(true);
        teamMatePreference.text = owner.IsPlayer ? string.Empty : indifferentString;
        setPreferenceButton.onClick.AddListener(MakePreference);
    }

    private void MakePreference()
    {
        CharacterManager.Instance.LocalPlayerMediator.NetworkInput.SelectTeamMate(ownerId);
        preferenceStated?.Invoke();
    }
    public void FadeOut()
    {
        TweenState(false, () => Destroy(gameObject));
    }
    private void CleanUp()
    {
        preferenceStated -= CleanUp;
        if (setPreferenceButton.gameObject != null)
        {
            Destroy(setPreferenceButton.gameObject);
        }
    }

    private Color GetColor(CharacterMediator mediator)
        => mediator.Toolkit.CharacterVisuals.PrimaryColor * colorMultiplier;
}
