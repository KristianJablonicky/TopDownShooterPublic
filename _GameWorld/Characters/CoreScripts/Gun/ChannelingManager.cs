using System;

public class ChannelingManager : IUpdatable, IResettable
{
    public ObservableValue<float> TimeRemaining { get; set; }
    public float TimeTotal { get; set; } = 1f;
    public bool Channeling { get; set; }
    private Action actionOnExit;
    public ChannelingManager()
    {
        TimeRemaining = new(0f);
        Channeling = false;
    }

    public void Channel(float duration, Action actionOnExit)
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

    public void Reset()
    {
        TimeRemaining.Set(0f);
        Channeling = false;
        actionOnExit?.Invoke();
    }

    public float Progress => TimeRemaining / TimeTotal;
}