using TMPro;
using UnityEngine;

public class AbilityCardSetUp : CardSetUp
{

    [Header("References")]
    [SerializeField] private IconBehindText hotkey, coolDown;
    [SerializeField] private TMP_Text abilityDescription;

    protected override string SpecificInit(CharacterToolkit toolkit, ScriptableObjectBase SOBase)
    {
        if (SOBase is Ability ability)
        {
            abilityDescription.text = ability.Description;

            if (ability is ActiveAbility activeAbility)
            {
                if (activeAbility.KeyCode == AbilityHotKeys.Movement)
                {
                    hotkey.SetText(",_,");
                }
                else
                {
                    hotkey.SetText(((KeyCode)activeAbility.KeyCode).ToString());
                }

                coolDown.SetText(activeAbility.CoolDown.ToString());
            }
            else
            {
                hotkey.Hide();
                coolDown.Hide();
            }

            return ability.GetLongDescription();
        }

        var errorMessage = $"{this} received ScriptableObjectBase not of type Ability!";
        Debug.LogError(errorMessage);
        return errorMessage;
    }
}
public enum AbilityType
{
    Movement,
    Utility,
    Passive,
    PostMortem
}