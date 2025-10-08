using TMPro;
using UnityEngine;

public class HereSelectionButton : MonoBehaviour
{
    [SerializeField] private CharacterToolkit toolkit;
    [SerializeField] private AbilityUI[] abilityUIButtons;

    [SerializeField] private TMP_Text heroName, heroDescription, abilityDescription;

    private void Start()
    {
        var pickedHero = DataStorage.Instance.GetInt(DataKeyInt.PickedHero);
        if ((int)toolkit.DatabaseEntry == pickedHero)
        {
            UpdateUI();
        }
    }
    public void OnClick()
    {
        DataStorage.Instance.SetInt(DataKeyInt.PickedHero, (int)toolkit.DatabaseEntry);
        UpdateUI();
    }

    private void UpdateUI()
    {
        for (int i = 0; i < abilityUIButtons.Length; i++)
        {
            abilityUIButtons[i].Init(toolkit.GetAbility((AbilityType)i), false);
        }
        heroName.text = toolkit.HeroName;
        heroDescription.text = toolkit.HeroDescription;
        abilityDescription.text = "";
    }
}
