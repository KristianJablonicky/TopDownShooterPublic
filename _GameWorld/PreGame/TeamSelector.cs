using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TeamSelector : SingletonMonoBehaviour<TeamSelector>
{
    [SerializeField] private BoxCollider2D[] teamAreas;
    [SerializeField] private SpriteRenderer[] teamAreaColors;

    public void AllPlayersConnected(CharacterManager characterManager)
    {
        for (int i = 0; i < teamAreas.Length; i++)
        {
            teamAreas[i].gameObject.SetActive(true);
            var color = CommonColors.GetTeamColor(i);
            color.a = 0.25f;
            teamAreaColors[i].color = color;
        }

        if (NetworkManager.Singleton.IsHost)
        {
            StartCoroutine(WaitUntilPlayersPickTeams(characterManager));
        }
    }
    private int secondsStoodInside = 0;
    private IEnumerator WaitUntilPlayersPickTeams(CharacterManager characterManager)
    {
        var wait = new WaitForSeconds(1f);
        while (secondsStoodInside < 2)
        {
            if (AllPlayersInside(characterManager))
            {
                secondsStoodInside++;
            }
            else
            {
                secondsStoodInside = 0;
            }
            yield return wait;
        }
        SortMediators(characterManager);
    }

    private bool AllPlayersInside(CharacterManager manager)
    {
        int[] areaConditionMet = new int[]{0, 0};

        for (int i = 0; i < 2; i++)
        {
            foreach (var player in manager.Mediators.Values)
            {
                if (teamAreas[i].bounds.Contains(player.GetPosition()))
                {
                    areaConditionMet[i]++;
                }
            }
        }
        return areaConditionMet[0] == 2 && areaConditionMet[1] == 2;
    }

    private void SortMediators(CharacterManager characterManager)
    {
        List<ulong> orange, cyan;
        orange = new();
        cyan = new();
        foreach (var character in characterManager.Mediators.Values)
        {
            if (character.GetPosition().x < 0)
            {
                orange.Add(character.PlayerId);
            }
            else
            {
                cyan.Add(character.PlayerId);
            }
        }
        GameStateManager.Instance.AllPlayersPickedATeam(
            new ulong[] {
                orange[0],
                orange[1],
                cyan[0],
                cyan[1]
            }
        );
    }

    public void DestroyAreas()
    {
        foreach (var teamArea in teamAreas)
        {
            Destroy(teamArea.gameObject);
        }
    }
}
