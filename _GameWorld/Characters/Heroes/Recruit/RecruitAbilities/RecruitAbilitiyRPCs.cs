using Unity.Netcode;
using UnityEngine;

public class RecruitAbilityRPCs : AbilityRPCs
{
    [SerializeField] private Bomb bomb;
    [SerializeField] private DashPostMortem dashPostMortem;

    [Rpc(SendTo.Server)]
    public void RequestBombRPC(ulong ownerId, Vector2 throwVelocity2D)
    {
        ClientBombRPC(ownerId, throwVelocity2D);
    }
    [Rpc(SendTo.Everyone)]
    private void ClientBombRPC(ulong ownerId, Vector2 throwVelocity2D)
    {
        var owner = CharacterManager.Instance.Players[ownerId];
        var bombInstance = Instantiate(bomb, owner.GetPosition(), Quaternion.identity);
        bombInstance.Init(this, owner, throwVelocity2D);
    }

    [Rpc(SendTo.Server)]
    public void RequestExplosionRPC(Vector2 position)
    {
        ClientExplosionRPC(position);
    }

    [Rpc(SendTo.Everyone)]
    private void ClientExplosionRPC(Vector2 position)
    {
        var explosionGO = Instantiate(bomb.Explosion, position, Quaternion.identity);
        explosionGO.transform.localScale = Vector3.one * bomb.ExplosionRadius;
    }

    [Rpc(SendTo.Server)]
    public void RequestExplosionHitRPC(ulong[] ids)
    {
        var target = GetRpcParams(ids);
        ClientExplosionHitRPC(target);
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void ClientExplosionHitRPC(RpcParams rpcParams = default)
    {
        ownerMediator.Gun.ApplyRecoil(bomb.RecoilMultiplierOnHit);
    }




    [Rpc(SendTo.Server)]
    public void RequestAllyDashRPC(ulong teamMate, Vector2 velocity)
    {
        AllyDashRPC(teamMate, velocity);
    }

    [Rpc(SendTo.Everyone)]
    private void AllyDashRPC(ulong teamMate, Vector2 velocity)
    {
        if (NetworkManager.Singleton.LocalClientId == teamMate)
        {
            dashPostMortem.ExecuteDash(
                CharacterManager.Instance.Players[teamMate],
                velocity
            );
        }
    }
}
