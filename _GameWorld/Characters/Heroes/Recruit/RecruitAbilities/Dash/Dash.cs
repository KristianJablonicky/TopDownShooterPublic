using UnityEngine;

[CreateAssetMenu(fileName = "Dash", menuName = "Abilities/Movement/Dash")]
public class Dash : ActiveAbility
{
    [SerializeField] private float appliedVelocity = 25f;

    protected override void OnKeyDown(Vector2 position) { }

    protected override void OnKeyUp(Vector2 position)
    {
        var movement = owner.MovementController;
        if (movement.GetMoveVelocityNormalized.magnitude == 0f) return;

        owner.MovementController.ApplyForceInWalkingDirection(appliedVelocity);

        OnCast();
    }
}
