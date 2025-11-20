using System;
using UnityEngine;

public class AttackersTimer : IUpdatable, IResettable
{
    private readonly float maxTime;
    private float _timeRemaining;
    public ObservableValue<int> TimeRemaining { get; private set; }
    public Action TimeRanOut;
    public AttackersTimer(float maxTime)
    {
        this.maxTime = maxTime;
        _timeRemaining = maxTime;
        TimeRemaining = new((int)maxTime);
    }
    public void IUpdate(float dt)
    {
        _timeRemaining = Mathf.Max(_timeRemaining - dt, 0f);
        TimeRemaining.Set(Mathf.CeilToInt(_timeRemaining));
        if (_timeRemaining == 0f)
        {
            TimeRanOut?.Invoke();
        }
    }

    public void Reset()
    {
        _timeRemaining = maxTime;
        TimeRemaining.Set((int)maxTime);
    }
}
