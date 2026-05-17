using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

public partial class PlayerNetworkInput : NetworkBehaviour
{
    public ulong[] MediatorsToIds(List<CharacterMediator> mediators)
    {
        return mediators.Select(m => m.PlayerId).ToArray();
    }
    public RpcParams GetRpcParams(List<CharacterMediator> mediators)
    {
        var ids = mediators.Select(m => m.PlayerId).ToArray();
        return GetRpcParams(ids);
    }

    public RpcParams GetRpcParams(ulong id)
    {
        return new RpcParams
        {
            Send = new RpcSendParams
            {
                Target = RpcTarget.Single(id, RpcTargetUse.Temp)
            }
        };
    }

    public RpcParams GetRpcParams(ulong[] groupIds, RpcTargetUse targetUse = RpcTargetUse.Temp)
    {
        return new RpcParams
        {
            Send = new RpcSendParams
            {
                Target = RpcTarget.Group(groupIds, targetUse)
            }
        };
    }
    private RpcParams GetRpcParams(List<ulong> playerIds)
        => GetRpcParams(playerIds.ToArray());
}
