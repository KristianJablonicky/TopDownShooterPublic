using System;
using UnityEngine;

public abstract class ActiveAbility : Ability, IUpdatable
{
    protected ChannelingManager channelingManager;
    [field: SerializeField] public float CoolDown { get; set; }
    [field: SerializeField] public bool availableBeforeRoundStart { get; set; } = true;


    private GameObject rangeIndicator;
    public ObservableValue<float> CurrentCoolDown { get; private set; }
    
    public event Action AbilityBecameReady,
        AbilityCast;
    public abstract AbilityHotKeys KeyCode { get; protected set; }
    
    public virtual void IUpdate(float dt)
    {
        if (CurrentCoolDown == 0f) return;
        
        CurrentCoolDown.Adjust(-dt, floor: 0f);
        if (CurrentCoolDown == 0f)
        {
            AbilityBecameReady?.Invoke();
        }
    }

    protected override void AbstractReset()
    {
        CurrentCoolDown?.Set(0f);

        if (rangeIndicator.activeSelf)
        {
            HideRangeIndicator();
        }

        OnReset();
    }

    protected override string _GetAbilitySuffix() => $"Cooldown: {CoolDown}";

    // optional subclass cleanup
    protected virtual void OnReset() { }

    protected override void SetUp()
    {
        CurrentCoolDown = new(0f);
        channelingManager = owner.Gun.ChannelingManager;
    }

    public void SetCoolDown(float newCoolDown) => CurrentCoolDown.Set(newCoolDown);

    // Do not forget to invoke OnCast when the actual ability is used.
    protected void OnCast()
    {
        CurrentCoolDown.Set(CoolDown);
        AbilityCast?.Invoke();
    }

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

    public void SetRangeIndicatorReference(GameObject rangeIndicator) => this.rangeIndicator = rangeIndicator;
    protected void ShowRangeIndicator(float range)
    {
        rangeIndicator.SetActive(true);
        rangeIndicator.transform.localScale = Vector2.one * range;
    }
    protected void HideRangeIndicator() => rangeIndicator.SetActive(false);
    protected Vector2? GetDestination(Vector2 mousePosition, float range, bool worksAcrossFloors,
        CharacterMediator positionRelativeTo = null)
    {
        if (positionRelativeTo == null)
        {
            positionRelativeTo = owner;
        }
        var yOffset = FloorUtilities.GetYOffset(owner.InputHandler.AimDirection, positionRelativeTo.CurrentFloor);
        var characterPosition = positionRelativeTo.GetPosition();
        var distance = Vector2.Distance(characterPosition, mousePosition);
        
        // clamp mouse position if aiming too far
        if (distance > range)
        {
            mousePosition = characterPosition + ((mousePosition - characterPosition).normalized * range);
        }

        Vector2 finalPosition;
        if (worksAcrossFloors && yOffset.HasValue)
        {
            finalPosition = new(mousePosition.x, mousePosition.y + yOffset.Value);
        }
        else
        {
            finalPosition = mousePosition;
        }

        if ((FloorUtilities.GetCurrentFloor(finalPosition) == Floor.Outside)
        || BoundsCheck.Instance.IsPositionInsideBasement(finalPosition))
        {
            return finalPosition;
        }

        return null;
    }

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

public abstract class MovementAbility : ActiveAbility
{
    public override AbilityHotKeys KeyCode { get; protected set; } = AbilityHotKeys.Movement;
    public override AbilityType GetAbilityType() => AbilityType.Movement;
}

public abstract class UtilityAbility : ActiveAbility
{
    public override AbilityHotKeys KeyCode { get; protected set; } = AbilityHotKeys.Utility;
    public override AbilityType GetAbilityType() => AbilityType.Utility;
}



public enum AbilityHotKeys
{
    Movement = KeyCode.Space,
    Utility = KeyCode.E,
}
