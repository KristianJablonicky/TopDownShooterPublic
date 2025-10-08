using UnityEngine;

[CreateAssetMenu(fileName = "ThrowBomb", menuName = "Abilities/Utility/ThrowBomb")]
public class ThrowBomb : ActiveAbility
{
    [Header("Values - Throw")]
    [SerializeField] private float throwVelocity = 10f;
    [SerializeField] private float carriedVelocityRatio = 0.5f;

    protected override void OnKeyUp(Vector2 position)
    {
        var throwVelocity2D = (position - owner.GetPosition()).normalized * throwVelocity
                                + owner.MovementController.GetLinearVelocity * carriedVelocityRatio;
        if (characterRPCs is RecruitAbilityRPCs recruitRPCs)
        {
            recruitRPCs.RequestBombRPC(owner.PlayerId, throwVelocity2D);
            OnCast();
        }
    }
    protected override void OnKeyDown(Vector2 position) { }
}
