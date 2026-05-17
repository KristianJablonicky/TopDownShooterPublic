using UnityEngine;

public class ImportantPositions : SingletonMonoBehaviour<ImportantPositions>
{
    [field: SerializeField] public Transform[] StairsBasement { get; private set; }
    [field: SerializeField] public Transform[] StairsFirstFloor { get; private set; }
    [field: SerializeField] public Transform[] DefenderPositionSpots { get; private set; }
    [field: SerializeField] public Transform[] AttackerPositionSpots { get; private set; }
    [field: SerializeField] public Transform[] RitualAltarSpots { get; private set; }
    public Vector2 GetTargetPosition(Transform aiAgent, Transform[] possibleGoals)
        => GetTargetPosition(aiAgent, possibleGoals[Random.Range(0, possibleGoals.Length)]);
    public Vector2 GetTargetPosition(Transform aiAgent, Transform goal)
    {
        if (!FloorUtilities.IsOnTheSameFloor(aiAgent, goal))
        {
            return GetClosestStair(aiAgent);
        }
        return goal.position;
    }
    public Vector2 GetTargetPosition(Transform aiAgent, Vector2 goalPos)
    {
        if (!FloorUtilities.IsOnTheSameFloor(aiAgent.position, goalPos))
        {
            return GetClosestStair(aiAgent);
        }
        return goalPos;
    }
    public Vector2 GetClosestStair(Transform transform)
        => GetClosestStair(transform.position);
    public Vector2 GetClosestStair(Vector2 destination)
    {
        var closestDistance = 1000f;
        var stairs = GetStairs(FloorUtilities.GetCurrentFloor(destination));
        Transform returningStairs = null;
        foreach (var stair in stairs)
        {
            var distance = Vector2.Distance(destination, stair.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                returningStairs = stair;
            }
        }

        return returningStairs.position;
    }

    public Vector2 GetRandomStairs(Transform transform)
    {
        var stairs = GetStairs(FloorUtilities.GetCurrentFloor(transform));
        return stairs[Random.Range(0, stairs.Length)].position;
    }

    public Vector2 AltarPosition =>
        RitualAltarSpots[Random.Range(0, RitualAltarSpots.Length)].position;

    private Transform[] GetStairs(Floor floor)
    {
        return floor switch
        {
            Floor.Basement => StairsBasement,
            Floor.Outside => StairsFirstFloor,
            _ => throw new System.ArgumentOutOfRangeException(nameof(floor), floor, null)
        };
    }
}
