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

    public RpcParams GetRpcParams(ulong[] ids) => ownerMediator.NetworkInput.GetRpcParams(ids);
    public RpcParams GetRpcParams(ulong id) => ownerMediator.NetworkInput.GetRpcParams(id);
    public ulong[] MediatorsToIds(List<CharacterMediator> mediators) => ownerMediator.NetworkInput.MediatorsToIds(mediators);

    private ulong[] alliedUnits;
    private ulong[] enemies;
    public ulong[] GetTeamMateIds()
    {
        alliedUnits ??= CharacterManager.Instance.AlliedUnits.Select(p => p.Mediator.PlayerId).ToArray();
        return alliedUnits;
    }

    public ulong[] GetEnemyIds()
    {
        enemies ??= CharacterManager.Instance.Enemies.Select(p => p.Mediator.PlayerId).ToArray();
        return enemies;
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
