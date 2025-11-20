using UnityEngine;

public static class SpriteRendererExtensions
{
    public static void SetAlpha(this SpriteRenderer sr, float alpha)
    {
        var color = sr.color;
        color.a = alpha;
        sr.color = color;
    }

    public static void MultiplyColor(this SpriteRenderer sr, float r, float g, float b, float a = 1f)
    {
        sr.color = sr.color.Multiply(r, g, b, a);
    }
}
