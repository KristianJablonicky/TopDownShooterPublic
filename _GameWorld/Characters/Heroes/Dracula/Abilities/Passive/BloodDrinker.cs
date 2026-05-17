using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "BloodDrinker", menuName = "Abilities/Passive/BloodDrinker")]
public class BloodDrinker : PassiveAbility
{
    [SerializeField, Range(0f, 1f)] private double healthThresholdPercentage = 0.4d;
    [SerializeField, Range(0f, 1f)] private double healthThresholdPercentageSpooked = 0.6d;
    [SerializeField] private int healOnKill = 50;
    [SerializeField] private float beforeKillDelay = 0.5f;

    [SerializeField] private OneTimeAnimation drainAnimationPrefab;
    private Dictionary<CharacterMediator, bool> mediatorsDrained;
    protected override void SetUp() { }
    protected override void SafeSetUpWithRPCsReady()
    {
        owner.NetworkInput.OnDamageDealtToMediator += OnDamageDealt;
        owner.ScoredAKill += OnKill;

        var manager = CharacterManager.Instance;

        mediatorsDrained = new();
        foreach (var player in manager.Mediators.Values)
        {
            AddToDictionary(player);
        }
        manager.CharacterRegistered += AddToDictionary;
    }

    private void AddToDictionary(CharacterMediator character)
    {
        if (character == owner) return;
        mediatorsDrained.Add(character, false);
    }

    private void OnDamageDealt(int damage, DamageTag tag, CharacterMediator hitMediator)
    {
        if (tag != DamageTag.Shot) return; // just in case

        if (!mediatorsDrained.ContainsKey(hitMediator))
        {
            Debug.LogWarning($"BloodDrinker: {hitMediator} not in dict");
            return;
        }

        // this player has already been drained or is an NPC
        if (mediatorsDrained[hitMediator]) return;
        var hc = hitMediator.HealthComponent;
        var threshold = healthThresholdPercentage;
        if (hitMediator.Modifiers.ModifierExistsOfType(
            typeof(ShadowWaveSpook)))
        {
            threshold = healthThresholdPercentageSpooked;
        }

        if (hc.CanTakeDamage
        &&  hc.CurrentHealth <= threshold * hc.MaxHealth)
        {
            mediatorsDrained[hitMediator] = true;
            ExecuteAfterDelay(hitMediator);
            owner.NetworkInput.RequestHealRpc(healOnKill, true);
        }
    }

    private void OnKill(CharacterMediator killedMediator, CharacterMediator killer)
    {
        if (mediatorsDrained[killedMediator]) return; // avoid getting the heal 2 times if the player was drained
        owner.NetworkInput.RequestHealRpc(healOnKill, true);
    }

    private async void ExecuteAfterDelay(CharacterMediator hitMediator)
    {
        TryInvokeRPC<DraculaRPCs>(rpcs => rpcs.RequestShowBloodDrinkerProcRPC(hitMediator.PlayerId));
        await Task.Delay((int)(beforeKillDelay * 1000f));
        hitMediator.HealthComponent.TakeLethalDamage(owner, DamageTag.Ability);
    }

    public void ShowAnimation(ulong playerID)
    {
        var hitMediator = CharacterManager.Instance.Mediators[playerID];
        var animation = Instantiate(drainAnimationPrefab, hitMediator.GetTransform());
        animation.PlayAnimation(beforeKillDelay);
    }

    protected override void AbstractReset()
    {
        if (mediatorsDrained is null) return;
        foreach (var key in mediatorsDrained.Keys.ToList())
        {
            mediatorsDrained[key] = false;
        }
    }

    public override string _GetSpecificAttributes()
    {
        return $"Health threshold: {healthThresholdPercentage * 100d}%\nHealth threshold against spooked heroes: {healthThresholdPercentageSpooked * 100d}%\nHeal on kill: {healOnKill}\nDelay: {beforeKillDelay}";
    }
}
