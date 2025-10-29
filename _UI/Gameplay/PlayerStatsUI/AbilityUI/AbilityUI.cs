using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityUI : MonoBehaviour
{

    [Header("References")]
    [SerializeField] private Image abilityIcon;
    [SerializeField] private TMP_Text hotkeyText;
    [SerializeField] private GameObject hotkey;
    public string Text {  get; private set; }
    public void Init(Ability ability, bool longDescription)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"<b>{ability.Name}</b>");

        sb.AppendLine(longDescription ? ability.GetLongDescription() : ability.GetDescription());

        Text = sb.ToString();
        abilityIcon.sprite = ability.Icon;

        if (ability is ActiveAbility activeAbility)
        {
            hotkeyText.text = ((KeyCode)activeAbility.KeyCode).ToString();
        }
        else
        {
            hotkey.SetActive(false);
        }
    }

    
}
public enum AbilityType
{
    Movement,
    Utility,
    Passive,
    PostMortem
}