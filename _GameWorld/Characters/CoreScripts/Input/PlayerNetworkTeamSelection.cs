using Unity.Netcode;

public partial class PlayerNetworkInput : NetworkBehaviour
{
    public void VoteForBots()
    {
        VoteForBotsRpc(mediator.PlayerId);
    }

    [Rpc(SendTo.Server)]
    private void VoteForBotsRpc(ulong voter)
    {
        TeamCreationManager.Instance.VoteForBots(true);
        PlayerVotedForBotsRpc(voter);
    }

    [Rpc(SendTo.Everyone)]
    private void PlayerVotedForBotsRpc(ulong voter)
    {
        TeamCreationManager.Instance.InvokeVotedForBots(voter);
    }

    public void SelectTeamMate(ulong teamMateId)
    {
        RequestTeamMateRpc(mediator.PlayerId, teamMateId);
    }

    [Rpc(SendTo.Server)]
    private void RequestTeamMateRpc(ulong requester, ulong teamMate)
    {
        TeamMateRequestedRpc(requester, teamMate);
        TeamCreationManager.Instance.PlayerRequestedTeamMate(requester, teamMate);
    }

    [Rpc(SendTo.Everyone)]
    public void TeamMateRequestedRpc(ulong requester, ulong teamMate)
    {
        TeamCreationManager.Instance.InvokeTeamMatePreference(requester, teamMate);
    }
}
