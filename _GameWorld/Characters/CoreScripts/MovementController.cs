using System;
using Unity.Netcode.Components;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovementController : MonoBehaviour, IResettable
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float walkSpeed = 3f;

    [field: Header("References")]
    [field: SerializeField] public Rigidbody2D RigidBody { get; private set; }
    [SerializeField] private NetworkTransform networkTransform;

    public event Action<Vector2> PositionChanged;
    public event Action<Floor> FloorChanged;

    private bool _movementEnabled = true;
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

    private float movementMultiplier = 1f;
    public void AdjustMovementMultiplier(float multiplier)
    {
        if (multiplier > 0f)
        {
            movementMultiplier *= multiplier;
            moveVelocity *= multiplier;
        }
        else if (multiplier < 0f)
        {
            multiplier *= -1f;
            movementMultiplier /= multiplier;
            moveVelocity /= multiplier;
        }
        else
        {
            return;
        }
    }

    private Vector2 moveVelocity;
    private bool walking = false;

    
    public void WalkInDirection(float x, float y)
    {
        if (!MovementEnabled) return;
        if (RigidBody.linearVelocity.magnitude > maxSpeed * movementMultiplier) return;

        var velocity = new Vector2(x, y).normalized;
        if (!walking)
        {
            moveVelocity = velocity * (moveSpeed * movementMultiplier);
        }
        else
        {
            moveVelocity = velocity * (walkSpeed * movementMultiplier);
        }
    }

    private void FixedUpdate()
    {
        RigidBody.AddForce(moveVelocity);
    }

    public float GetCurrentSpeed() => RigidBody.linearVelocity.magnitude / 3.75f; // <0; 1)

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
        networkTransform.Teleport(newPosition, transform.rotation, transform.localScale);
        PositionChanged?.Invoke(newPosition);
    }

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

    public void Walk(bool walk) => walking = walk;

    public Vector2 GetMoveVelocityNormalized => moveVelocity.normalized;
    public Vector2 GetLinearVelocity => RigidBody.linearVelocity;


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
        walking = false;
        movementMultiplier = 1f;
        MovementEnabled = true;
    }
}