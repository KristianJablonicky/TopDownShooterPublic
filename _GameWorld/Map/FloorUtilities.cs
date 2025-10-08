public static class FloorUtilities
{
    public static float GetYOffset(Floor targetFloor)
    {
        return targetFloor switch
        {
            Floor.First => -1 * Constants.floorYOffset,
            Floor.Second => Constants.floorYOffset,
            _ => throw new System.NotImplementedException(),
        };
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
}
