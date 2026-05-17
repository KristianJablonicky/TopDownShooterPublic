using UnityEngine;

public class PatrolBetweenPoints : SinusUpdater
{
    [SerializeField] private float x, y;
    private float startX, startY;
    private void Start()
    {
        startX = transform.localPosition.x;
        startY = transform.localPosition.y;
    }

    protected override void UpdateSinus(float sinusValue, float sinus01)
    {
        transform.localPosition = new(
            startX + x * sinusValue,
            startY + y * sinusValue
        );
    }
}
