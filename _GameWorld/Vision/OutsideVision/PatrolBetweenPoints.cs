using UnityEngine;

public class PatrolBetweenPoints : SinusUpdater
{
    [SerializeField] private float x, y;
    protected override void UpdateSinus(float sinusValue, float sinus01)
    {
        transform.localPosition = new Vector3(x * sinusValue, y * sinusValue);
    }
}
