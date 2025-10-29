using UnityEngine;

public class DjinnVision : MonoBehaviour
{
    [SerializeField] private DjinnSummoned djinn;
    public void Init(float duration, float start, float end)
    {
        Tweener.Tween(this, start, end, duration, TweenStyle.quadratic,
            value => transform.localScale = Vector2.one * value
        );
    }
    public void Init(float radius)
    {
        transform.localScale = Vector2.one * radius;
    }
}
