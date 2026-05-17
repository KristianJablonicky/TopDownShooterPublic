
using UnityEngine;

[CreateAssetMenu(fileName = "Hex", menuName = "Abilities/Passive/Hex")]
public class Hex : PassiveAbility
{
    [field: SerializeField] public float AmmoDrainPercentage { get; private set; } = 0.1f;
    [Header("Visuals")]
    [field: SerializeField] public HexVisuals VisualsPrefab { get; private set; }
    [field: SerializeField] public AudioClip WhenHexedSound { get; private set; }
    protected override void AbstractReset() { }

    protected override void SetUp() { }
    protected override void SafeSetUpWithRPCsReady()
    {
        owner.HealthComponent.DamageTakenFromMediator += OnDamageTaken;
    }


    private void OnDamageTaken(int damage, CharacterMediator attacker)
    {
        if (attacker == owner) return;
        TryInvokeRPC<BabaYagaRPCs>(rpcs => rpcs.RequestHexRPC(owner.PlayerId, attacker.PlayerId));
    }

    public override string _GetSpecificAttributes()
    {
        return $"Magazine ammo drain: {Mathf.RoundToInt(AmmoDrainPercentage * 100f)}%";
    }
}
