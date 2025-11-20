using UnityEngine;

[CreateAssetMenu(fileName = "FulfilWish", menuName = "Abilities/Movement/FulfilWish")]
public class FulfilWish : MovementAbility
{
    [SerializeField] private float channelDuration = 1f;
    [field: SerializeField] public int HealAmount { get; private set; } = 50;
    [field: SerializeField][Range(0f, 1f)] public float MoveSpeedMultiplier { get; private set; } = 0.4f;
    [field: SerializeField] public float Duration { get; private set; } = 2f;
    [field: SerializeField] public GameObject WishVisuals { get; private set; }
    protected override void OnKeyDown(Vector2 position) { }

    private DjinnRPCs rpcs;
    protected override void SetUpRPCsReady()
    {
        TryInvokeRPC<DjinnRPCs>(rpcs => this.rpcs = rpcs);
    }

    protected override void OnKeyUp(Vector2 position)
    {
        if (rpcs.remainingWishes <= 0) return;
        if (channelingManager.RequestInterrupt())
        {
            channelingManager.StartChanneling(channelDuration, CastEffect);
        }
    }

    private void CastEffect()
    {
        OnCast();
        rpcs.remainingWishes--;
        rpcs.RequestWishRPC(owner.PlayerId);
    }

    protected override string _GetAbilitySpecificStats()
    {
        return $"Heal amount: {HealAmount}\nMovement speed bonus: {Mathf.RoundToInt(MoveSpeedMultiplier * 100f)}%\nDuration: {Duration}s\nChannel time: {channelDuration}";
    }

    public class WishModifier : IModifierStrategy
    {
        private readonly float movementSpeedMultiplier;
        public WishModifier(float movementSpeedMultiplier)
        {
            this.movementSpeedMultiplier = 1f + movementSpeedMultiplier;
        }

        public void Apply(CharacterMediator owner, Modifier modifier)
        {
            owner.MovementController.AdjustMovementMultiplier(movementSpeedMultiplier);
        }

        public void Expire(CharacterMediator owner)
        {
            owner.MovementController.AdjustMovementMultiplier(-movementSpeedMultiplier);
        }

        public bool ExpireOnRoundEnd() => true;

        public string GetDescription() => $"Moving {Mathf.FloorToInt(100f * (movementSpeedMultiplier - 1f))}% faster.";

        public bool RealTimeDuration() => true;
    }
}
