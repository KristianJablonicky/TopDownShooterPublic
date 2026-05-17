using UnityEngine;

[RequireComponent(typeof(JaggedPolygonUI))]
public class JaggedMaskResize : MonoBehaviour
{
    [SerializeField] private RectTransform parentMask;
    [SerializeField, Range(0f, 1f)] private float safetyMultiplier = 0.2f;
    private void Start()
    {
        var multiplier = 1f + safetyMultiplier;
        parentMask.localScale *= multiplier;
        transform.localScale /= multiplier;
    }
}
