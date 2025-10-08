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

    public NetworkVariable<FixedString32Bytes> PlayerName = new(writePerm: NetworkVariableWritePermission.Owner);

    public static event Action<CharacterMediator> OwnerSpawned;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            inputHandler.enabled = false;
            PlayerName.OnValueChanged += (oldName, newName) =>
            {
                Debug.Log($"Player {OwnerClientId} is now named {newName}");
            };
        }
        else
        {
            mediator.PlayerVision.gameObject.SetActive(true);
            PlayerName.Value = DataStorage.Instance.GetString(DataKeyString.Name);
            inputHandler.Init();
            OwnerSpawned?.Invoke(mediator);
            mediator.PlayerVision.GetEnabled(false);
        }

        CharacterManager.Instance.RegisterCharacter(mediator);
    }
    #region damage

    public void DealDamage(int damage, CharacterMediator targetMediator)
    {
        if (!targetMediator.HealthComponent.CanTakeDamage) return;
        DealDamageRequestRpc(damage, targetMediator.PlayerId);
    }
    public void TakeLethalDamage()
    {
        if (!mediator.HealthComponent.CanTakeDamage) return;
        DealDamageRequestRpc(mediator.HealthComponent.CurrentHealth, mediator.PlayerId);
    }

    [Rpc(SendTo.Server)]
    private void DealDamageRequestRpc(int damage, ulong targetID, RpcParams rpcParams = default)
    {
        NotifyDealDamageRpc(damage, targetID);
    }

    [Rpc(SendTo.Everyone)]
    private void NotifyDealDamageRpc(int damage, ulong targetID, RpcParams rpcParams = default)
    {
        var player = CharacterManager.Instance.Players[targetID];
        //Debug.Log($"dealing {damage} damage to {targetID}");
        player.HealthComponent.TakeDamage(damage, mediator);
    }
    [Rpc(SendTo.Server)]
    public void RequestHealRpc(int healAmount)
    {
        NotifyHealRpc(healAmount);
    }
    [Rpc(SendTo.Everyone)]
    private void NotifyHealRpc(int healAmount)
    {
        mediator.HealthComponent.CurrentHealth.Adjust(healAmount);
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
        var player = CharacterManager.Instance.Players[playerId];
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
    [Rpc(SendTo.Server, RequireOwnership = true)]
    public void RequestPingRpc(Vector2 pingPos, ulong playerId)
    {
        if (!GameStateManager.Instance.GameInProgress) return;

        var mediator = CharacterManager.Instance.Players[playerId];
        var teamMateIds = mediator.AbilityRPCs.GetTeamMateIds();
        ClientPingRpc(pingPos, GetRpcParams(teamMateIds));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void ClientPingRpc(Vector2 pingPos, RpcParams rpcParams = default)
    {
        Instantiate(ping, pingPos, Quaternion.identity);
    }
    #endregion

    public override void OnDestroy()
    {
        base.OnDestroy();
        OwnerSpawned = null;
    }

}
