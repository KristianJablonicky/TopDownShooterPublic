using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class AbilityUI : MonoBehaviour
{

    [Header("References")]
    [SerializeField] private Image abilityIcon;
    public string Text {  get; private set; }
    public void Init(Ability ability, bool longDescription)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"<b>{ability.Name}</b>");

        sb.AppendLine(longDescription ? ability.LongDescription : ability.Description);

        if (ability is ActiveAbility activeAbility)
        {
            sb.AppendLine($"Cooldown: {activeAbility.CoolDown}");
        }
        Text = sb.ToString();
        abilityIcon.sprite = ability.Icon;
    }

    
}
public enum AbilityType
{
    Movement,
    Utility,
    Passive,
    PostMortem
}