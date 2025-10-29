using UnityEngine;

[CreateAssetMenu(fileName = "ThreeWishes", menuName = "Abilities/Passive/ThreeWishes")]
public class ThreeWishes : PassiveAbility
{
    [SerializeField] private int maxWishes = 3;
    [SerializeField] private Sprite[] wishIcons;
    private DjinnRPCs rpcs;
    protected override void AbstractReset()
    {
        rpcs.remainingWishes.Set(maxWishes);
    }

    protected override void SetUp() { }

    protected override void SetUpRPCsReady()
    {
        TryInvokeRPC<DjinnRPCs>(rpcs => this.rpcs = rpcs);
        rpcs.remainingWishes = new(maxWishes);
        rpcs.remainingWishes.OnValueSet += newValue => ChangeIcon(wishIcons[newValue]);
    }

    protected override string _GetAbilitySpecificStats()
    {
        return $"Number of wishes (yep, you guessed it): {maxWishes}";
    }
}
