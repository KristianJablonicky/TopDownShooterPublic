using UnityEngine;

public class ObjectiveArea : MonoBehaviour, IActivatable
{
    [SerializeField, Range(0f, 1f)] private float highlightAlpha = 0.25f;
    [Header("References")]
    [SerializeField] private SpriteRenderer highlightSr;
    public void GetActivated()
    {
        gameObject.SetActive(true);
    }
    public void GetDeactivated()
    {
        Highlight(false);
        gameObject.SetActive(false);
    }

    public void Highlight(bool highlight)
    {
        if (highlight)
        {
            highlightSr.SetAlpha(highlightAlpha);
        }
        else
        {
            highlightSr.SetAlpha(0f);
        }
    }
}
