using System;
using System.Collections;
using UnityEngine;

public class AiShootingHandler : InputHandlerBase, IAiComponent
{
    [SerializeField] private float guaranteedVisionRange = 1.5f;
    private NpcDifficultyConfig difficultyConfig;
    private float currentInterestTimer;

    public event Action EnemySpotted;
    public event Action EnemyLost;

    private CharacterMediator[] enemies;
    private CharacterMediator Player;
    private CharacterMediator currentlySeenEnemy;
    
    private AiDecisions decisions;
    private AiMovement movementController;

    private float currentDifficultyValue;
    private bool alert = false;
    private float currentWaitDelay;
    private float currentDelayBeforeShooting;
    private Coroutine aimCoroutine;

    public bool IsAlert => alert;
    public void SetDecisionsReference(AiDecisions decisions)
    {
        enabled = false;
        this.decisions = decisions;
        movementController = decisions.Movement;
        difficultyConfig = decisions.DifficultyConfig;

        currentDifficultyValue = NpcSpawner.CurrentDifficultyValue;
        currentInterestTimer = difficultyConfig.LoseInterestDuration;

        if (CharacterManager.Instance != null
            && CharacterManager.Instance.LocalPlayerMediator != null)
        {
            OnPlaySpawned(CharacterManager.Instance.LocalPlayerMediator);
            return;
        }

        PlayerNetworkInput.PlayerSpawned += OnPlaySpawned;
    }

    public void BecomePrepared()
    {
        currentDelayBeforeShooting = Mathf.Min(
            currentDelayBeforeShooting,
            UnityEngine.Random.Range(0.25f, 0.5f) * MaxDelayBeforeShooting);
    }

    public void AdjustDifficulty(float adjustment)
        => currentDifficultyValue += adjustment;

    private void OnPlaySpawned(CharacterMediator mediator)
    {
        Player = mediator;
    }

    public void SetEnemies(bool altarDefense)
    {
        if (altarDefense)
        {
            enemies = new[] { Player };
        }
        else
        {
            var enemyPlayers = mediator.playerData.Team.EnemyTeamData.Players;
            var enemyCount = enemyPlayers.Length;
            enemies = new CharacterMediator[enemyCount];
            for (int i = 0; i < enemyCount; i++)
            {
                enemies[i] = enemyPlayers[i].Mediator;
            }
        }

        enabled = true;
        StartCoroutine(AiInput());
    }

    private void OnDisable()
    {
        if (aimCoroutine != null) StopCoroutine(aimCoroutine);
    }
    private void OnEnable()
    {
        if (enemies != null)
        {
            aimCoroutine = StartCoroutine(AiInput());
        }
    }

    private IEnumerator AiInput()
    {
        currentWaitDelay = difficultyConfig.CheckInterval;
        //bool inLineOfSight;
        bool enemyInSight;
        while (true)
        {
            yield return new WaitForSeconds(currentWaitDelay);
            enemyInSight = false;

            if (decisions.CurrentGoal == AiGoal.RemainIdle)
            {
                if (alert) LoseInterest();
                continue;
            }

            foreach (var player in enemies)
            {
                enemyInSight = enemyInSight || ConsiderShooting(player, enemyInSight);
            }

            if (enemyInSight) continue;

            // we've lost the enemy, but not our interest
            ReduceInterest(currentWaitDelay);
            if (!alert) continue;

            var inaccuracy = UnityEngine.Random.insideUnitCircle *
                difficultyConfig.InaccuracyMultiplierOutOfVision.GetValue(currentDifficultyValue);
                    
            var pos = AiCommunicationManager.Instance.EnemyLastSeenPosition(decisions.Team);
                    
            if (pos.HasValue)
            {
                Shoot(pos.Value + inaccuracy, true);
            }
        }
    }

    private bool ConsiderShooting(CharacterMediator player, bool seenAnEnemyAlready)
    {
        if (!player.IsAlive) return false;

        var distance = Vector2.Distance(mediator.GetPosition(), player.GetPosition());

        if (distance > mediator.VisionRange.ModifiableValue.CurrentValue + difficultyConfig.VisionRangeBonus) return false;

        var raycast = mediator.PlayerVision.GetRaycastHit(
            mediator.GetPosition(),
            (player.GetPosition() - mediator.GetPosition()).normalized,
            distance);

        // see sprites sticking out and enemies in smoke
        if (raycast.collider != null && distance > guaranteedVisionRange) return false;

        if (!alert)
        {
            BecomeAlert();
            currentlySeenEnemy = player;
        }
        if (!seenAnEnemyAlready)
        {
            currentDelayBeforeShooting -= currentWaitDelay;
        }
        if (currentDelayBeforeShooting > 0f) return true;

        if (distance > gun.GunConfig.bulletRange) return true; // the enemy is visible but out of range, wait close up on them
        
        AiCommunicationManager.Instance.EnemySeenAtPosition(player.GetPosition(), decisions.Team);

        var inaccuracy = UnityEngine.Random.insideUnitCircle *
            difficultyConfig.InaccuracyMultiplier.GetValue(currentDifficultyValue);

        // Player is visible and in range (or the Ai remembers where they were), fire away
        var pos = AiCommunicationManager.Instance.EnemyLastSeenPosition(decisions.Team);
        if (pos.HasValue)
        {
            Shoot(pos.Value + inaccuracy, true);
        }
        return true;
    }

    /// <summary>
    /// The player got lost, should we keep firing and chasing?
    /// </summary>
    /// <returns>true if the player got lost recently</returns>
    private bool ReduceInterest(float timeSinceLastCheck)
    {
        if (!alert) return false;
        if (currentInterestTimer <= 0f) // player got lost for too long, stop firing and chasing
        {
            LoseInterest();
            return false;
        }
        currentInterestTimer -= timeSinceLastCheck;
        return true;
    }

    private void BecomeAlert()
    {
        EnemySpotted?.Invoke();
        alert = true;
        currentWaitDelay = difficultyConfig.CheckIntervalAlert;
        currentInterestTimer = difficultyConfig.LoseInterestDuration;
        currentDelayBeforeShooting = MaxDelayBeforeShooting + currentWaitDelay;
        // add currentWaitDelay since we'll subtract that value immediately before the if check
    }
    private float MaxDelayBeforeShooting => difficultyConfig.DelayBeforeInitialShooting.GetValue(currentDifficultyValue);
    private void LoseInterest()
    {
        EnemyLost?.Invoke();
        alert = false;
        currentWaitDelay = difficultyConfig.CheckInterval;
    }

    #region rotation
    private Func<Vector2> getLookingTarget;
    private void FixedUpdate()
    {
        if (alert)
        {
            rotationController.LookAt(currentlySeenEnemy.GetPosition());
        }
        else if (movementController.enabled)
        {
            rotationController.LookAt(movementController.GetNextCorner());
        }
        //rotationController.LookAt(getLookingTarget());
    }
    /*
    private Vector2 LookAtPlayer()
    {
        if (player.IsAlive) return player.GetPosition();
        return Vector2.zero;
    }
    private Vector2 altarPosition;
    private Vector2 LookAtAltar() => altarPosition;
    */
    #endregion
}
