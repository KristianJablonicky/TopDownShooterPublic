using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class PlayerViewScaler : MonoBehaviour
{
    [SerializeField] private RectTransform rt;
    [SerializeField] private float width = 16f, height = 9f;

    private float targetAspect;
    private void Awake()
    {
        targetAspect = width / height;
        Resize();
    }

    protected void OnRectTransformDimensionsChange()
    {
        Resize();
    }

    private void Resize()
    {
        var parentWidth = ((RectTransform)rt.parent).rect.width;
        var parentHeight = ((RectTransform)rt.parent).rect.height;
        var parentAspect = parentWidth / parentHeight;

        if (parentAspect > targetAspect)
        {
            // Parent is wider than desired, height fills parent, width adjusts
            var newHeight = parentHeight;
            var newWidth = newHeight * targetAspect;
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newHeight);
        }
        else
        {
            // Parent is taller than desired, width fills parent, height adjusts
            var newWidth = parentWidth;
            var newHeight = newWidth / targetAspect;
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newHeight);
        }

    }
}