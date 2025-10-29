using UnityEngine;

[CreateAssetMenu(fileName = "ThrowBomb", menuName = "Abilities/Utility/ThrowBomb")]
public class ThrowBomb : UtilityAbility
{
    [Header("Values - Throw")]
    [SerializeField] private float throwVelocity = 10f;
    [SerializeField] private float carriedVelocityRatio = 0.5f;
    [SerializeField] private Bomb bomb;

    protected override void OnKeyUp(Vector2 position)
    {
        var throwVelocity2D = (position - owner.GetPosition()).normalized * throwVelocity
                                + owner.MovementController.GetLinearVelocity * carriedVelocityRatio;

        TryInvokeRPC<RecruitAbilityRPCs>(rpcs =>
        {
            rpcs.RequestBombRPC(owner.PlayerId, throwVelocity2D);
            OnCast();
        });
    }
    protected override void OnKeyDown(Vector2 position) { }

    protected override string _GetAbilitySpecificStats()
    {
        return $"Explosion radius: {bomb.ExplosionRadius}\nExplosion damage: {bomb.DamageRangeString}";
    }
}
