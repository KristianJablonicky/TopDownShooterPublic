using System;
using System.Collections;
using UnityEngine;

public class AiDecisions : MonoBehaviour
{
    [field: SerializeField] public CharacterMediator Mediator { get; private set; }
    [field: SerializeField] public AiMovement Movement { get; private set; }
    [field: SerializeField] public AiShootingHandler ShootingHandler { get; private set; }
    [field: SerializeField] public AiAbilityCaster[] AbilityCasters { get; private set; }
    public NpcDifficultyConfig DifficultyConfig { get; private set; }
    public void SetDifficulty(NpcDifficultyConfig difficulty) => DifficultyConfig = difficulty;
    private AiGoal _currentGoal = AiGoal.MoveToAltar;
    private AiGoal _previousGoal = AiGoal.MoveToAltar;

    private AiGoalCheck currentGoalCheck;
    private Coroutine goalCheckRoutine;

    public AiGoal CurrentGoal
    {
        get => _currentGoal;
        private set
        {
            _previousGoal = _currentGoal;
            _currentGoal = value;
            if (gameObject.activeInHierarchy) GoalChanged?.Invoke(_currentGoal);
        }
    }

    public void SetGoal(AiGoal newGoal) => CurrentGoal = newGoal;

    public event Action<AiGoal> GoalChanged;

    private void Start()
    {
        if (!Mediator.IsOwner)
        {
            CleanUp();
            return;
        }
        Movement.SetDecisionsReference(this);
        ShootingHandler.SetDecisionsReference(this);
        foreach (var abilityCaster in AbilityCasters)
        {
            abilityCaster.SetDecisionsReference(this);
        }

        GoalChanged += OnGoalChanged;
        ShootingHandler.EnemySpotted += () => CurrentGoal = AiGoal.MoveToEnemy;
        ShootingHandler.EnemyLost += OnEnemyLost;

        if (DataStorage.IsSinglePlayer)
        {
            ShootingHandler.SetEnemies(true);
            CurrentGoal = AiGoal.MoveToAltar;
        }
        else
        {
            CurrentGoal = AiGoal.RemainIdle;
        }

        GameStateManager.Instance.GameStarted += OnGameStart;
    }

    private void CleanUp()
    {
        Destroy(gameObject);
    }
    private void OnGoalChanged(AiGoal newGoal)
    {
        if (goalCheckRoutine != null) StopCoroutine(goalCheckRoutine);
        if (!gameObject.activeInHierarchy) return;
        currentGoalCheck = GetGoalChecker(newGoal);

        if (currentGoalCheck != null) goalCheckRoutine = StartCoroutine(GoalCheck());
    }

    private void OnEnemyLost()
    {
        if (Mediator.Role == Role.Attacker)
        {
            CurrentGoal = AiGoal.MoveToAltar;
        }
        else
        {
            if (DefenderObjective.Instance.SafeToSacrifice(Mediator))
            {
                CurrentGoal = AiGoal.MoveToAltar;
            }
            else
            {
                CurrentGoal = AiGoal.MoveToDefenderPosition;
            }
        }
    }

    public BotTeam Team { get; private set; } = BotTeam.AgainstLocalPlayer;

    private IEnumerator GoalCheck()
    {
        var wait = new WaitForSeconds(currentGoalCheck.GetCheckFrequency());
        while (Mediator.IsAlive)
        {
            yield return wait; // must wait first to avoid stack overflow and such
            currentGoalCheck.Check();
        }
    }

    private AiGoalCheck GetGoalChecker(AiGoal newGoal)
    {
        return newGoal switch
        {
            AiGoal.MoveToAltar => new AltarObjectiveCheck(this),
            AiGoal.MoveToDefenderPosition => new GetInPositionCheck(this,
                () => CurrentGoal = AiGoal.MoveToDefenderPosition,
                5f),
            AiGoal.MoveToAttackerPosition => new GetInPositionCheck(this,
                () => CurrentGoal = AiGoal.MoveToAltar,
                3f),
            AiGoal.MoveToPosition => new GetInPositionCheck(this,
                //() => CurrentGoal = _previousGoal,
                OnEnemyLost, // role-specific behaviour
                2f),
            _ => null
        };
    }

    private void OnGameStart()
    {
        var manager = GameStateManager.Instance;
        manager.NewRoundStarted += OnRoundStart;
        manager.RoundEnded += () => CurrentGoal = AiGoal.RemainIdle;
        DefenderObjective.Instance.ObjectiveChanneled += OnObjectiveChanneled;

        Mediator.HealthComponent.DamageTaken += OnDamageTaken;

        Team = (BotTeam)Mediator.playerData.Team.Name;

        var teamMate = Mediator.GetTeamMate();
        if (teamMate.AiDecisions != null)
        {
            teamMate.AiDecisions.ShootingHandler.EnemySpotted += OnAllySpottedEnemy;
        }

        ShootingHandler.SetEnemies(false);
    }

    private void OnAllySpottedEnemy()
    {
        if (CurrentGoal == AiGoal.MoveToEnemy) return;
        CurrentGoal = AiGoal.MoveToEnemy;
        ShootingHandler.BecomePrepared();
    }

    private void OnRoundStart()
    {
        Invoker.Instance.ExecuteAfterDelay(0.5f,
        () => {
            var role = Mediator.Role;
            if (role == Role.Attacker)
            {
                CurrentGoal = AiGoal.MoveToAttackerPosition;
            }
            else
            {
                CurrentGoal = AiGoal.MoveToDefenderPosition;
            }
        });
    }

    private void OnDamageTaken()
    {
        // force goal recalculation so that the bot doesn't just stand still when getting shot
        if (CurrentGoal == AiGoal.MoveToEnemy) return;
        OnEnemyLost();
    }

    private void OnObjectiveChanneled()
    {
        if (CurrentGoal != AiGoal.MoveToAltar) CurrentGoal = AiGoal.MoveToAltar;
        AiCommunicationManager.Instance.EnemySeenAtPosition(
            ImportantPositions.Instance.AltarPosition,
            Team);
        ShootingHandler.BecomePrepared();
    }

    public void GoToPosition(Vector2 targetPosition)
    {
        var actualDestination = Movement.IsDestinationValid(targetPosition);
        if (!actualDestination.HasValue) return; // null if invalid

        CurrentGoal = AiGoal.MoveToPosition;
        Movement.SetDestination(actualDestination.Value);
    }
}

public interface IAiComponent
{
    public void SetDecisionsReference(AiDecisions decisions);
}

public enum AiGoal
{
    MoveToAltar,
    MoveToEnemy,
    MoveToDefenderPosition,
    MoveToAttackerPosition,
    RemainIdle,
    DefendCurrentPosition,
    MoveToPosition
}