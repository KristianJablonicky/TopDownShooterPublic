using UnityEngine;

[CreateAssetMenu(fileName = "Curse", menuName = "Abilities/Utility/Curse")]
public class Curse : ActiveAbility
{
    [field: SerializeField] public float Duration { get; private set; } = 3f;
    [field: SerializeField] public float NearSightedMultiplier { get; private set; } = 0.5f;
    [SerializeField] private float castAnimationTime = 0.25f;

    protected override void OnKeyDown(Vector2 position) { }

    protected override void OnKeyUp(Vector2 position)
    {
        var channel = owner.Gun.ChannelingManager;

        if (channel.Channeling) return;
        channel.Channel(castAnimationTime, CastEffect);
    }

    private void CastEffect()
    {
        if (owner.AbilityRPCs is BabaYagaRPCs rpcs)
        {
            rpcs.RequestCurseRPC(owner.PlayerId);
            OnCast();
        }
        else
        {
            Debug.LogError("Character RPCs are not of the correct type");
        }
    }
}
