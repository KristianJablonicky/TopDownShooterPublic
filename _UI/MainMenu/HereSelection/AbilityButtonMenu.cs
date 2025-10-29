using TMPro;
using UnityEngine;

public class AbilityButtonMenu : MonoBehaviour
{
    [SerializeField] private TMP_Text abilityDescription;
    [SerializeField] private AbilityUI ability;
    [SerializeField] private GunDescriptionUI gun;
    public void OnClick()
    {
        if (ability != null)
        {
            abilityDescription.text = ability.Text;
        }
        else if (gun != null)
        {
            abilityDescription.text = gun.Text;
        }
    }
}
