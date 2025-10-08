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
            StartCoroutine(RoundStartWait());
        }
    }

    private IEnumerator RoundStartWait()
    {
        localPlayer.MovementController.MovementEnabled = false;
        localPlayer.AbilityManager.DisableAbilities(roundStartWait);
        yield return new WaitForSeconds(roundStartWait);
        localPlayer.MovementController.MovementEnabled = true;
    }
}
