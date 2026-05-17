using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// This class is split into:
/// PlayerNetworkUtilities
/// PlayerNetworkDamage
/// PlayerNetworkShooting
/// PlayerNetworkPrediction
/// PlayerNetworkAnimations
/// PlayerNetworkTeamSelection
/// PlayerNetworkObjectives
/// </summary>
public partial class PlayerNetworkInput : NetworkBehaviour
{
    [SerializeField] private CharacterMediator mediator;
    [SerializeField] private PlayerInputHandler inputHandler;
    [SerializeField] private Gun gun;

    [SerializeField] private Ping ping;

    [SerializeField] private bool NPC = false;

    public NetworkVariable<FixedString32Bytes> PlayerName = new(writePerm: NetworkVariableWritePermission.Owner);

    public static event Action<CharacterMediator> PlayerSpawned;

    public Action<EmoteType> Emoted, EmotedPrivately;

    public override void OnNetworkSpawn()
    {
        if (!mediator.IsTrainingDummy
        &&  !mediator.IsBot) SetUpPlayer();
        else SetUpNPC();
    }

    private void SetUpPlayer()
    {
        mediator.SetID(OwnerClientId);
        if (!IsOwner)
        {
            inputHandler.enabled = false;
            mediator.RotationController.enabled = false;
        }
        else
        {
            mediator.PlayerVision.gameObject.SetActive(true);
            PlayerName.Value = DataStorage.Instance.GetString(DataKeyString.Name);
            inputHandler.Init();
            PlayerSpawned?.Invoke(mediator);
            mediator.PlayerVision.GetEnabled(false);
        }
        SetUpPrediction(IsOwner);
        CharacterManager.Instance.RegisterCharacter(mediator);
    }

    public void SetName(string name) => PlayerName.Value = name;

    private static ulong botOffset = 0;
    private void SetUpNPC()
    {
        //mediator.SetID(Constants.NPCID + botOffset);
        mediator.SetID((ulong)CharacterManager.Instance.Mediators.Count);
        botOffset++;
        if (mediator.IsBot)
        {
            SetUpPrediction(IsOwner);
        }
        CharacterManager.Instance.RegisterCharacter(mediator);
    }

    #region misc
    [Rpc(SendTo.Server)]
    public void RequestPingRpc(Vector2 pingPos, AimDirection aimDirection, ulong playerId)
    {
        var mediator = CharacterManager.Instance.Mediators[playerId];
        ClientPingRpc(pingPos, aimDirection, GetRpcParams(mediator.PlayerId));

        if (GameStateManager.Instance.GameInProgress)
        {
            var teamMate = mediator.GetTeamMate();
            var ai = teamMate.AiDecisions;
            if (ai != null && teamMate.IsAlive)
            {
                var mediatorDecidingFloor = mediator.IsAlive ? mediator : teamMate;
                var targetFloor = FloorUtilities.GetTargetFloor(mediatorDecidingFloor, aimDirection);
                var targetPos = FloorUtilities.TransformPositionIfNeeded(pingPos, targetFloor);
                ai.GoToPosition(targetPos);
            }
            else
            {
                ClientPingRpc(pingPos, aimDirection, GetRpcParams(teamMate.PlayerId));
            }
        }
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void ClientPingRpc(Vector2 pingPos, AimDirection aimDirection, RpcParams rpcParams = default)
    {
        var pingInstance = Instantiate(ping, pingPos, Quaternion.identity);
        pingInstance.Init(aimDirection);
    }

    [Rpc(SendTo.Everyone)]
    public void EmoteRpc(ulong emotingPlayerId, EmoteType emoteType)
    {
        var mediator = CharacterManager.GetCharacterMediator(emotingPlayerId);
        mediator.NetworkInput.Emoted?.Invoke(emoteType);
    }

    public void EmoteTeam(ulong playerId, ulong teamMateId, EmoteType emoteType)
    {
        var targetIds = new ulong[] { playerId, teamMateId };
        TeamMateEmoteRpc(emoteType, GetRpcParams(targetIds));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void TeamMateEmoteRpc(EmoteType emoteType, RpcParams rpcParams)
    {
        EmotedPrivately?.Invoke(emoteType);
    }


    [Rpc(SendTo.Server)]
    public void RequestAbilityCastRpc(AbilityType abilityType, bool pushedDown, Vector2 cursorPosition)
    {
        var ability = (ActiveAbility)mediator.GetTeamMate().AbilityManager.GetAbility(abilityType);
        if (!ability.ReadyToCast) return;

        ability.OnKeyInteraction(pushedDown, cursorPosition);
    }
    #endregion

    public override void OnDestroy()
    {
        base.OnDestroy();
        
        if (!IsOwner) return;
        PlayerSpawned = null;
    }

}
