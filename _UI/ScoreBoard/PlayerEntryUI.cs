using System;
using TMPro;
using UnityEngine;

public class PlayerEntryUI : MonoBehaviour
{

    [SerializeField] private TMP_Text playerName;
    [SerializeField] private ObservableVariableBinder playerKills, playerDeaths;
    [SerializeField] private GameObject highlight;
    [field: SerializeField] public RectTransform RectTransform { get; private set; }

    public Action<PlayerEntryUI> Promoted, Demoted;
    public void BindData(PlayerData player)
    {
        playerName.text = player.Name;

        playerKills.Bind(player.PlayerScore.Kills, true);
        playerDeaths.Bind(player.PlayerScore.Deaths, true);

        player.PlayerScore.Kills.OnValueSet += (_) =>
        {
            var pKills = player.PlayerScore.Kills;
            var aKills = player.GetTeamMate().PlayerScore.Kills;
            if (pKills.Get() == aKills.Get()) return;

            if (pKills > aKills) Promoted?.Invoke(this);
            else Demoted?.Invoke(this);
        };
    }

    public void Highlight(bool enable)
    {
        highlight.SetActive(enable);
    }
}
