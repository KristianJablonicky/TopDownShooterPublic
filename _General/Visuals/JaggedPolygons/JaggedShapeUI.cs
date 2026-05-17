using UnityEngine;
using UnityEngine.UI;

public class JaggedShapeUI : MaskableGraphic
{
    [SerializeField] protected Vector2[] points;

    [Header("References")]
    [SerializeField] protected Image imageTexture;

    protected Rect rect;

    protected override void Awake()
    {
        base.Awake();
        rect = rectTransform.rect;
    }

    protected virtual void BeforeMeshPopulation() { }
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        
        if (points == null
            || points.Length < 3)
        {
            ResetPoints();
        }

        BeforeMeshPopulation();

        var center = Vector2.zero;
        foreach (var p in points) center += p;
        center /= points.Length;

        vh.AddVert(center, color, Vector2.one * 0.5f);

        for (int i = 0; i < points.Length; i++)
        {
            vh.AddVert(points[i], color, points[i]);
        }

        for (int i = 1; i < points.Length; i++)
        {
            vh.AddTriangle(0, i, i + 1);
        }

        vh.AddTriangle(0, points.Length, 1);
    }

    [ContextMenu("Reset Points")]
    public void ResetPoints()
    {
        rect = GetPixelAdjustedRect();
        points = new Vector2[]
        {
            new(rect.xMin, rect.yMin),
            new(rect.xMin, rect.yMax),
            new(rect.xMax, rect.yMax),
            new(rect.xMax, rect.yMin)
        };
    }

    public virtual void SetColor(Color newColor)
    {
        color = newColor;
        if (imageTexture != null)
        {
            imageTexture.color = newColor;
        }
    }
}
