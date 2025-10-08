using UnityEngine;
using UnityEngine.EventSystems;


public class HoverManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Hoverable[] hoverableElements;
    public void OnPointerEnter(PointerEventData eventData)
    {
        InvokeMethods(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        InvokeMethods(false);
    }

    private void InvokeMethods(bool argument)
    {
        foreach (var hoverableElement in hoverableElements)
        {
            hoverableElement.HoverStateChanged(argument);
        }
    }
}
