using TMPro;
using UnityEngine;

public class PlayerEntryUI : MonoBehaviour
{
    [SerializeField] private TMP_Text playerName, playerKills, playerDeaths;
    [SerializeField] private GameObject highlight;
    public void SetPlayerData(string name, int kills, int deaths)
    {
        playerName.text = name;
        playerKills.text = kills.ToString();
        playerDeaths.text = deaths.ToString();
    }

    public void Highlight(bool enable)
    {
        highlight.SetActive(enable);
    }
}
