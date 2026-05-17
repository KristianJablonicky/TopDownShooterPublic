using System;
using UnityEngine;

public abstract class AiGoalCheck
{
    protected AiDecisions decisions;
    public AiGoalCheck(AiDecisions decisions)
    {
        this.decisions = decisions;
    }
    public abstract float GetCheckFrequency();
    public abstract void Check();
}

public class AltarObjectiveCheck : AiGoalCheck
{
    private readonly DefenderObjective objective;
    private readonly CharacterMediator owner;
    public AltarObjectiveCheck(AiDecisions decisions) : base(decisions)
    {
        objective = DefenderObjective.Instance;
        owner = decisions.Mediator;
    }

    public override void Check()
    {
        if (!objective.SafeToSacrifice(owner)) return; // the bot would die and not complete the ritual, don't bother
        
        if (objective.CanSacrifice(owner))
        {
            owner.NetworkInput.RequestObjectiveSacrifice();
        }
    }
    public override float GetCheckFrequency() => 1f;
}

public class GetInPositionCheck : AiGoalCheck
{
    Vector2? destination;
    public event Action DestinationReached;
    public GetInPositionCheck(AiDecisions decisions, Action OnGoalReached, float maxRandomDelay) : base(decisions)
    {
        DestinationReached += () =>
        {
            var delay = maxRandomDelay * UnityEngine.Random.Range(0.5f, 1f);
            decisions.SetGoal(AiGoal.DefendCurrentPosition);
            Invoker.Instance.ExecuteAfterDelay(delay, OnGoalReached);
        };
    }
    public override void Check()
    {
        destination = decisions.Movement.CurrentDestination;
        if (!destination.HasValue) return;
        if (decisions.Mediator.GetDistance(destination.Value) < 1.5f)
        {
            DestinationReached?.Invoke();
        }
    }

    public override float GetCheckFrequency() => 1f;
}