using UnityEngine.EventSystems;

public class FadeInCanvasOnHover : CanvasFaderBase, IPointerEnterHandler//, IPointerExitHandler
{
    private static FadeInCanvasOnHover currentlyHovered;

    private void OnEnable()
    {
        canvasGroup.alpha = 0f;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentlyHovered != null &&
            currentlyHovered.gameObject == gameObject) return;

        if (currentlyHovered != null
            && currentlyHovered.gameObject.activeSelf)
        {
            currentlyHovered.OnPointerExit(eventData);
        }
        currentlyHovered = this;
        TweenState(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TweenState(false);
    }
}
