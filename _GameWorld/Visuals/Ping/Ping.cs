using UnityEngine;

public class Ping : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite spriteCurrent, spriteUp, spriteDown;
    [SerializeField] private Ping pingPrefab;

    private Color teamColor;

    public void Init(AimDirection direction)
    {
        var owner = CharacterManager.Instance.LocalPlayer;
        teamColor = CommonColors.GetTeamColor(owner.Team.Name);

        var currentFloor = FloorUtilities.GetCurrentFloor(transform);
        var otherFloor = FloorUtilities.GetDifferentFloor(currentFloor);

        if ((currentFloor == Floor.Basement && direction == AimDirection.Down)
        ||  (currentFloor == Floor.Outside && direction == AimDirection.Up))
        {
            direction = AimDirection.Straight;
        }

        SetSprite(direction, teamColor);
        if (direction == AimDirection.Straight)
        {
            SpawnPingWithTargetFloor(MapFloorToDirection(currentFloor, otherFloor));
        }
        else
        {
            SpawnPingWithTargetFloor(AimDirection.Straight);
        }
    }

    private void SpawnPingWithTargetFloor(AimDirection direction)
    {
        var pingInstance = Instantiate(pingPrefab,
            FloorUtilities.GetPositionOnTheOtherFloor(transform.position),
            Quaternion.identity);
        pingInstance.SetSprite(direction, teamColor);

    }

    public void SetSprite(AimDirection direction, Color srColor)
    {
        spriteRenderer.sprite = GetSprite(direction);
        spriteRenderer.color = srColor;
    }

    private Sprite GetSprite(AimDirection direction)
    {
        return direction switch
        {
            AimDirection.Up => spriteUp,
            AimDirection.Down => spriteDown,
            _ => spriteCurrent,
        };
    }

    private AimDirection MapFloorToDirection(Floor current, Floor other)
    {
        if (current > other) return AimDirection.Up;
        return AimDirection.Down;
    }
}
