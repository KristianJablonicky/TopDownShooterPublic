using UnityEngine;

public class ObjectiveArea : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)] private float highlightAlpha = 0.25f;
    [Header("References")]
    [SerializeField] private SpriteRenderer highlightSr;
    public void GetActivated()
    {
        highlightSr.SetAlpha(highlightAlpha);
    }
    public void GetDeactivated()
    {
        highlightSr.SetAlpha(0f);
    }
}
