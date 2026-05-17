using System.Collections;
using Unity.Netcode;
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
            var teamMate = caster.GetTeamMate();

            var ids = new ulong[] { casterID, teamMate.PlayerId };
            if (teamMate.IsNpc)
            {
                ClientWishRPC(GetRpcParams(caster.PlayerId));
                Wish(teamMate);
            }
            else
            {
                ClientWishRPC(GetRpcParams(ids));
            }
            WishVisualsRPC(ids);
            return;
        }
        
        WishVisualsRPC(caster.PlayerId);
        ClientWishRPC(GetRpcParams(caster.PlayerId));
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

    private void Wish(CharacterMediator mediator)
    {
        if (!mediator.IsAlive) return;
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
        if (!mediator.IsAlive) return;
        Instantiate(wish.WishVisuals,
            mediator.MovementController.transform
        );
    }

    [Rpc(SendTo.Everyone)]
    public void FingerGunRpc(ulong senderId)
    {
        var mediator = CharacterManager.GetCharacterMediator(senderId);
        SpawnFingerGun(mediator);

        var teamMate = mediator.GetTeamMate();
        if (teamMate != null)
        {
            SpawnFingerGun(teamMate);
        }
    }

    private void SpawnFingerGun(CharacterMediator mediator)
    {
        if (!mediator.IsAlive) return;
        var instance = Instantiate(wish.FingerGunVisuals, mediator.GetTransform());
        instance.PlayAnimation(wish.ChannelingDuration);
    }

    [Rpc(SendTo.Server)]
    public void RequestReleaseDjinnRPC(ulong summonerId, bool postMortem)
    {
        var summoner = CharacterManager.Instance.Mediators[summonerId];
        var djinnInstance = Instantiate(djinn, summoner.GetPosition(), Quaternion.identity);
        if (!summoner.IsNpc)
        {
            djinnInstance.NetworkObject.SpawnWithOwnership(summonerId);
        }
        else
        {
            djinnInstance.NetworkObject.Spawn();
            djinnInstance.SetUp(summonerId);
            summoner = CharacterManager.Instance.LocalPlayerMediator;
        }

        if (!postMortem)
        {
            StartRubbingRpc(summonerId);
            //StartCoroutine(DespawnDjinnAfterDelay(djinnInstance));
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

    [Rpc(SendTo.Everyone)]
    private void StartRubbingRpc(ulong rubberId)
    {
        var rubbingMediator = CharacterManager.GetCharacterMediator(rubberId);
        if (rubbingMediator.AbilityManager.UtilityAbility is ReleaseDjinn ability)
        {
            ability.StartRubbing();
        }
    }

    private IEnumerator DespawnDjinnAfterDelay(DjinnSummoned instance)
    {
        yield return new WaitForSeconds(releaseData.Duration + releaseData.FlyBackDuration + 0.1f);
        DespawnDjinn(instance);
    }

    public void DespawnDjinn(DjinnSummoned instance)
    {
        instance?.NetworkObject.Despawn(true);
    }
}
