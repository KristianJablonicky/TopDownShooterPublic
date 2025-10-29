using UnityEngine;

public static class FloorUtilities
{
    const float yThreshold = Constants.floorYOffset * 0.5f;
    public static float GetYOffset(Floor targetFloor)
    {
        return targetFloor switch
        {
            Floor.First => -1 * Constants.floorYOffset,
            Floor.Second => Constants.floorYOffset,
            _ => throw new System.NotImplementedException(),
        };
    }

    public static void ApplyYOffset(Transform transform, Floor targetFloor)
    {
        var yOffset = GetYOffset(targetFloor);
        transform.position += Vector3.up * yOffset;
    }

    public static Vector2 GetPositionY(Vector2 position, Floor targetFloor)
    {
        var yOffset = GetYOffset(targetFloor);
        return position + (Vector2.up * yOffset);
    }

    public static float? GetYOffset(AimDirection direction, Floor currentFloor)
    {
        if (currentFloor == Floor.First && direction == AimDirection.Down
            || currentFloor == Floor.Second && direction == AimDirection.Up
            || direction == AimDirection.Straight)
        {
            return null;
        }

        return currentFloor switch
        {
            Floor.First => Constants.floorYOffset,
            Floor.Second => -Constants.floorYOffset,
            _ => throw new System.NotImplementedException()
        };
    }

    public static Floor GetCurrentFloor(Transform transform)
    {
        return GetCurrentFloor(transform.position);
    }
    public static Floor GetCurrentFloor(Vector2 position) => position.y < yThreshold ? Floor.First : Floor.Second;


    public static Floor GetDifferentFloor(Floor floor) => floor == Floor.First ? Floor.Second : Floor.First;
}
