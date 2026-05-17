using Unity.Netcode;

public partial class PlayerNetworkInput : NetworkBehaviour
{
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

    [Rpc(SendTo.Everyone)]
    public void ClientObjectiveChannelCompletedRpc(ulong sacrificedMediatorId)
    {
        var mediator = CharacterManager.GetCharacterMediator(sacrificedMediatorId);
        DefenderObjective.Instance.CompleteSacrifice(mediator);
    }
}
