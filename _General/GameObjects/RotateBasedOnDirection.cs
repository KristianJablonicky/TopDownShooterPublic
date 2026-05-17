using UnityEngine;

public class RotateBasedOnDirection : PeriodicallyInvoked
{
    private Vector2 lastPosition;
    private void Awake()
    {
        lastPosition = transform.position;
    }
    public override void Invoke()
    {
        Vector2 current = transform.position;
        var dir = current - lastPosition;

        var angle = -1f * Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        lastPosition = current;
    }
}
