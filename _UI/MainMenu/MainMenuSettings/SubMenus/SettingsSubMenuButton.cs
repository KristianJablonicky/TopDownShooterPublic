using System;
using TMPro;
using UnityEngine;

public sealed class SettingsSubMenuButton : MonoBehaviour, IActivatable
{
    [field: SerializeField] public SettingsSubmenus SubmenuType {  get; private set; }

    [Header("References")]
    [field: SerializeField] public RectTransform RectTransform { get; private set; }
    [SerializeField] private JaggedPolygonUI polygonBackground;
    [SerializeField] private JaggedUIAnimator animator;
    [SerializeField] private TMP_Text buttonText;

    [Header("Visual Settings")]
    [SerializeField] private Color colorHighlighted = Color.white;


    public Action<SettingsSubMenuButton> Clicked;

    private Color colorDefault;

    private void Awake()
    {
        colorDefault = polygonBackground.color;
        buttonText.text = SubmenuType.ToString();
    }

    public void GetActivated()
    {
        Clicked?.Invoke(this);
        polygonBackground.SetColor(colorHighlighted);
        animator.enabled = true;
    }

    public void GetDeactivated()
    {
        animator.enabled = false;
        polygonBackground.SetColor(colorDefault);
    }
}

public enum SettingsSubmenus
{
    Audio,
    Video,
    Gameplay
}
