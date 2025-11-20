using Unity.Netcode;
using UnityEngine;

public class GameStateManagerRPCs : NetworkBehaviour
{
    [SerializeField] private CharacterManager characterManager;
    public override void OnNetworkSpawn() => GameStateManager.Instance.SetRPCs(this);

    [Rpc(SendTo.Everyone)]
    public void RequestGameStartRPC(ulong[] sortedIDs)
    {
        GameStateNotifications.Instance.ShowMessage("Starting soon...");
        characterManager.AllPlayersPickedATeam(sortedIDs);
    }
}
