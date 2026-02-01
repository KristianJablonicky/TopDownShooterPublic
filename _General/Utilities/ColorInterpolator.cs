using System;
using UnityEngine;
using UnityEngine.UI;

public class ColorInterpolator : SinusUpdater
{
    [SerializeField] private Color colorA = Color.black, colorB = Color.white;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Image image;

    private Action<Color> updateColor;
    private void Start()
    {
        if (spriteRenderer != null) updateColor += (color) => spriteRenderer.color = color;
        if (image != null) updateColor += (color) => image.color = color;
    }
    protected override void UpdateSinus(float sinusValue, float sinus01)
    {
        var currentColor = Color.Lerp(colorA, colorB, sinus01);
        updateColor?.Invoke(currentColor);
    }
}
