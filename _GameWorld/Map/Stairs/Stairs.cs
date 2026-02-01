using UnityEngine;

public class Stairs : MonoBehaviour
{
    private Floor stairsLeadToFloor;

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] stairSprites;
    [SerializeField] private BoxCollider2D boxCollider;
    [SerializeField] private GameObject walkingRestrictions;
    [SerializeField] private Obstacle obstacleComponent;
    [SerializeField] private GameObject lightSource, particleSpawner;

    private Vector2 yOffset;
    private void Awake()
    {
        stairsLeadToFloor = FloorUtilities.GetDifferentFloor(FloorUtilities.GetCurrentFloor(transform.position));
        spriteRenderer.sprite = stairSprites[(int)stairsLeadToFloor];
        if (stairsLeadToFloor == Floor.Basement)
        {
            boxCollider.offset = new Vector2(0f, -0.7f);
            boxCollider.size = new Vector2(0.9f, 0.5f);
            particleSpawner.SetActive(true);
        }
        else
        {
            boxCollider.offset = new Vector2(0f, 0.7f);
            boxCollider.size = new Vector2(0.9f, 0.5f);
            lightSource.SetActive(true);
        }

        if (stairsLeadToFloor == Floor.Basement)
        {
            walkingRestrictions.transform.Rotate(0f, 0f, 180f);
            obstacleComponent.gameObject.layer = 0;
            Destroy(obstacleComponent);
        }
        yOffset = Vector2.up * FloorUtilities.GetYOffset(stairsLeadToFloor);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent(out HealthComponent healthComponent))
        {
            var mediator = healthComponent.Mediator;
            if (!mediator.IsLocalPlayer) return;

            var movementController = healthComponent.Mediator.MovementController;
            movementController.SetPosition(
                healthComponent.Mediator.GetPosition() + yOffset,
                stairsLeadToFloor
            );
        }
    }

}

public enum Floor
{
    Basement,
    Outside
}