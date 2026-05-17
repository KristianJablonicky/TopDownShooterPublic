using UnityEditor;
using UnityEngine;

public sealed class JaggedOutline : JaggedPolygonUI
{
    [SerializeField] private JaggedPolygonUI outerPolygon;
    [SerializeField] private Color innerMultiplier = Color.white,
        outerMultiplier = Color.white;
    [SerializeField] private bool setColorsOnStart = true;
    protected override void Awake()
    {
        base.Awake();
        (randomness, pointRandomness, sizeRandomness, roundedCorners) = outerPolygon.GetSettings();
        if (setColorsOnStart)
        {
            SetColor(outerPolygon.color);
        }
    }

    public override void SetColor(Color newColor)
    {
#if UNITY_EDITOR
        if (!EditorApplication.isPlaying)
            return;
#endif
        base.SetColor(newColor * innerMultiplier);
        outerPolygon.SetColor(outerMultiplier * newColor);
    }
}
