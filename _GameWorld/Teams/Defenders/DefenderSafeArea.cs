using System.Collections;
using UnityEngine;

public class BoundsCheck : MonoBehaviour
{
    [SerializeField] private BoxCollider2D[] boxColliders;
    [SerializeField] private float checkInterval = 0.2f, maxTimeOutside = 2f;
    [SerializeField] private CanvasGroup screenOverlay;

    private GameStateManager gameStateManager;

    private CharacterMediator localPlayer;
    private float timeSpentOutside = 0f;

    private Coroutine checkCoroutine;
    private void Start()
    {
        gameStateManager = GameStateManager.Instance;
        gameStateManager.NewRoundStarted += OnNewRound;
        gameStateManager.RoundEnded += OnRoundEnd;

        PlayerNetworkInput.PlayerSpawned += (player) => localPlayer = player;
    }

    private void OnRoundEnd()
    {
        timeSpentOutside = 0f;
        if (checkCoroutine is not null)
        {
            StopCoroutine(checkCoroutine);
            checkCoroutine = null;
        }
        screenOverlay.alpha = 0f;
    }

    private void OnNewRound()
    {
        if (localPlayer.Role == Role.Defender)
        {
            checkCoroutine = StartCoroutine(CheckDefender());
        }
    }

    private IEnumerator CheckDefender()
    {
        var wait = new WaitForSeconds(checkInterval);
        timeSpentOutside = 0f;
        screenOverlay.alpha = 0f;

        while (gameStateManager.GameInProgress)
        {
            yield return wait;

            if (!localPlayer.IsAlive || localPlayer == null) break;

            var position = localPlayer.GetPosition();

            bool isInside = false;
            foreach(var box in boxColliders)
            {
                if (box.bounds.Contains(position))
                {
                    isInside = true;
                    break;
                }
            }
            
            if (isInside)
            {
                if (timeSpentOutside != 0f)
                {
                    timeSpentOutside = 0f;
                    screenOverlay.alpha = 0f;
                }
                continue;
            }

            timeSpentOutside += checkInterval;
            screenOverlay.alpha = timeSpentOutside / maxTimeOutside;

            if (timeSpentOutside >= maxTimeOutside)
            {
                localPlayer.HealthComponent.TakeLethalDamage();
                break;
            }
        }
        
    }
}
