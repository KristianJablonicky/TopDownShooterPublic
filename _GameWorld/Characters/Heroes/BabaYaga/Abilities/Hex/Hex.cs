
using UnityEngine;

[CreateAssetMenu(fileName = "Hex", menuName = "Abilities/Passive/Hex")]
public class Hex : PassiveAbility
{

    [field: SerializeField] public float AmmoDrainPercentage { get; private set; } = 0.1f;

    protected override void AbstractReset() { }

    protected override void SetUp()
    {
        owner.HealthComponent.DamageTaken += OnDamageTaken;
    }


    private void OnDamageTaken(int damage, CharacterMediator attacker)
    {
        TryInvokeRPC<BabaYagaRPCs>(rpcs => rpcs.RequestHexRPC(attacker.PlayerId));
    }
    protected override string _GetAbilitySpecificStats()
    {
        return $"Magazine ammo drain: {Mathf.RoundToInt(AmmoDrainPercentage * 100f)}%";
    }
}
