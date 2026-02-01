
using UnityEngine;

[CreateAssetMenu(fileName = "Hex", menuName = "Abilities/Passive/Hex")]
public class Hex : PassiveAbility
{

    [field: SerializeField] public float AmmoDrainPercentage { get; private set; } = 0.1f;

    protected override void AbstractReset() { }

    protected override void SetUp()
    {
        PlayerNetworkInput.PlayerSpawned += OnLocalPlayerSpawn;
    }

    private void OnLocalPlayerSpawn(CharacterMediator mediator)
    {
        if (mediator != owner) return;
        owner.HealthComponent.DamageTakenFromMediator += OnDamageTaken;
    }


    private void OnDamageTaken(int damage, CharacterMediator attacker)
    {
        if (attacker == owner) return;
        TryInvokeRPC<BabaYagaRPCs>(rpcs => rpcs.RequestHexRPC(attacker.PlayerId));
    }
    protected override string _GetAbilitySpecificStats()
    {
        return $"Magazine ammo drain: {Mathf.RoundToInt(AmmoDrainPercentage * 100f)}%";
    }
}
