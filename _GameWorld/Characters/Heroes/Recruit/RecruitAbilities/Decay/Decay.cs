using UnityEngine;

[CreateAssetMenu(fileName = "Decay", menuName = "Abilities/Passive/Decay")]
public class Decay : PassiveAbility
{
    [SerializeField] private float decayInterval = 5f;
    [SerializeField] private float initialDecayDelay = 10f;
    [SerializeField] private int decayAmount = 10;
    [SerializeField, Range(0, 100)] private int startingHealthBonus = 50;
    [SerializeField, Range(0, 100)] private int decayHealthFloor = 50;
    float timeRemaining;
    protected override void AbstractReset()
    {
        owner.HealthComponent.AdjustMaxHealth(startingHealthBonus, true);
        if (!owner.IsOwner) return;
        timeRemaining = initialDecayDelay + owner.PlayerId; // Avoid decay audio effect overlaps
        Subscribe(true);
    }

    protected override void SetUp() { }
    /*
    protected override void SetUpRPCsReady()
    {
        base.SetUpRPCsReady();
        Subscribe(true);
        if (!owner.IsLocalPlayer) return;
        owner.Died += (_) => Subscribe(false);
    */
    protected override void SafeSetUpWithRPCsReady()
    {
        if (!owner.IsOwner) return;
        Subscribe(true);
        owner.Died += (_) => Subscribe(false);
    }
    private void MidRoundUpdate(float dt)
    {
        timeRemaining -= dt;
        if (timeRemaining <= 0f)
        {
            if (owner.HealthComponent.CurrentHealth <= decayHealthFloor)
            {
                Subscribe(false);
                return;
            }
            OnDecay();
            timeRemaining += decayInterval;
        }
    }

    private void OnDecay()
    {
        owner.NetworkInput.TakeDamage(decayAmount, DamageTag.Ability);
        PlaySound();
        TryInvokeRPC<RecruitAbilityRPCs>(rpc => rpc.RequestDecayRPC());
    }

    private bool subscribed = false;
    private void Subscribe(bool subscribe)
    {
        if (subscribe == subscribed) return;

        subscribed = subscribe;
        if (subscribe)
        {
            Updater.Instance.UpdatedDuringRound += MidRoundUpdate;
        }
        else
        {
            Updater.Instance.UpdatedDuringRound -= MidRoundUpdate;
        }
    }
    public override string _GetSpecificAttributes()
    {
        return $"Starting health bonus: {startingHealthBonus}hp\nDecay interval: {decayInterval}s\nDecay health amount: {decayAmount}\nDecay stop threshold: {decayHealthFloor}\nInitial decay delay: {initialDecayDelay}s";
    }
}
