using UnityEngine;

public class CharacterLegAnimator : MonoBehaviour
{

    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private CharacterMediator owner;


    [Header("Leg Sprites")]
    [SerializeField] private Sprite[] legSprites;
    [SerializeField] private Sprite defaultLegSprite;

    [Header("Animation Settings")]
    [SerializeField] private float animationSpeed = 10f;
    [SerializeField] private float maxDegreesPerSecond = 180f;
    [SerializeField] private float minVelocityToAnimate = 0.1f, minVelocityToRotate = 0.01f;

    private MovementController movementController;
    private Vector2 currentVelocity;

    private int frameIndex;
    private float frameTimer, speed, currentAngle;

    private void Start()
    {
        movementController = owner.MovementController;
    }

    private void LateUpdate()
    {
        currentVelocity = movementController.GetLinearVelocity();
        speed = currentVelocity.magnitude;
        HandleRotation();
        HandleAnimation();
    }

    private void HandleRotation()
    {
        if (speed < minVelocityToRotate) return;
        
        var targetAngle = 90f + Mathf.Atan2(currentVelocity.y, currentVelocity.x) * Mathf.Rad2Deg;

        if (speed > minVelocityToAnimate
        && owner.IsLocalPlayer)
        {
            currentAngle = Mathf.MoveTowardsAngle(
                currentAngle,
                targetAngle,
                maxDegreesPerSecond * Time.deltaTime
            );
        }
        else
        {
            currentAngle = targetAngle;
        }
        transform.rotation = Quaternion.Euler(0, 0, currentAngle);
    }

    private void HandleAnimation()
    {
        if (speed < minVelocityToAnimate)
        {
            spriteRenderer.sprite = defaultLegSprite;
            frameIndex = 0;
            frameTimer = 0f;
            return;
        }

        var currentSpeed = Mathf.Min(speed, Constants.CharacterMaxMovementSpeed); // avoid teleportation caused visual glitches
        movementController.GetCurrentSpeed();
        frameTimer += Time.deltaTime * currentSpeed * animationSpeed;

        if (frameTimer >= 1f)
        {
            frameTimer -= 1f;
            frameIndex = (frameIndex + 1) % legSprites.Length;
        }

        spriteRenderer.sprite = legSprites[frameIndex];
    }
}
