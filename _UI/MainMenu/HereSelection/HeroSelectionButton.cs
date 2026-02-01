using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroSelectionButton : MonoBehaviour
{
    [SerializeField] private CharacterToolkit toolkit;
    [SerializeField] private AbilityUI[] abilityUIButtons;
    [SerializeField] private GunDescriptionUI gunDescription;

    [SerializeField] private TMP_Text heroName, heroDescription, abilityDescription;
    [SerializeField] private Image BackgroundHighlight;

    private static Action<HeroSelectionButton> OnHeroSelected;

    private void Start()
    {
        BackgroundHighlight.color = toolkit.PrimaryColor;
        
        var pickedHero = DataStorage.Instance.GetInt(DataKeyInt.PickedHero);
        if ((int)toolkit.DatabaseEntry == pickedHero)
        {
            UpdateUI(false);
        }
        else
        {
            SetBackgroundVisibility(false, true);
        }


        OnHeroSelected += selectedButton =>
        {
            if (selectedButton != this)
            {
                SetBackgroundVisibility(false, false);
            }
        };
    }
    public void OnClick()
    {
        DataStorage.Instance.SetInt(DataKeyInt.PickedHero, (int)toolkit.DatabaseEntry);
        OnHeroSelected?.Invoke(this);
        UpdateUI(true);
    }

    private void UpdateUI(bool clicked)
    {
        for (int i = 0; i < abilityUIButtons.Length; i++)
        {
            abilityUIButtons[i].Init(toolkit.GetAbility((AbilityType)i), true);
        }
        gunDescription.Init(toolkit);
        heroName.text = toolkit.HeroName;
        heroDescription.text = toolkit.HeroDescription;
        abilityDescription.text = "";

        SetBackgroundVisibility(true, false);

        if (clicked)
        {
            MainMenuManager.Instance.UpdateHighScore();
        }
    }

    private void SetBackgroundVisibility(bool visible, bool instant)
    {
        var targetAlpha = visible ? 1f : 0f;
        var duration = instant ? 0f : 0.25f;
        Tweener.Tween(this, BackgroundHighlight.color.a, targetAlpha, duration, TweenStyle.quadratic,
            value =>
            {
                var color = BackgroundHighlight.color;
                color.a = value;
                BackgroundHighlight.color = color;
            }
        );
    }
}
