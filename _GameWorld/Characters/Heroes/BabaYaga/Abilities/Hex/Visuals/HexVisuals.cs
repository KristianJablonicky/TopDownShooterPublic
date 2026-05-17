using UnityEngine;

public class HexVisuals : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LineRenderer lineRenderer;
    [Header("Settings")]
    [SerializeField, Range(0f, 0.5f)] private float pointDistance = 0.4f;
    [SerializeField, Range(0f, 0.5f)] private float pointWidth = 0.1f;
    [SerializeField, Range(0f, 1f)] private float randomness = 0.25f;
    [SerializeField] private float duration = 0.5f;
    public void Init(Vector2 start, Vector2 end)
    {
        lineRenderer.positionCount = 4;

        var side = Random.value < 0.5f;

        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, GetPoint(start, end, true, side));
        lineRenderer.SetPosition(2, GetPoint(start, end, false, !side));
        lineRenderer.SetPosition(3, end);

        var startWidth = lineRenderer.startWidth;
        var endWidth = lineRenderer.endWidth;
        Tweener.Tween(this, 0f, 1f, duration, TweenStyle.sinusPingPong,
            value =>
            {
                lineRenderer.startWidth = startWidth * value;
                lineRenderer.endWidth = endWidth * value;
            },
            () => Destroy(gameObject)
        );
    }

    private Vector2 GetPoint(Vector2 start, Vector2 end, bool first, bool rightSide)
    {
        var dir = end - start;
        var dist = dir.magnitude;
        var n = dir / dist;

        var perp = new Vector2(-n.y, n.x);

        var t = first ? pointDistance : (1f - pointDistance);
        var side = rightSide ? 1f : -1f;

        var along = dist * t;
        var offset = dist * pointWidth * side * randomness;
        return start + n * along + perp * offset;
    }
}
