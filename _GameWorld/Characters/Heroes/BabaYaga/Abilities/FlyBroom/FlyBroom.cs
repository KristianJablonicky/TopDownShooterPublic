using UnityEngine;

[CreateAssetMenu(fileName = "FlyBroom", menuName = "Abilities/Movement/FlyBroom")]
public class FlyBroom : MovementAbility
{
    [SerializeField] private float duration = 2f;
    [SerializeField] private float movementForcePerSecond = 85f;
    [SerializeField] private float startChannel = 0.5f;
    [SerializeField] private float onFlightEndChannel = 0.25f;
    private bool heldDown = false;
    protected override void OnKeyDown(Vector2 position)
    {
        if (channelingManager.Channeling) return;

        channelingManager.StartChannelingStandingStill(startChannel, StartFlight, owner, false);
    }
    private void StartFlight()
    {
        heldDown = true;
        channelingManager.StartChannelingStandingStill(duration, () => OnKeyUp(Vector2.zero), owner, false);
    }

    public override void IUpdate(float dt)
    {
        base.IUpdate(dt);
        if (!heldDown) return;

        owner.MovementController.ApplyForceInDirection(
            movementForcePerSecond * dt,
            owner.InputHandler.GetCursorPositionNormalized()
        );
    }

    protected override void OnKeyUp(Vector2 position)
    {
        if (!heldDown) return;

        heldDown = false;
        OnCast();
        owner.MovementController.MovementEnabled = true;
        channelingManager.StartChanneling(onFlightEndChannel, null);
    }

    protected override void OnReset()
    {
        heldDown = false;
    }

    protected override string _GetAbilitySpecificStats()
    {
        return $"Duration: {duration}\nSpeed: {movementForcePerSecond}\nTotal channel time: {startChannel + onFlightEndChannel}";
    }
}
