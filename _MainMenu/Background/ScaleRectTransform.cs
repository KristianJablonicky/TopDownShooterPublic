using UnityEngine;

public class ScaleRectTransform : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private RectTransform canvas;
    [SerializeField, Range(0f, 1f)] private float margin = 0.1f;
    [SerializeField] private FloatRange aspectRatio = (16f, 9f);
    private float aspect;

    void Start()
    {
        aspect = aspectRatio.start / aspectRatio.end;

        // Ensure correct anchor setup
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.zero;
        rectTransform.pivot = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;

        Scale();
    }

    void Scale()
    {
        float canvasWidth = canvas.rect.width;

        float width = canvasWidth * (1f + margin);
        float height = width / aspect;

        rectTransform.sizeDelta = new Vector2(width, height);
    }
}
