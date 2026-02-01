using System;
using System.Diagnostics;

public class ChannelingManager : IUpdatable, IResettable
{
    public ObservableValue<float> TimeRemaining { get; set; }
    public float TimeTotal { get; private set; } = 1f;
    public bool Channeling { get; private set; }
    public bool Interruptible { get; private set; }

    private AnimationController animationController;

    private bool allowActionCompleteOnInterruption;
    private Action actionOnExit;
    public ChannelingManager(AnimationController animationController)
    {
        TimeRemaining = new(0f);
        Channeling = false;
        this.animationController = animationController;
    }

    public void StartChanneling(float duration, Action actionOnExit)
    {
        SetChannelingShared(duration, actionOnExit);
        Interruptible = false;
    }
    public void StartChannelingStandingStill(float duration, Action actionOnExit, CharacterMediator mediator, bool interruptible)
    {
        StartChannelingSlowedDown(duration, actionOnExit, mediator, 0f, interruptible);
    }

    public void StartChannelingSlowedDown(float duration, Action actionOnExit,
        CharacterMediator mediator, float movementSpeedMultiplier, bool interruptible)
    {
        Action enableMovement;
        if (movementSpeedMultiplier == 0f)
        {
            mediator.MovementController.MovementEnabled = false;
            enableMovement = () => mediator.MovementController.MovementEnabled = true;
        }
        else
        {
            mediator.MovementController.AddOrChangeMultiplier(this, -movementSpeedMultiplier);
            enableMovement = () => mediator.MovementController.AddOrChangeMultiplier(this, 0f);
        }


        if (interruptible)
        {
            StartChanneling(duration, enableMovement + actionOnExit, false);
        }
        else
        {
            StartChanneling(duration, enableMovement + actionOnExit);
        }
    }

    public void StartChanneling(float duration, Action actionOnExit, bool finishActionOnInterrupt)
    {
        SetChannelingShared(duration, actionOnExit);
        Interruptible = true;
        allowActionCompleteOnInterruption = finishActionOnInterrupt;
    }


    private void SetChannelingShared(float duration, Action actionOnExit)
    {
        this.actionOnExit = actionOnExit;
        TimeTotal = duration;
        TimeRemaining.Set(duration);
        Channeling = true;
    }

    public void IUpdate(float dt)
    {
        if (Channeling)
        {
            TimeRemaining.Adjust(-dt, 0f);
            if (TimeRemaining == 0f)
            {
                Channeling = false;
                // since multiple actions can be chained,
                // we have to clear the queue first, then invoke the action
                var actionOnExitTemp = actionOnExit;
                actionOnExit = null;
                actionOnExitTemp?.Invoke();
            }
        }
    }

    public void AlsoPlayAnAnimation(Animations animation, float durationMultiplier = 1f, float durationBonus = 0f)
    {
        animationController.PlayAnimation(animation, TimeTotal * durationMultiplier + durationBonus);
    }


    public void Reset()
    {
        TimeRemaining.Set(0f);
        Channeling = false;
        actionOnExit = null;
    }

    public float Progress => TimeRemaining / TimeTotal;

    /// <summary>
    /// Request interrupt.
    /// </summary>
    /// <returns><b>true</b> if channeling was successfully interrupted, or no channel was currently performed.</returns>
    public bool RequestInterrupt()
    {
        if (Channeling && !Interruptible) return false;
        if (!allowActionCompleteOnInterruption) actionOnExit = null;
        else actionOnExit?.Invoke();
        
        Reset();
        return true;
    }
}