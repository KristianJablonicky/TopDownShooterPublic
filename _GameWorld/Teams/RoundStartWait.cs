using System;
using UnityEngine;

public class RoundStartWait : SingletonMonoBehaviour<RoundStartWait>
{
    [SerializeField] private float delayAttackers = 8f, delayDefenders = 3f;
    public event Action<float> OnRoundStartWait;

    private GameStateManager gameStateManager;

    private CharacterMediator localPlayer;


    private void Start()
    {
        gameStateManager = GameStateManager.Instance;
        gameStateManager.NewRoundStarted += OnNewRound;

        PlayerNetworkInput.PlayerSpawned += (player) => localPlayer = player;
    }

    private void OnNewRound()
    {
        var delay = localPlayer.Role == Role.Attacker ? delayAttackers : delayDefenders;
        DisableActions(delay);
        OnRoundStartWait?.Invoke(delay);
    }

    private void DisableActions(float delay)
    {
        localPlayer.AbilityManager.DisableAbilities(delay);
        localPlayer.Gun.ChannelingManager.StartChannelingStandingStill(delay, null, localPlayer, false);
    }
}
