using System.Collections;
using UnityEngine;

public class BoundsCheck : SingletonMonoBehaviour<BoundsCheck>
{
    [SerializeField] private BoxCollider2D[] boxColliders;
    [SerializeField] private float checkInterval = 0.2f, maxTimeOutside = 2f;
    [SerializeField] private CanvasGroup screenOverlay;
    [SerializeField] private BoxCollider2D basementCollider;

    private GameStateManager gameStateManager;

    private CharacterMediator localMediator;
    private float timeSpentOutside = 0f;

    private Coroutine checkCoroutine;
    private void Start()
    {
        gameStateManager = GameStateManager.Instance;
        gameStateManager.NewRoundStarted += OnNewRound;
        gameStateManager.RoundEnded += OnRoundEnd;

        PlayerNetworkInput.PlayerSpawned += OnMediatorSpawn;
    }

    private void OnMediatorSpawn(CharacterMediator mediator)
    {
        localMediator = mediator;
        localMediator.Died += (_) => screenOverlay.alpha = 0f;
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
        if (localMediator.Role == Role.Defender)
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

            if (!localMediator.IsAlive || localMediator == null) break;

            var position = localMediator.GetPosition();

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
                localMediator.HealthComponent.TakeLethalDamage();
                break;
            }
        }
    }

    public bool IsPositionInsideBasement(Vector2 position) => basementCollider.bounds.Contains(position);
}
