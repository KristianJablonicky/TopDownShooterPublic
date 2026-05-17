using Unity.Netcode;
using UnityEngine;

public partial class PlayerNetworkInput : NetworkBehaviour
{
    [Rpc(SendTo.Server)]
    public void RequestReloadRpc()
    {
        ClientReloadRpc();
    }

    [Rpc(SendTo.Everyone)]
    public void ClientReloadRpc()
    {
        mediator.Gun.Reload();
    }


    [Rpc(SendTo.Server)]
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
}
