using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class PlayerViewScaler : MonoBehaviour
{
    [SerializeField] private RectTransform rt;
    [SerializeField] private float width = 16f, height = 9f;

    private float desiredAspect;
    private void Awake()
    {
        desiredAspect = width / height;
        Resize();
    }

    protected void OnRectTransformDimensionsChange()
    {
        Resize();
    }

    private void Resize()
    {
        float parentWidth = ((RectTransform)rt.parent).rect.width;
        float parentHeight = ((RectTransform)rt.parent).rect.height;
        float parentAspect = parentWidth / parentHeight;

        if (parentAspect > desiredAspect)
        {
            // Parent is wider than desired, height fills parent, width adjusts
            float newHeight = parentHeight;
            float newWidth = newHeight * desiredAspect;
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newHeight);
        }
        else
        {
            // Parent is taller than desired, width fills parent, height adjusts
            float newWidth = parentWidth;
            float newHeight = newWidth / desiredAspect;
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newHeight);
        }

    }
}