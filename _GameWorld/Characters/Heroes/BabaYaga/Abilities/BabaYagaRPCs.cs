using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BabaYagaRPCs : AbilityRPCs
{
    [SerializeField] private Curse curse;
    [SerializeField] private CursePostMortem cursePostMortem;
    [SerializeField] private Hex hex;

    [Rpc(SendTo.Server)]
    public void RequestSweepCurseRPC(ulong invokerId)
    {
        var hitPlayerIds = new List<ulong>();
        var manager = CharacterManager.Instance;
        var caster = manager.Mediators[invokerId];

        ClientSweepRPC(invokerId);

        foreach (var character in manager.Mediators.Values)
        {
            if (character == caster
                || !character.IsAlive) continue;

            if (!caster.InRange(character, curse.Range, true)) continue;
            if (!caster.LookingAt(character, curse.ArcDegrees, true)) continue;

            hitPlayerIds.Add(character.PlayerId);
        }
        var targetsHitCount = hitPlayerIds.Count;
        if (targetsHitCount == 0) return;

        //ClientCurseRPC(GetRpcParams(ids));
        ClientCurseRPC(hitPlayerIds.ToArray());

        if (!caster.IsBot)
        {
            TargetsCursedRpc(targetsHitCount, GetRpcParams(invokerId));
        }
    }
    [Rpc(SendTo.SpecifiedInParams)]
    private void TargetsCursedRpc(int targetsHitCount, RpcParams rpcParams = default)
    {
        var caster = CharacterManager.Instance.Mediators[rpcParams.Receive.SenderClientId];
        var curseInstance = (Curse)caster.AbilityManager.UtilityAbility;
        curseInstance.PlayCurseSounds(targetsHitCount);
    }

    [Rpc(SendTo.Server)]
    public void RequestCursePostMortemRPC(ulong teamMateId)
    {
        if (!GameStateManager.Instance.GameInProgress)
        {
            //ClientCurseWarmUpRpc(playerId);
            return;
        }

        var manager = CharacterManager.Instance;
        var invoker = manager.Mediators[teamMateId];
        var opponents = manager.EnemiesOf(invoker);
        List<PlayerData> hitOpponents = new();
        foreach (var opponent in opponents)
        {
            if (!opponent.Mediator.IsAlive) continue;

            if (invoker.InRange(opponent.Mediator, cursePostMortem.Range, false))
            {
                hitOpponents.Add(opponent);
            }
        }

        ClientCursePostMortemVisualsRpc(teamMateId);
        if (hitOpponents.Count == 0) return;

        ulong[] cursedOpponentIds = new ulong[hitOpponents.Count];
        for (int i = 0; i < hitOpponents.Count; i++)
        {
            cursedOpponentIds[i] = hitOpponents[i].Mediator.PlayerId;
        }
        
        //ClientCurseRPC(GetRpcParams(cursedOpponentIds));
        ClientCurseRPC(cursedOpponentIds);
    }

    [Rpc(SendTo.Everyone)]
    private void ClientCursePostMortemVisualsRpc(ulong cursingTeamMate)
    {
        var visuals = Instantiate(cursePostMortem.curseVisuals,
            CharacterManager.Instance.Mediators[cursingTeamMate].GetPosition(),
            Quaternion.identity);
        visuals.transform.localScale = Vector2.one * cursePostMortem.Range;
    }

    private Modifier CursePlayer(CharacterMediator cursedMediator)
    {
        return new Modifier
        (
            cursedMediator,
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

    [Rpc(SendTo.Everyone)]
    private void ClientCurseRPC(ulong[] cursedCharacters)
    {
        var manager = CharacterManager.Instance;
        foreach (var id in cursedCharacters)
        {
            var mediator = manager.Mediators[id];
            if (!mediator.IsAlive) continue;
            var curseModifier = CursePlayer(mediator);
            var debuff = Instantiate(curse.DebuffVisuals);
            debuff.SetUp(mediator, curseModifier);

            if (mediator.IsLocalPlayer)
            {
                mediator.SoundPlayer.RequestPlaySound(mediator.GetTransform(), curse.WhenCursedDebuffSound, false);
            }
        }
    }

    private GameObject GetDust(CharacterMediator caster)
    {
        var dust = Instantiate(curse.SweepGO, caster.GetPosition(),
            caster.RotationController.transform.rotation);
        dust.transform.localScale = Vector2.one * curse.Range;
        return dust;
    }

    [Rpc(SendTo.Server)]
    public void RequestHexRPC(ulong sourceId, ulong targetId)
    {
        ClientHexRPC(GetRpcParams(targetId));
        ClientHexVisualsRPC(sourceId, targetId);
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void ClientHexRPC(RpcParams rpcParams = default)
    {
        var manager = CharacterManager.Instance.LocalPlayerMediator.Gun.ShootManager;
        manager.AdjustAmmo(
            -Mathf.CeilToInt(manager.GetCapacity() * hex.AmmoDrainPercentage)
        );
    }

    [Rpc(SendTo.Everyone)]
    private void ClientHexVisualsRPC(ulong sourceId, ulong targetId)
    {
        var manager = CharacterManager.Instance;
        var source = manager.Mediators[sourceId];
        var target = manager.Mediators[targetId];
        var instance = Instantiate(hex.VisualsPrefab);
        var sourcePos = source.GetPosition();
        var targetPos = target.GetPosition();
        sourcePos = FloorUtilities.TranslateToTheOtherFloorIfNeeded(sourcePos, targetPos);
        instance.Init(sourcePos, targetPos);

        if (target.IsLocalPlayer) target.SoundPlayer.RequestPlaySound(target.GetTransform(), hex.WhenHexedSound, false);
    }
}
