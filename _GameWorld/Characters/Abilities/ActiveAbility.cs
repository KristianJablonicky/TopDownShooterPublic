using UnityEngine;

public abstract class ActiveAbility : Ability, IUpdatable
{
    [field: SerializeField] public float CoolDown { get; private set; }
    [field: SerializeField] public AbilityHotKeys KeyCode { get; private set; } = AbilityHotKeys.Movement;

    public ObservableValue<float> CurrentCoolDown { get; private set; }
    public virtual void IUpdate(float dt)
    {
        if (CurrentCoolDown != 0f)
        {
            CurrentCoolDown.Adjust(-dt, floor: 0f);
        }
    }

    protected override void AbstractReset()
    {
        CurrentCoolDown?.Set(0f);
        OnReset();
    }

    // optional subclass cleanup
    protected virtual void OnReset() { }

    protected override void SetUp() => CurrentCoolDown = new(0f);

    public void SetCoolDown(float newCoolDown) => CurrentCoolDown.Set(newCoolDown);

    // Do not forget to invoke OnCast when the actual ability is used.
    protected void OnCast() => CurrentCoolDown.Set(CoolDown);

    public bool ReadyToCast() => CurrentCoolDown == 0f;

    public void OnKeyInteraction(bool pressedDown, Vector2 position)
    {
        if (pressedDown)
        {
            OnKeyDown(position);
        }
        else
        {
            OnKeyUp(position);
        }
    }
    protected abstract void OnKeyUp(Vector2 position);
    protected abstract void OnKeyDown(Vector2 position);

    /*
    [Rpc(SendTo.Server)]
    public void RequestCastRPC()
    {
        NotifyClientCastRPC();
    }

    [Rpc(SendTo.Everyone)]
    public void NotifyClientCastRPC()
    {
        //...
    }
    */
}

public enum AbilityHotKeys
{
    Movement = KeyCode.Space,
    Utility = KeyCode.E,
}
