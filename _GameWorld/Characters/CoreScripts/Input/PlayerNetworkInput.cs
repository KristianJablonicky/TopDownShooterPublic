using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetworkInput : NetworkBehaviour
{
    [SerializeField] private CharacterMediator mediator;
    [SerializeField] private PlayerInputHandler inputHandler;
    [SerializeField] private Gun gun;

    [SerializeField] private GameObject ping;

    [SerializeField] private bool NPC = false;

    public NetworkVariable<FixedString32Bytes> PlayerName = new(writePerm: NetworkVariableWritePermission.Owner);

    public static event Action<CharacterMediator> PlayerSpawned;

    public override void OnNetworkSpawn()
    {
        if (!NPC) SetUpPlayer();
        else SetUpNPC();
    }

    private void SetUpPlayer()
    {
        mediator.SetID(OwnerClientId);
        if (!IsOwner)
        {
            inputHandler.enabled = false;
        }
        else
        {
            mediator.PlayerVision.gameObject.SetActive(true);
            PlayerName.Value = DataStorage.Instance.GetString(DataKeyString.Name);
            inputHandler.Init();
            PlayerSpawned?.Invoke(mediator);
            mediator.PlayerVision.GetEnabled(false);
        }

        CharacterManager.Instance.RegisterCharacter(mediator);
    }

    private static ulong botOffset = 0;
    private void SetUpNPC()
    {
        mediator.SetID(Constants.NPCID + botOffset);
        botOffset++;
        CharacterManager.Instance.RegisterCharacter(mediator);
    }

    #region damage

    public void DealDamage(int damage, DamageTag tag, CharacterMediator targetMediator)
    {
        if (!targetMediator.HealthComponent.CanTakeDamage) return;

        if (!mediator.IsAlive)
        {
            Debug.Log($"Would have dealt {damage} to {targetMediator.PlayerId}, but I am dead :/");
            return;
        }


        if (!targetMediator.IsNPC) DealDamageRequestRpc(damage, tag, targetMediator.PlayerId);
        else targetMediator.HealthComponent.TakeDamage(damage, tag, mediator);
    }
    public void TakeLethalDamage()
    {
        if (!mediator.HealthComponent.CanTakeDamage) return;
        DealDamageRequestRpc(mediator.HealthComponent.CurrentHealth, DamageTag.Neutral, mediator.PlayerId);
    }

    public void TakeDamage(int damage, DamageTag tag)
    {
        DealDamageRequestRpc(damage, tag, mediator.PlayerId);
    }

    [Rpc(SendTo.Server)]
    private void DealDamageRequestRpc(int damage, DamageTag tag, ulong targetID)
    {
        NotifyDealDamageRpc(damage, tag, targetID);
    }

    [Rpc(SendTo.Everyone)]
    private void NotifyDealDamageRpc(int damage, DamageTag tag, ulong targetID)
    {
        var player = CharacterManager.Instance.Mediators[targetID];
        //Debug.Log($"dealing {damage} damage to {targetID}");
        player.HealthComponent.TakeDamage(damage, tag, mediator);
    }
    [Rpc(SendTo.Server)]
    public void RequestHealRpc(int healAmount, bool overHeal)
    {
        if (mediator.IsAlive)
        {
            NotifyHealRpc(healAmount, overHeal);
        }
    }
    [Rpc(SendTo.Everyone)]
    private void NotifyHealRpc(int healAmount, bool overHeal)
    {
        var health = mediator.HealthComponent;
        if (!overHeal)
        {
            health.CurrentHealth.Adjust(healAmount, ceiling: health.MaxHealth);
        }
        else
        {
            health.AdjustMaxHealth(healAmount, true);
        }
    }
    #endregion

    #region utilities
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
    #endregion


    #region shooting

    [Rpc(SendTo.Server, RequireOwnership = true)]
    public void RequestReloadRpc()
    {
        ClientReloadRpc();
    }

    [Rpc(SendTo.Everyone)]
    public void ClientReloadRpc()
    {
        mediator.Gun.Reload();
    }


    [Rpc(SendTo.Server, RequireOwnership = true)]
    public void RequestShootRpc(
        Vector2 cursorPos,
        Vector2 actualShootPosition,
        bool canHeadShot,
        AimDirection aimDirection,
        RpcParams rpcParams = default)
    {
        // Runs on the server
        if (canHeadShot)
        {
            var headShotResult = gun.TryToHeadShot(cursorPos);
            if (headShotResult.HasValue)
            {
                NotifyHeadshotRpc(headShotResult.Value);
                return;
            }
        }
        var source = mediator.GetPosition();
        var direction = (actualShootPosition - mediator.GetPosition()).normalized;
        var sourceToDestinationDistance = Vector2.Distance(source, actualShootPosition);

        if (gun.SingleFloorShot(aimDirection, sourceToDestinationDistance))
        {
            var target = gun.RaycastForDamage(source, direction, null);
            if (target.HasValue)
            {
                NotifyStoppedShotRpc(target.Value);
            }
            else
            {
                NotifyShotRpc(direction);
            }
        }
        else
        {
            // shot travelling through the current floor
            var target = gun.RaycastForDamage(source, direction, actualShootPosition);
            if (target.HasValue)
            {
                NotifyStoppedShotRpc(target.Value);
            }

            // Visiting the other floor
            else
            {
                NotifyStoppedShotRpc(actualShootPosition);

                var floorOffset = FloorUtilities.GetYOffset(aimDirection, mediator.CurrentFloor);
                if (floorOffset.HasValue)
                {
                    var offset2D = new Vector2(0f, floorOffset.Value);

                    var range = Mathf.Min(sourceToDestinationDistance * 2, gun.GunConfig.bulletRange);

                    var destination = source + offset2D + direction * range;

                    source = actualShootPosition + offset2D;

                    target = gun.RaycastForDamage(source, direction, destination, Constants.crossFloorDamageMultiplier);
                    if (target.HasValue)
                    {
                        NotifyStoppedShotRpc(target.Value, source);
                    }
                    else
                    {
                        NotifyStoppedShotRpc(destination, source);
                    }
                }
            }
        }
    }

    [Rpc(SendTo.Everyone)]
    public void ShowGunshotVisualsRPC(ulong playerId)
    {
        var player = CharacterManager.Instance.Mediators[playerId];
        player.Gun.ShowShotVisuals();
    }

    [Rpc(SendTo.Everyone)]
    private void NotifyHeadshotRpc(Vector2 bulletEnd, RpcParams rpcParams = default)
    {
        gun.ShowHeadshotBulletTrail(bulletEnd);
    }

    [Rpc(SendTo.Everyone)]
    private void NotifyShotRpc(Vector2 direction, RpcParams rpcParams = default)
    {
        gun.ShowBulletTrail(direction);
    }

    [Rpc(SendTo.Everyone)]
    private void NotifyStoppedShotRpc(Vector2 target, RpcParams rpcParams = default)
    {
        gun.ShowBulletTrailStopped(target);
    }

    [Rpc(SendTo.Everyone)]
    private void NotifyStoppedShotRpc(Vector2 target, Vector2 source, RpcParams rpcParams = default)
    {
        gun.ShowBulletTrailStopped(target, source, true);
    }
    #endregion

    #region misc
    [Rpc(SendTo.Server)]
    public void RequestPingRpc(Vector2 pingPos, ulong playerId)
    {
        if (!GameStateManager.Instance.GameInProgress) return;

        var mediator = CharacterManager.Instance.Mediators[playerId];
        var teamMateIds = mediator.AbilityRPCs.GetTeamMateIds();
        ClientPingRpc(pingPos, GetRpcParams(teamMateIds));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void ClientPingRpc(Vector2 pingPos, RpcParams rpcParams = default)
    {
        Instantiate(ping, pingPos, Quaternion.identity);
    }
    #endregion

    #region Objectives
    public void RequestBloodPickUp(ulong bloodOwnerId)
    {
        RequestBloodPickUpRpc(mediator.PlayerId, bloodOwnerId);
    }

    [Rpc(SendTo.Server)]
    private void RequestBloodPickUpRpc(ulong playerPickingUpBloodId, ulong bloodOwnerId)
    {
        RequestBloodPickUpClientRpc(playerPickingUpBloodId, bloodOwnerId);
    }

    [Rpc(SendTo.Everyone)]
    private void RequestBloodPickUpClientRpc(ulong playerPickingUpBloodId, ulong bloodOwnerId)
    {
        var manager = CharacterManager.Instance;
        var bloodOwner = manager.Mediators[bloodOwnerId];

        bloodOwner.BloodManager.CleanUpBlood();

        var defenderPickingUp = manager.Mediators[playerPickingUpBloodId];
        defenderPickingUp.BloodManager.PickUpBlood();
    }


    public void RequestObjectiveSacrifice()
    {
        RequestObjectiveSacrificeRpc(mediator.PlayerId);
    }

    [Rpc(SendTo.Server)]
    private void RequestObjectiveSacrificeRpc(ulong sacrificingMediatorId)
    {
        ObjectiveSacrificeClientRpc(sacrificingMediatorId);
    }

    [Rpc(SendTo.Everyone)]
    private void ObjectiveSacrificeClientRpc(ulong sacrificingMediatorId)
    {
        var manager = CharacterManager.Instance;
        var sacrificingMediator = manager.Mediators[sacrificingMediatorId];

        var objective = DefenderObjective.Instance;
        objective.StartSacrifice(sacrificingMediator);
    }

    #endregion

    public override void OnDestroy()
    {
        base.OnDestroy();
        
        if (!IsOwner) return;
        PlayerSpawned = null;
    }

}
