using UnityEngine;

public static class FloorUtilities
{
    const float yThreshold = Constants.floorYOffset * 0.5f;
    public static float GetYOffset(Floor targetFloor)
    {
        return targetFloor switch
        {
            Floor.Basement => -1 * Constants.floorYOffset,
            Floor.Outside => Constants.floorYOffset,
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
    public static Vector2 GetPositionOnTheOtherFloor(CharacterMediator mediator)
        => GetPositionOnTheOtherFloor(mediator.GetPosition());
    public static Vector2 GetPositionOnTheOtherFloor(Vector2 position)
    {
        var floor = GetCurrentFloor(position);
        floor = GetDifferentFloor(floor);
        return GetPositionY(position, floor);
    }

    public static float? GetYOffset(AimDirection direction, Floor currentFloor)
    {
        if (currentFloor == Floor.Basement && direction == AimDirection.Down
            || currentFloor == Floor.Outside && direction == AimDirection.Up
            || direction == AimDirection.Straight)
        {
            return null;
        }

        return currentFloor switch
        {
            Floor.Basement => Constants.floorYOffset,
            Floor.Outside => -Constants.floorYOffset,
            _ => throw new System.NotImplementedException()
        };
    }
    public static Floor GetCurrentFloor(CharacterMediator mediator)
        => GetCurrentFloor(mediator.GetPosition());
    public static Floor GetCurrentFloor(Transform transform)
        => GetCurrentFloor(transform.position);
    public static Floor GetCurrentFloor(Vector2 position)
        => position.y < yThreshold ? Floor.Basement : Floor.Outside;

    public static Floor GetDifferentFloor(Floor floor)
        => floor == Floor.Basement ? Floor.Outside : Floor.Basement;
}
