using UnityEngine;

public class RectTransformScaler : MonoBehaviour
{
    [SerializeField] private RectTransform parentRectTransform;
    [SerializeField] private Vector2 intendedParentSize;
    void Start()
    {
        var scaleMultiplier = Mathf.Min(parentRectTransform.rect.width / intendedParentSize.x, parentRectTransform.rect.height / intendedParentSize.y);
        transform.localScale = new Vector2(scaleMultiplier, scaleMultiplier);
    }
}
