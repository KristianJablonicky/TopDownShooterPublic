using UnityEngine;

[CreateAssetMenu(fileName = "DashPostMortem", menuName = "Abilities/PostMortem/Dash")]
public class DashPostMortem : AbilityPostMortem
{
    [SerializeField] private float appliedVelocity = 10f;

    public override AbilityHotKeys KeyCode { get; protected set; } = AbilityHotKeys.Movement;

    protected override void OnKeyUpSecure(Vector2 position)
    {
        var positionNormalized = owner.InputHandler.GetCursorPositionNormalized();
        characterRPCs.RequestApplyForceRPC(teamMate.PlayerId, positionNormalized * appliedVelocity);

        OnCast();
    }

    public void ExecuteDash(CharacterMediator mediator, Vector2 velocity)
    {
        mediator.MovementController.ApplyForce(velocity);
    }
    protected override void OnKeyDownSecure(Vector2 position) { }

    protected override string _GetAbilitySpecificStats()
    {
        return $"Applied velocity: {appliedVelocity}";
    }
}
