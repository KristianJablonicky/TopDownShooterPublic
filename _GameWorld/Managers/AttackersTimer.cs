using System;

public class AttackersTimer : IUpdatable, IResettable
{
    private readonly float maxTime;
    public ObservableValue<float> TimeRemaining { get; private set; }
    public Action TimeRanOut;
    public AttackersTimer(float maxTime)
    {
        this.maxTime = maxTime;
        TimeRemaining = new(maxTime);
    }
    public void IUpdate(float dt)
    {
        TimeRemaining.Adjust(-dt, floor: 0f);
        if (TimeRemaining == 0f)
        {
            TimeRanOut?.Invoke();
        }
    }

    public void Reset()
    {
        TimeRemaining.Set(maxTime);
    }
}
