using UnityEngine;

public class Stairs : MonoBehaviour
{
    [SerializeField] private Floor stairsLeadToFloor;

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] stairSprites;
    [SerializeField] private BoxCollider2D boxCollider;
    [SerializeField] private GameObject walkingRestrictions;
    [SerializeField] private Obstacle obstacleComponent;
    private void Awake()
    {
        spriteRenderer.sprite = stairSprites[(int)stairsLeadToFloor];
        if (stairsLeadToFloor == Floor.First)
        {
            boxCollider.offset = new Vector2(0f, -0.7f);
            boxCollider.size = new Vector2(0.9f, 0.5f);
        }
        else
        {
            boxCollider.offset = new Vector2(0f, 0.7f);
            boxCollider.size = new Vector2(0.9f, 0.5f);
        }

        if (stairsLeadToFloor != Floor.Second)
        {
            walkingRestrictions.transform.Rotate(0f, 0f, 180f);
            obstacleComponent.gameObject.layer = 0;
            Destroy(obstacleComponent);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent(out HealthComponent healthComponent))
        {
            var mediator = healthComponent.mediator;
            if (!mediator.IsLocalPlayer) return;
            var yOffset = FloorUtilities.GetYOffset(stairsLeadToFloor);
            var movementController = healthComponent.mediator.MovementController;
            movementController.SetPosition(healthComponent.mediator.GetPosition() + new Vector2(0f, yOffset));
        }
    }

}

public enum Floor
{
    First,
    Second
}