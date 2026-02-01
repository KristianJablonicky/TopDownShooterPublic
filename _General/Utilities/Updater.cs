using System;
using UnityEngine;

public class Updater : SingletonMonoBehaviour<Updater>
{
    public event Action<float> Updated, UpdatedDuringRound;
    private bool roundStarted = false;

    private void Start()
    {
        var gameState = GameStateManager.Instance;
        gameState.NewRoundStarted += () => roundStarted = true;
        gameState.RoundEnded += () => roundStarted = false;
    }
    private void Update()
    {
        Updated?.Invoke(Time.deltaTime);
        if (roundStarted)
        {
            UpdatedDuringRound?.Invoke(Time.deltaTime);
        }
    }
}
