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


    public bool MovementEnabled { get; set; } = true;
    public float movementMultiplier = 1f;
    public event Action<Vector2> PositionChanged;

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

    public float GetCurrentSpeed() => RigidBody.linearVelocity.magnitude / 0.375f;

    public void SetPosition(float x, float y)
    {
        SetPosition(new Vector2(x, y));
    }
    public void SetPosition(Vector2 newPosition)
    {
        transform.position = newPosition;
        networkTransform.Teleport(newPosition, transform.rotation, transform.localScale);
        PositionChanged?.Invoke(newPosition);
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
    }
}