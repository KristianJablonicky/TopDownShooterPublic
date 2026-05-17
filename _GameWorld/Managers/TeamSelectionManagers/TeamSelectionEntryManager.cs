using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TeamSelectionEntryManager : MonoBehaviour
{
    [SerializeField] private TeamSelectionEntry[] entries;
    [SerializeField] private TMP_Text hintText;
    private int currentIndex = 0;
    private Dictionary<ulong, TeamSelectionEntry> entryDictionary;

    private void Start()
    {
        entryDictionary = new();
        var manager = CharacterManager.Instance;
        foreach (var mediator in manager.Mediators.Values)
        {
            SetUp(mediator);
        }

        manager.CharacterRegistered += SetUp;

        var teamManager = TeamCreationManager.Instance;
        teamManager.VotedForBots += VotedForBots;
        teamManager.TeamMatePreferenceStated += SetPreference;
        teamManager.TeamMateSelectionPhaseStarted += SelectionPhaseStarted;
        //teamManager.TeamsCreated += FadeOut;
        GameStateManager.Instance.GameStarted += FadeOut;
    }

    private void SetUp(CharacterMediator mediator)
    {
        var entry = entries[currentIndex];
        entryDictionary.Add(mediator.PlayerId, entry);
        entry.SetUp(mediator);
        currentIndex++;
    }
    private void VotedForBots(CharacterMediator mediator)
    {
        entryDictionary[mediator.PlayerId].VotedForBots();
    }

    private void SetPreference(ulong player, ulong preference)
    {
        entryDictionary[player].SetPreference(preference);
    }
    private void SelectionPhaseStarted()
    {
        hintText.text = "Pick your\nteammate";
        foreach (var entry in entries)
        {
            entry.StartTeamMateSelectionPhase();
        }
    }

    private void FadeOut()
    {
        hintText.text = string.Empty;
        foreach (var entry in entries)
        {
            entry.FadeOut();
        }
    }
}
