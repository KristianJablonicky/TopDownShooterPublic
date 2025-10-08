using UnityEngine;

[CreateAssetMenu(fileName = "DashPostMortem", menuName = "Abilities/PostMortem/Dash")]
public class DashPostMortem : AbilityPostMortem
{
    [SerializeField] private float appliedVelocity = 10f;

    protected override void OnKeyUpSecure(Vector2 position)
    {
        if (characterRPCs is RecruitAbilityRPCs rpcs)
        {
            rpcs.RequestAllyDashRPC(teamMate.PlayerId, position.normalized * appliedVelocity);
        }
        else
        {
            Debug.LogError(characterRPCs + " not of type RecruitAbilityRPCs");
        }
        OnCast();
    }

    public void ExecuteDash(CharacterMediator mediator, Vector2 velocity)
    {
        mediator.MovementController.ApplyForce(velocity);
        Debug.Log($"Applied {velocity} velocity");
    }
    protected override void OnKeyDownSecure(Vector2 position) { }
    protected override void ThirdEyeOpen() { }
    protected override void ThirdEyeClosed() { }
}
