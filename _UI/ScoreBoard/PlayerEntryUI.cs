using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerEntryUI : MonoBehaviour
{
    [SerializeField] private TMP_Text playerName;
    [SerializeField] private ObservableVariableBinder playerKills, playerDeaths, playerDamage;
    [SerializeField] private GameObject highlight;
    [SerializeField] private Image heroLetter;
    [field: SerializeField] public RectTransform RectTransform { get; private set; }
    public PlayerData Player { get; private set; }
    public Action<PlayerEntryUI> Promoted, Demoted;
    
    public void Init(PlayerEntryData data)
    {
        Highlight(data.LocalPlayer);
        playerName.text = data.Name;
        if (data.IsBot)
        {
            playerName.color = CommonColors.Instance.BotNameColor;
        }

        playerKills.SetText(data.Kills.ToString());
        playerDeaths.SetText(data.Deaths.ToString());
        playerDamage.SetText(data.DamageDealt.ToString());
        SetUpHeroBasedVisuals(
            HeroDatabaseSingleton.GetHeroToolkit(data.Hero).CharacterVisuals
        );
    }
    public void BindData(PlayerData player)
    {
        Player = player;
        playerName.text = player.Name;
        if (player.Mediator.IsBot)
        {
            playerName.color = CommonColors.Instance.BotNameColor;
        }

        playerKills.Bind(player.PlayerScore.Kills, true);
        playerDeaths.Bind(player.PlayerScore.Deaths, true);
        playerDamage.Bind(player.PlayerScore.DamageDealt, true);


        player.PlayerScore.Kills.OnValueSet += (_) =>
        {
            var pKills = player.PlayerScore.Kills;
            var aKills = player.GetTeamMate().PlayerScore.Kills;
            if (pKills.Get() == aKills.Get()) return;

            if (pKills > aKills) Promoted?.Invoke(this);
            else Demoted?.Invoke(this);
        };

        SetUpHeroBasedVisuals(player.Mediator.Toolkit.CharacterVisuals);
    }
    private void SetUpHeroBasedVisuals(CharacterVisualToolkit toolkit)
    {
        heroLetter.sprite = toolkit.Letter;
        heroLetter.color = toolkit.PrimaryColor * 2f;
    }
    public void Highlight(bool enable)
    {
        highlight.SetActive(enable);
    }
}

public class PlayerEntryData
{
    public PlayerEntryData(PlayerData player)
    {
        LocalPlayer = player.Owner;
        Hero = player.Mediator.Toolkit.DatabaseEntry;
        Name = player.Name;
        Kills = player.PlayerScore.Kills.Get();
        Deaths = player.PlayerScore.Deaths.Get();
        DamageDealt = player.PlayerScore.DamageDealt.Get();
        IsBot = player.Mediator.IsBot;
    }
    public bool LocalPlayer { get; private set; }
    public HeroDatabase Hero { get; private set; }
    public string Name { get; private set; }
    public int Kills { get; private set; }
    public int Deaths { get; private set; }
    public int DamageDealt { get; private set; }
    public bool IsBot { get; private set; }
}