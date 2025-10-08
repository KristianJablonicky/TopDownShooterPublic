using System.Collections;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class BabaYagaRPCs : AbilityRPCs
{
    [SerializeField] private Curse curse;
    [SerializeField] private Hex hex;

    [Rpc(SendTo.Server)]
    public void RequestCurseRPC(ulong invokerId)
    {
        if (!GameStateManager.Instance.GameInProgress)
        {
            ClientCurseWarmUpRpc(invokerId);
            return;
        }

        var manager = CharacterManager.Instance;
        var invoker = manager.Players[invokerId];
        var opponents = manager.EnemiesOf(invoker);

        var opponentIds = opponents.Select(p => p.Mediator.PlayerId).ToArray();

        ClientCurseRPC(GetRpcParams(opponentIds));
    }

    [Rpc(SendTo.Everyone)]
    private void ClientCurseWarmUpRpc(ulong invokerId)
    {
        if (CharacterManager.Instance.LocalPlayerMediator.PlayerId == invokerId) return;
        CurseLocalPlayer();
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void ClientCurseRPC(RpcParams rpcParams = default)
    {
        CurseLocalPlayer();
    }

    private void CurseLocalPlayer()
    {
        var mediator = CharacterManager.Instance.LocalPlayerMediator;
        if (!mediator.IsAlive) return;
        
        var duration = curse.Duration;
        mediator.AbilityManager.DisableAbilities(duration);
        mediator.PlayerVision.SetVisionRangeProportional(curse.NearSightedMultiplier);

        StartCoroutine(CureCurse(mediator, duration));

    }

    private IEnumerator CureCurse(CharacterMediator mediator, float duration)
    {
        yield return new WaitForSeconds(duration);
        if (mediator.IsAlive)
        {
            mediator.PlayerVision.Reset();
        }
    }

    [Rpc(SendTo.Server)]
    public void RequestHexRPC(ulong targetId)
    {
        ClientHexRPC(GetRpcParams(targetId));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void ClientHexRPC(RpcParams rpcParams = default)
    {
        var manager = CharacterManager.Instance.LocalPlayerMediator.Gun.ShootManager;
        manager.AdjustAmmo(
            -Mathf.CeilToInt(manager.GetCapacity() * hex.AmmoDrainPercentage)
        );
    }
}
