using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class AiMovement : MonoBehaviour, IAiComponent
{
    [SerializeField] private NavMeshAgent navMeshAgent;
    private AiDecisions decision;
    private MovementController movementController;
    private AiCommunicationManager communicationManager;
    private ImportantPositions importantPositions;

    public Vector2? CurrentDestination { get; private set; } = null;
    public Vector2 CurrentPosition => movementController.transform.position;
    public Transform CurrentTransform => movementController.transform;
    private void Awake()
    {
        enabled = false;
        navMeshAgent.updatePosition = false;
        navMeshAgent.updateRotation = false;
        navMeshAgent.updateUpAxis = false;
        communicationManager = AiCommunicationManager.Instance;
    }
    public void SetDecisionsReference(AiDecisions decision)
    {
        this.decision = decision;
        movementController = decision.Mediator.MovementController;
        movementController.PositionChanged += OnPositionChanged;
        decision.GoalChanged += OnGoalChanged;
        navMeshAgent.Warp(transform.position);
        importantPositions = ImportantPositions.Instance;
    }
    
    private void OnGoalChanged(AiGoal goal)
    {
        Vector2 goalPosition = importantPositions.GetTargetPosition(
            CurrentTransform, importantPositions.RitualAltarSpots);
        
        StopAllCoroutines();
        enabled = true;

        if (goal == AiGoal.MoveToAltar)
        {
            // default value
            //goalPosition = positions.GetTargetPosition(CurrentTransform, positions.RitualAltar);
        }
        else if (goal == AiGoal.MoveToEnemy)
        {
            var lastSeen = communicationManager.EnemyLastSeenPosition(decision.Team);
            StartCoroutine(UpdateTargetPosition());
            if (lastSeen.HasValue)
            {
                goalPosition = importantPositions.GetTargetPosition(
                    CurrentTransform,
                    communicationManager.EnemyLastSeenPosition(decision.Team).Value
                );
            }
        }
        else if (goal == AiGoal.MoveToDefenderPosition)
        {
            goalPosition = importantPositions.GetTargetPosition(
                CurrentTransform, importantPositions.DefenderPositionSpots);
        }
        else if (goal == AiGoal.MoveToAttackerPosition)
        {
            goalPosition = importantPositions.GetTargetPosition(
                CurrentTransform, importantPositions.AttackerPositionSpots);
        }
        else if (goal == AiGoal.MoveToPosition)
        {
            if (_nextDestination.HasValue)
            {
                goalPosition = _nextDestination.Value;
            }
        }
        else
        {
            _nextDestination = null;
            enabled = false;
            return;
        }

        if (goal != AiGoal.MoveToPosition) _nextDestination = null; // reset just in case

        SetDestination(goalPosition);
    }
    private Vector2? _nextDestination;
    public Vector2? IsDestinationValid(Vector2 destination)
    {
        var result = NavMesh.SamplePosition(destination, out var hit, 1f, NavMesh.AllAreas);
        if (!result) return null;
        if (!FloorUtilities.IsOnTheSameFloor(destination, CurrentPosition))
        {
            var closestStair = importantPositions.GetClosestStair(CurrentPosition);
            _nextDestination = destination;
            return closestStair;
        }
        return destination;
    }

    public void SetDestination(Vector2 destination)
    {
        if (!gameObject.activeSelf)
        {
            CurrentDestination = null;
            return;
        }
        enabled = true;

        navMeshAgent.SetDestination(destination);
        CurrentDestination = destination;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private void OnPositionChanged(Vector2 newPosition)
    {
        navMeshAgent.Warp(newPosition);
        OnGoalChanged(decision.CurrentGoal);
    }

    private void Update()
    {
        movementController.WalkInDirection(
            GetNextDirection(CurrentPosition)
        );
    }
    private void LateUpdate()
    {
        var position = CurrentPosition;
        navMeshAgent.nextPosition = position;
        transform.position = position;
    }

    private IEnumerator UpdateTargetPosition()
    {
        while (true)
        {
            Vector2? lastSeen;
            yield return new WaitForSeconds(0.5f);
            lastSeen = communicationManager.EnemyLastSeenPosition(decision.Team);
            if (lastSeen.HasValue)
            {
                var goalPosition = ImportantPositions.Instance.GetTargetPosition(
                    CurrentTransform,
                    communicationManager.EnemyLastSeenPosition(decision.Team).Value
                );
                SetDestination(goalPosition);
            }
        }
    }

    public Vector2 GetNextDirection(Vector2 currentPosition)
    {
        if (!navMeshAgent.hasPath || navMeshAgent.path.corners.Length < 2)
            return Vector2.zero;

        return (GetNextCorner() - currentPosition).normalized;
    }

    public Vector2 GetNextCorner()
    {
        if (navMeshAgent.path.corners.Length > 1)
        {
            return navMeshAgent.path.corners[1];
        }
        return navMeshAgent.path.corners[0];
    }
}

