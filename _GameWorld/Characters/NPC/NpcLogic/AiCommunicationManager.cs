using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiCommunicationManager : SingletonMonoBehaviour<AiCommunicationManager>
{
    [SerializeField] private NpcDifficultyConfig config;
    [SerializeField] private float loseInterestToForgetPositionMultiplier = 2f;

    private Dictionary<BotTeam, LastSeenPositionData> positionsDict;


    protected override void OverriddenAwake()
    {
        positionsDict = new()
        {
            { BotTeam.Orange, new() },
            { BotTeam.Cyan, new() },
            { BotTeam.AgainstLocalPlayer, new() }
        };
    }

    public void EnemySeenAtPosition(Vector2 position, BotTeam team)
    {
        var positionData = positionsDict[team];
        if (positionData.forgetCoroutine != null) StopCoroutine(positionData.forgetCoroutine);
        positionData.lastSeenPosition = position;
        positionData.forgetCoroutine = StartCoroutine(ForgetPlayerPosition(positionData));
    }
    private IEnumerator ForgetPlayerPosition(LastSeenPositionData positionData)
    {
        yield return new WaitForSeconds(loseInterestToForgetPositionMultiplier
            * config.LoseInterestDuration);
        positionData.lastSeenPosition = null;
    }
    public Vector2? EnemyLastSeenPosition(BotTeam team)
        => positionsDict[team].lastSeenPosition;

    private class LastSeenPositionData
    {
        public Vector2? lastSeenPosition;
        public Coroutine forgetCoroutine;
    }
}

public enum BotTeam
{
    Orange = Team.Orange,
    Cyan = Team.Cyan,
    AgainstLocalPlayer
}
