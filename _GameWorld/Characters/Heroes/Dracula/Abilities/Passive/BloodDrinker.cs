using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "BloodDrinker", menuName = "Abilities/Passive/BloodDrinker")]
public class BloodDrinker : PassiveAbility
{
    [SerializeField] private int healthThreshold = 30;
    [SerializeField] private int healOnKill = 50;
    [SerializeField] private float beforeKillDelay = 0.5f;

    [SerializeField] private OneTimeAnimation drainAnimationPrefab;
    private Dictionary<CharacterMediator, bool> mediatorDrained;
    protected override void SetUp() { }
    protected override void SetUpRPCsReady()
    {
        if (!owner.IsLocalPlayer) return;
        owner.NetworkInput.OnDamageDealtToMediator += OnDamageDealt;
        owner.ScoredAKill += OnKill;

        var manager = CharacterManager.Instance;

        mediatorDrained = new();
        foreach (var player in manager.Mediators.Values)
        {
            AddToDictionary(player);
        }
        manager.CharacterRegistered += AddToDictionary;
    }

    private void AddToDictionary(CharacterMediator character)
    {
        if (character == owner) return;
        mediatorDrained.Add(character, false);
    }

    private void OnDamageDealt(int damage, DamageTag tag, CharacterMediator hitMediator)
    {
        if (tag != DamageTag.Shot) return; // just in case

        // this player has already been drained or is an NPC
        if (mediatorDrained[hitMediator]
            && !hitMediator.IsNPC) return;

        var hc = hitMediator.HealthComponent;
        if (hc.CurrentHealth <= healthThreshold && hc.CanTakeDamage)
        {
            mediatorDrained[hitMediator] = true;
            ExecuteAfterDelay(hitMediator);
        }
    }

    private void OnKill(CharacterMediator killedMediator, CharacterMediator killer)
    {
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
        if (mediatorDrained is null) return;
        foreach (var key in mediatorDrained.Keys.ToList())
        {
            mediatorDrained[key] = false;
        }
    }

    protected override string _GetAbilitySpecificStats()
    {
        return $"Health threshold: {healthThreshold}\nHeal on kill: {healOnKill}\nDelay: {beforeKillDelay}";
    }
}
