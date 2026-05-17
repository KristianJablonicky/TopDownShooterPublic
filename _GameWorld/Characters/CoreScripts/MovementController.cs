using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovementController : MonoBehaviour, IResettable
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float maxSpeed = 10f;

    [field: Header("References")]
    [SerializeField] private CharacterMediator owner;
    [field: SerializeField] public Rigidbody2D RigidBody { get; private set; }

    [Header("Network settings")]
    [SerializeField] private float velocityCheckInterval = 0.25f;

    public event Action<Vector2> PositionChanged;
    public event Action<Floor> FloorChanged;

    public ModifiableFloat MovementModifiers { get; private set; }

    private bool _movementEnabled = true;
    private Vector2 moveVelocity;
    private bool isLocalPlayer;

    public bool MovementEnabled
    {
        get => _movementEnabled;
        set
        {
            _movementEnabled = value;
            if (!value)
            {
                moveVelocity = Vector2.zero;
                RigidBody.linearVelocity = Vector2.zero;
            }
        }
    }

    private void Start()
    {
        if (owner == null
        ||  owner.IsTrainingDummy)
        {
            enabled = false;
            isLocalPlayer = false;
            return;
        }
        MovementModifiers = new();
        MovementModifiers.Multiplier.T1SetFromT2 += OnMovementSpeedChange;
        
        isLocalPlayer = owner.IsOwner;
    }
    private void OnMovementSpeedChange(float multiplier, float oldMultiplier)
    {
        moveVelocity *= (multiplier / oldMultiplier);
    }
    public void SetEulerAngleZ(float angle)
    {
        cameraRotation = Quaternion.Euler(0f, 0f, angle);
    }
    private Quaternion cameraRotation;
    public void WalkInDirection(float x, float y)
    {
        WalkInDirection(new Vector2(x, y));
    }
    public void WalkInDirection(Vector2 direction)
    {
        if (!MovementEnabled) return;
        if (RigidBody.linearVelocity.magnitude > maxSpeed * MovementModifiers) return;
        
        /*
        var camRelativeDir = cameraRotation * direction;
        moveVelocity = camRelativeDir.normalized * (moveSpeed * MovementModifiers);
        */
        moveVelocity = direction.normalized * (moveSpeed * MovementModifiers);
    }

    private void FixedUpdate()
    {
        RigidBody.AddForce(moveVelocity);
    }
    
    /// <summary>
    /// Get the current walking speed of a character
    /// </summary>
    /// <returns>A value from the range of <0; 1)</returns>
    public float GetCurrentSpeed() => RigidBody.linearVelocity.magnitude / Constants.CharacterMaxMovementSpeed;

    public void SetPosition(float x, float y, Floor? newFloor)
    {
        SetPosition(new(x, y), newFloor);
    }

    /// <summary>
    /// Set the position of the character instantly, invoking position and floor change events as needed.
    /// </summary>
    /// <param name="newFloor">Leave as null to dynamically determine if a new floor is visited</param>
    public void SetPosition(Vector2 newPosition, Floor? newFloor = null)
    {
        InvokeFloorChange(newPosition, newFloor);

        transform.position = newPosition;
        PositionChanged?.Invoke(newPosition);
    }
    public void SetPositionQuiet(Vector2 newPosition) => transform.position = newPosition;

    private void InvokeFloorChange(Vector2 newPosition, Floor? newFloor)
    {
        if (newFloor.HasValue)
        {
            FloorChanged?.Invoke(newFloor.Value);
            return;
        }
        
        var newCurrentFloor = FloorUtilities.GetCurrentFloor(newPosition);
        if (newCurrentFloor != FloorUtilities.GetCurrentFloor(transform.position))
        {
            FloorChanged?.Invoke(newCurrentFloor);
        }
    }

    public Vector2 GetMoveVelocityNormalized => moveVelocity.normalized;
    public Vector2 GetLinearVelocity()
    {
        if (isLocalPlayer) return RigidBody.linearVelocity;
        return estimatedVelocity;
    }

    public void ApplyForceInWalkingDirection(float force)
    {
        ApplyForceInDirection(force, moveVelocity.normalized);
    }

    public void ApplyForceInDirection(float force, Vector2 direction)
    {
        ApplyForce(direction * force);
    }
    public void ApplyForce(Vector2 force)
    {
        RigidBody.AddForce(force, ForceMode2D.Impulse);
    }

    public float GetVelocity() => RigidBody.linearVelocity.magnitude;

    public void Reset()
    {
        RigidBody.linearVelocity = Vector2.zero;
        MovementModifiers?.Reset();
        MovementEnabled = true;
    }

    #region Network rigid body velocity estimation
    
    private bool hasLastPosition = false;
    private Vector2 lastPosition, estimatedVelocity = Vector2.zero;
    private float timer;

    private void Update()
    {
        if (isLocalPlayer) return;
        timer += Time.deltaTime;

        if (timer < velocityCheckInterval)
            return;

        timer -= velocityCheckInterval;

        Vector2 pos = transform.position;

        if (!hasLastPosition)
        {
            hasLastPosition = true;
            lastPosition = pos;
            estimatedVelocity = Vector2.zero;
            return;
        }

        estimatedVelocity = (pos - lastPosition) / velocityCheckInterval;
        lastPosition = pos;
    }
    #endregion
}