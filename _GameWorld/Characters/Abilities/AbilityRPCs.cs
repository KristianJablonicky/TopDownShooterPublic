using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class AbilityRPCs : NetworkBehaviour
{
    [SerializeField] protected CharacterMediator ownerMediator;
    [SerializeField] protected AbilityManager abilityManager;
    public override void OnNetworkSpawn()
    {
        abilityManager.SetCharacterRPCs(this);
    }

    [Rpc(SendTo.Server)]
    public void RequestPlaySoundRpc(AbilityType type, int index)
    {
        PlaySoundRpc(type, index);

    }
    [Rpc(SendTo.Server)]
    public void RequestPlaySoundRpc(AbilityType type)
    {
        PlaySoundRpc(type);
    }

    [Rpc(SendTo.Everyone)]
    private void PlaySoundRpc(AbilityType type, int index)
    {
        var ability = abilityManager.GetAbility(type);
        ability.RpcInvokedPlaySound(index);
    }

    [Rpc(SendTo.Everyone)]
    private void PlaySoundRpc(AbilityType type)
    {
        var ability = abilityManager.GetAbility(type);
        ability.RpcInvokedPlaySound(null);
    }

    public RpcParams GetRpcParams(ulong[] ids) => ownerMediator.NetworkInput.GetRpcParams(ids);
    public RpcParams GetRpcParams(ulong id) => ownerMediator.NetworkInput.GetRpcParams(id);
    public ulong[] MediatorsToIds(List<CharacterMediator> mediators) => ownerMediator.NetworkInput.MediatorsToIds(mediators);

    private ulong[] alliedUnits;
    private ulong[] enemies;
    public ulong[] GetTeamMateIds()
    {
        alliedUnits ??= CharacterManager.Instance.GetAlliedUnitsOf(ownerMediator)
            .Select(p => p.Mediator.PlayerId).ToArray();
        return alliedUnits;
    }
    public ulong[] GetEnemyIds()
    {
        enemies ??= CharacterManager.Instance.Enemies.Select(p => p.Mediator.PlayerId).ToArray();
        return enemies;
    }

    [Rpc(SendTo.Server)]
    public void RequestApplyForceRPC(ulong id, Vector2 force)
    {
        ApplyForceRPC(force, GetRpcParams(id));
    }

    [Rpc(SendTo.Server)]
    public void RequestApplyForceRPC(ulong[] ids, Vector2 force)
    {
        ApplyForceRPC(force, GetRpcParams(ids));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void ApplyForceRPC(Vector2 force, RpcParams rpcParams = default)
    {
        var mediator = CharacterManager.Instance.LocalPlayerMediator;
        if (!mediator.IsAlive) return;

        mediator.MovementController.ApplyForce(force);
    }

    /*
    [Rpc(SendTo.Server)]
    public void RequestXXXRPC()
    {
        ClientXXXRPC();
    }

    [Rpc(SendTo.Everyone)]
    private void ClientXXXRPC()
    {
        // ...
    }
    */
}
