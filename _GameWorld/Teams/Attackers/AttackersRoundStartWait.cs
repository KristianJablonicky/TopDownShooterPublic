using System.Collections;
using UnityEngine;

public class AttackersRoundStartWait : MonoBehaviour
{
    [SerializeField] private float roundStartWait = 5f;
    private GameStateManager gameStateManager;

    private CharacterMediator localPlayer;


    private void Start()
    {
        gameStateManager = GameStateManager.Instance;
        gameStateManager.NewRoundStarted += OnNewRound;

        PlayerNetworkInput.OwnerSpawned += (player) => localPlayer = player;
    }

    private void OnNewRound()
    {
        if (localPlayer.role == Role.Attacker)
        {
            localPlayer.AbilityManager.DisableAbilities(roundStartWait);
            localPlayer.Gun.ChannelingManager.StartChannelingStandingStill(roundStartWait, null, localPlayer, false);
        }
    }
}
