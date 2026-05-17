using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamCreationManager : SingletonMonoBehaviour<TeamCreationManager>
{
    [SerializeField] private float checkDelay = 1.0f;

    public event Action<CharacterMediator> VotedForBots;
    public event Action<ulong, ulong> TeamMatePreferenceStated;
    public event Action TeamMateSelectionPhaseStarted;

    private CharacterManager characterManager;
    private int playersThatWantToPlayWithBots = 0;
    private bool votingForBotsEnabled = true;

    // Value:
    // null -> the player has not decided yet
    // Id different to playerID -> Id of the preferred teamMate
    // Id == playerId -> the player is indifferent
    private Dictionary<ulong, ulong?> teamMatePreferences;

    protected override void OverriddenAwake()
    {
        if (DataStorage.IsSinglePlayer) Destroy(gameObject);
    }

    private void Start()
    {
        characterManager = CharacterManager.Instance;
        characterManager.AllPlayersConnected += StartPickingPhase;
    }

    public void VoteForBots(bool addBots)
    {
        if (!votingForBotsEnabled) return;
        playersThatWantToPlayWithBots += addBots ? 1 : -1;
        StopAllCoroutines();
        StartCoroutine(CheckVotingResults());
    }

    private IEnumerator CheckVotingResults()
    {
        yield return new WaitForSeconds(checkDelay);
        if (playersThatWantToPlayWithBots < characterManager.Mediators.Count) yield break;

        BotManager.Instance.FillLobbyWithBots();
        votingForBotsEnabled = false; // just in case
    }

    public void InvokeVotedForBots(ulong playerId)
        => VotedForBots?.Invoke(characterManager.Mediators[playerId]);

    public void InvokeTeamMatePreference(ulong player, ulong preferredTeamMate)
        => TeamMatePreferenceStated?.Invoke(player, preferredTeamMate);
    public void StartPickingPhase()
    {
        teamMatePreferences = new();
        votingForBotsEnabled = false;
        
        foreach (var mediator in characterManager.Mediators.Values)
        {
            var id = mediator.PlayerId;
            if (mediator.IsPlayer)
            {
                teamMatePreferences.Add(id, null);
            }
            else
            {
                // bots are immediately indifferent
                teamMatePreferences.Add(id, id);
                InvokeTeamMatePreference(id, id);
            }
        }

        TeamMateSelectionPhaseStarted?.Invoke();
    }

    private bool teamsWereMade = false;
    public void PlayerRequestedTeamMate(ulong requester, ulong teamMate)
    {
        if (teamsWereMade) return;
        teamMatePreferences[requester] = teamMate;

        // let two coroutines run at the same time,
        // cancel all subsequent when the first match is found
        //StopAllCoroutines();
        StartCoroutine(CheckTeamMateMatch());
    }
    private (ulong, ulong)? match = null;
    private IEnumerator CheckTeamMateMatch()
    {
        yield return new WaitForSeconds(checkDelay);
        var indifferentPlayers = 0;
        var diningPhilosophersCount = 0;
        var allPlayersReady = true;
        foreach (var preference in teamMatePreferences)
        {
            if (!preference.Value.HasValue)
            {
                allPlayersReady = false;
            }
            if (!IsIndifferent(preference.Key))
            {
                diningPhilosophersCount++;
                if (Matching(preference))
                {
                    if (match.HasValue) continue;
                    match = (preference.Key, preference.Value.Value);
                }
            }
            else
            {
                indifferentPlayers++;
            }
        }
        if (!allPlayersReady) yield break;

        if (match.HasValue)
        {
            characterManager.APairWasMade(match.Value);
        }
        else if (indifferentPlayers >= Constants.maxPlayerCount // all players ready, 0 preferences stated, create teams for them
        ||  diningPhilosophersCount >= Constants.maxPlayerCount) // everyone has a preference, but sadly no match was made
        {
            characterManager.ShuffleTeams();
        }
        TeamsWereMade();
    }

    private void TeamsWereMade()
    {
        StopAllCoroutines();
        teamsWereMade = true;
        Destroy(gameObject, 5f);
    }
    private bool IsIndifferent(ulong playerId)
        => playerId == teamMatePreferences[playerId];

    private bool Matching(KeyValuePair<ulong, ulong?> preference)
    {
        if (!preference.Value.HasValue) return false; // the player hasn't decided yet

        var preferredTeamMatePreference = teamMatePreferences[preference.Value.Value];
        if (!preferredTeamMatePreference.HasValue) return false; // the player's preferred teamMate has not decided yet
        return preference.Key == preferredTeamMatePreference.Value // both players prefer to play together
            || preference.Value == preferredTeamMatePreference.Value;
    }
}
