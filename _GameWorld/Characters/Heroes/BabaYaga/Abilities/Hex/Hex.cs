
using UnityEngine;

[CreateAssetMenu(fileName = "Hex", menuName = "Abilities/Passive/Hex")]
public class Hex : Ability
{

    [field: SerializeField] public float AmmoDrainPercentage { get; private set; } = 0.1f;

    protected override void AbstractReset() { }

    protected override void SetUp()
    {
        owner.HealthComponent.DamageTaken += OnDamageTaken;
    }

    private void OnDamageTaken(int damage, CharacterMediator attacker)
    {
        if (owner.AbilityRPCs is BabaYagaRPCs rpcs)
        {
            rpcs.RequestHexRPC(attacker.PlayerId);
        }
        else
        {
            Debug.LogError("Wrong rpcs assigned.");
        }
    }
}
