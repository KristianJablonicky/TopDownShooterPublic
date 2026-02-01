using UnityEngine;

public class FloorParallax : MonoBehaviour
{
    [SerializeField] private float xAndYMultiplier = 0.1f;
    private Transform playerTransform;
    private float baseX, baseY;
    private void Awake()
    {
        PlayerNetworkInput.PlayerSpawned += OnPlayerSpawn;
        enabled = false;

        baseX = transform.position.x;
        baseY = transform.position.y;
    }

    private void OnPlayerSpawn(CharacterMediator player)
    {
        player.MovementController.FloorChanged += OnFloorChange;
        playerTransform = player.GetTransform();
    }

    private void OnFloorChange(Floor newFloor)
    {
        enabled = newFloor == Floor.Outside;
    }

    private void LateUpdate()
    {
        if (playerTransform == null) return;
    
        transform.position = new
        (
            baseX + playerTransform.position.x * xAndYMultiplier,
            baseY + (playerTransform.position.y - Constants.floorYOffset) * xAndYMultiplier
        );
    }
}
