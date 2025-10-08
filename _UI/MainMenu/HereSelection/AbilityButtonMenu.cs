using TMPro;
using UnityEngine;

public class AbilityButtonMenu : MonoBehaviour
{
    [SerializeField] private TMP_Text abilityDescription;
    [SerializeField] private AbilityUI ability;
    public void OnClick()
    {
        abilityDescription.text = ability.Text;
    }
}
