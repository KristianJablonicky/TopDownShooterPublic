using UnityEngine;

public static class ColorExtensions
{
    public static Color Multiply(this Color color, float r, float g, float b, float a = 1f)
    {
        return new Color(
            color.r * r,
            color.g * g,
            color.b * b,
            color.a * a
        );
    }
}