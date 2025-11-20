using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class BabaYagaRPCs : AbilityRPCs
{
    [SerializeField] private Curse curse;
    [SerializeField] private Hex hex;

    [Rpc(SendTo.Server)]
    public void RequestSweepCurseRPC(ulong invokerId)
    {
        var targetsHit = new List<CharacterMediator>();
        var manager = CharacterManager.Instance;
        var caster = manager.Mediators[invokerId];

        ClientSweepRPC(invokerId);

        foreach (var character in manager.Mediators.Values)
        {
            if (character == caster) continue;

            if (!caster.InRange(character, curse.Range, true)) continue;
            if (!caster.LookingAt(character, curse.ArcDegrees)) continue;

            targetsHit.Add(character);
        }

        if (targetsHit.Count == 0) return;

        var ids = MediatorsToIds(targetsHit);
        ClientCurseRPC(GetRpcParams(ids));
    }


    [Rpc(SendTo.Server)]
    public void RequestCurseRPC(ulong invokerId)
    {
        if (!GameStateManager.Instance.GameInProgress)
        {
            ClientCurseWarmUpRpc(invokerId);
            return;
        }

        var manager = CharacterManager.Instance;
        var invoker = manager.Mediators[invokerId];
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

        new Modifier
        (
            mediator,
            new Curse.CurseModifier(curse.NearSightedMultiplier),
            curse.Duration,
            1,
            curse.Icon
        );
    }

    [Rpc(SendTo.Everyone)]
    private void ClientSweepRPC(ulong casterId)
    {
        var caster = CharacterManager.Instance.Mediators[casterId];
        GetDust(caster);
        var dustOtherFloor = GetDust(caster);
        dustOtherFloor.transform.position = FloorUtilities.GetPositionOnTheOtherFloor(dustOtherFloor.transform.position);
    }

    private MonoBehaviour GetDust(CharacterMediator caster)
    {
        var dust = Instantiate(curse.SweepGO, caster.GetPosition(),
            caster.RotationController.transform.rotation);
        dust.transform.localScale = Vector2.one * curse.Range;
        return dust;
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
