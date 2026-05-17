using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonAnimations : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    //[SerializeField] private float animationDuration = 0.2f;
    [SerializeField, Range(1f, 2f)] private float sizeIncrease = 1.25f;
    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = Vector3.one * sizeIncrease;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = Vector3.one;
    }
}
