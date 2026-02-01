using System;
using System.Collections.Generic;
using Unity.Netcode.Components;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovementController : MonoBehaviour, IResettable
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float maxSpeed = 10f;

    [field: Header("References")]
    [field: SerializeField] public Rigidbody2D RigidBody { get; private set; }
    [SerializeField] private NetworkTransform networkTransform;

    [Header("Network settings")]
    [SerializeField] private float velocityCheckInterval = 0.25f;

    public event Action<Vector2> PositionChanged;
    public event Action<Floor> FloorChanged;

    private Dictionary<object, float> movementMultipliers;

    private float movementMultiplier = 1f;
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
        // NPCs
        if (networkTransform == null)
        {
            enabled = false;
            isLocalPlayer = false;
            return;
        }

        movementMultipliers = new();
        isLocalPlayer = networkTransform.IsOwner;
    }
    /// <summary>
    /// Add or change a movement multiplier from a specific source.
    /// </summary>
    /// <param name="source">the external caller</param>
    /// <param name="multiplier">movement multiplier in %. Negative numbers mean X% slow</param>
    public void AddOrChangeMultiplier(object source, int multiplier) => AddOrChangeMultiplier(source, multiplier / 100f);
    
    /// <summary>
    /// Add or change a movement multiplier from a specific source.
    /// </summary>
    /// <param name="source">the external caller</param>
    /// <param name="multiplier">movement multiplier as a float. Negative numbers mean X% slow</param>
    public void AddOrChangeMultiplier(object source, float multiplier)
    {
        if (!movementMultipliers.ContainsKey(source))
        {
            movementMultipliers.Add(source, multiplier);
        }
        movementMultipliers[source] = multiplier;

        SetMovementMultiplier();
    }

    public void RemoveMultiplier(object source)
    {
        if (!movementMultipliers.ContainsKey(source)) return;
        movementMultipliers.Remove(source);
        SetMovementMultiplier();
    }

    private void SetMovementMultiplier()
    {
        var totalMultiplier = 1f;
        foreach (var multiplier in movementMultipliers.Values)
        {
            totalMultiplier *= (1f + multiplier);
        }
        
        moveVelocity *= (totalMultiplier / movementMultiplier);
        movementMultiplier = totalMultiplier;

    }




    public void WalkInDirection(float x, float y)
    {
        if (!MovementEnabled) return;
        if (RigidBody.linearVelocity.magnitude > maxSpeed * movementMultiplier) return;

        var velocity = new Vector2(x, y).normalized;
        moveVelocity = velocity * (moveSpeed * movementMultiplier);
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
        movementMultiplier = 1f;
        movementMultipliers?.Clear();
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