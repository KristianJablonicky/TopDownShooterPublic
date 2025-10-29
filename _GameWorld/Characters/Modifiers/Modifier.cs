using System;
using UnityEngine;

public class Modifier
{
    public ObservableValue<float> Duration { get; private set; }
    public ObservableValue<int> Stacks { get; private set; }
    public Sprite Icon { get; private set; }

    public event Action Expired;

    private readonly IModifierStrategy strategy;
    private readonly CharacterMediator owner;

    public Modifier(CharacterMediator owner, IModifierStrategy strategy, float duration, int stacks, Sprite icon)
    {
        var alreadyExists = owner.Modifiers.ModifierExistsOfType(strategy.GetType(), stacks);

        if (alreadyExists) return;

        Duration = new(duration);
        Stacks = new(stacks);
        this.owner = owner;
        this.strategy = strategy;
        Duration.OnValueSet += OnDurationChanged;

        Icon = icon;

        strategy.Apply(owner, this);
        owner.Modifiers.AddModifier(this);
    }

    private void OnDurationChanged(float newDuration)
    {
        if (newDuration <= 0)
        {
            Expire();
        }
    }

    private void Expire()
    {
        strategy.Expire(owner);
        Expired?.Invoke();
        Expired = null;
    }

    public Type ModifierType => strategy.GetType();
    public string Description => strategy.GetDescription();
    public void StartTimer()
    {
        Tweener.Tween(this, Duration, 0f, Duration, TweenStyle.linear,
            value => Duration.Set(value));
    }
}
