using UnityEngine;

[CreateAssetMenu(fileName = "FulfilWish", menuName = "Abilities/Movement/FulfilWish")]
public class FulfilWish : MovementAbility
{
    [SerializeField] private float channelDuration = 1f;
    [field: SerializeField] public int HealAmount { get; private set; } = 50;
    [field: SerializeField][Range(0f, 1f)] public float MoveSpeedMultiplier { get; private set; } = 0.4f;
    [field: SerializeField] public float Duration { get; private set; } = 2f;
    [field: SerializeField] public GameObject WishVisuals { get; private set; }
    [field: SerializeField] public OneTimeAnimation FingerGunVisuals { get; private set; }
    protected override void OnKeyDown(Vector2 position) { }

    private DjinnRPCs rpcs;
    private const float animationDurationBonus = 0.25f;

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
            AlsoPlayAnimation(durationBonus: animationDurationBonus);
            rpcs.FingerGunRpc(owner.PlayerId);
            OnCast();
        }
    }

    private void CastEffect()
    {
        rpcs.remainingWishes--;
        rpcs.RequestWishRPC(owner.PlayerId);
    }

    public override string _GetSpecificAttributes()
    {
        return $"Heal amount: {HealAmount}\nMovement speed bonus: {Mathf.RoundToInt(MoveSpeedMultiplier * 100f)}%\nDuration: {Duration}s\nChannel time: {channelDuration}s";
    }

    public class WishModifier : IModifierStrategy
    {
        private readonly float movementSpeedMultiplier;
        public WishModifier(float movementSpeedMultiplier)
        {
            this.movementSpeedMultiplier = movementSpeedMultiplier;
        }

        public ModifierType ModifierType => ModifierType.Buff;

        public void Apply(CharacterMediator owner, Modifier modifier)
        {
            owner.MovementController.MovementModifiers.AddOrChangeMultiplier(this, movementSpeedMultiplier);
            modifier.Stacks.OnValueSet += stacks =>
            {
                owner.MovementController.MovementModifiers.AddOrChangeMultiplier(this, stacks * movementSpeedMultiplier);
            };
        }

        public void Expire(CharacterMediator owner)
        {
            owner.MovementController.MovementModifiers.RemoveMultiplier(this);
        }

        public bool ExpireOnRoundEnd() => true;

        public string GetDescription() => $"Moving {Mathf.FloorToInt(100f * movementSpeedMultiplier)}% faster.";

        public bool RealTimeDuration() => true;
    }

    public float ChannelingDuration => channelDuration + animationDurationBonus;
}
