using System.Collections;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
public class DjinnRPCs : AbilityRPCs
{
    [SerializeField] private FulfilWish wish;
    [SerializeField] private ReleaseDjinn releaseData;
    [SerializeField] private DjinnSummoned djinn;

    public ObservableValue<int> remainingWishes = new(0);

    [Rpc(SendTo.Server)]
    public void RequestWishRPC(ulong casterID)
    {
        var manager = CharacterManager.Instance;
        var caster = manager.Mediators[casterID];
        if (GameStateManager.Instance.GameInProgress)
        {
            var teamMate = caster.playerData.GetTeamMate();
            var ids = new ulong[] { casterID, teamMate.Mediator.PlayerId };
            ClientWishRPC(GetRpcParams(ids));
            WishVisualsRPC(ids);
        }
        else
        {
            ClientWishRPC(GetRpcParams(caster.PlayerId));
            WishVisualsRPC(caster.PlayerId);
        }
    }

    [Rpc(SendTo.Server)]
    public void RequestWishPostMortemRPC(ulong targetID)
    {
        ClientWishRPC(GetRpcParams(targetID));
        WishVisualsRPC(targetID);
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void ClientWishRPC(RpcParams rpcParams = default)
    {
        Wish(CharacterManager.Instance.LocalPlayerMediator);
    }

    private async void Wish(CharacterMediator mediator)
    {
        mediator.NetworkInput.RequestHealRpc(wish.HealAmount, false);

        mediator.Gun.ChannelingManager.RequestInterrupt();
        mediator.Gun.ShootManager.Reset();

        var modifier = new Modifier
        (
            mediator,
            new FulfilWish.WishModifier(wish.MoveSpeedMultiplier),
            wish.Duration,
            1,
            wish.Icon
        );
    }

    [Rpc(SendTo.Everyone)]
    private void WishVisualsRPC(ulong[] ids)
    {
        var manager = CharacterManager.Instance;
        foreach (var id in ids)
        {
            WishVisuals(manager.Mediators[id]);
        }
    }

    [Rpc(SendTo.Everyone)]
    private void WishVisualsRPC(ulong id)
    {
        WishVisuals(CharacterManager.Instance.Mediators[id]);
    }

    private void WishVisuals(CharacterMediator mediator)
    {
        Instantiate(wish.WishVisuals,
            mediator.MovementController.transform
        );
    }

    [Rpc(SendTo.Server)]
    public void  RequestReleaseDjinnRPC(ulong summonerId, bool postMortem)
    {
        var summoner = CharacterManager.Instance.Mediators[summonerId];
        var djinnInstance = Instantiate(djinn, summoner.GetPosition(), Quaternion.identity);
        djinnInstance.NetworkObject.SpawnWithOwnership(summonerId);

        if (!postMortem)
        {
            StartCoroutine(DespawnDjinnAfterDelay(djinnInstance));
        }
        else
        {
            void OnRespawn(CharacterMediator _)
            {
                summoner.Respawned -= OnRespawn;
                DespawnDjinn(djinnInstance);
            }

            summoner.Respawned += OnRespawn;
        }
    }

    private IEnumerator DespawnDjinnAfterDelay(DjinnSummoned instance)
    {
        yield return new WaitForSeconds(releaseData.Duration + releaseData.FlyBackDuration + 0.1f);
        DespawnDjinn(instance);
    }

    private void DespawnDjinn(DjinnSummoned instance)
    {
        instance?.NetworkObject.Despawn(true);
    }
}
