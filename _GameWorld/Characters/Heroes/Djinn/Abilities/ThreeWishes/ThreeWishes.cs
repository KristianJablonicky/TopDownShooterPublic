using UnityEngine;

[CreateAssetMenu(fileName = "ThreeWishes", menuName = "Abilities/Passive/ThreeWishes")]
public class ThreeWishes : PassiveAbility
{
    [SerializeField] private int maxWishes = 3;
    [SerializeField] private Sprite[] wishIcons;
    private DjinnRPCs rpcs;
    private bool isOwner = false;
    protected override void AbstractReset()
    {
        if (!owner.IsOwner) return;
        rpcs.remainingWishes.Set(maxWishes);
    }

    protected override void SetUp() { }
    /*
    protected override void SetUpRPCsReady()
    {
        base.SetUpRPCsReady();
        TryInvokeRPC<DjinnRPCs>(rpcs => this.rpcs = rpcs);
        rpcs.remainingWishes = new(maxWishes);
        rpcs.remainingWishes.OnValueSet += newValue => ChangeIcon(wishIcons[newValue]);
    }
    */

    public override string _GetSpecificAttributes()
    {
        return $"Number of wishes (yes, you guessed it): {maxWishes}";
    }

    protected override void SafeSetUpWithRPCsReady()
    {
        TryInvokeRPC<DjinnRPCs>(rpcs => this.rpcs = rpcs);
        rpcs.remainingWishes = new(maxWishes);
        rpcs.remainingWishes.OnValueSet += newValue => ChangeIcon(wishIcons[newValue]);
        isOwner = true;
    }
}
