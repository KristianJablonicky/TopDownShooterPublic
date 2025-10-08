using UnityEngine;

[CreateAssetMenu(fileName = "FlyBroom", menuName = "Abilities/Movement/FlyBroom")]
public class FlyBroom : ActiveAbility
{
    [SerializeField] private float duration = 2f;
    [SerializeField] private float movementForcePerSecond = 85f;
    [SerializeField] private float startChannel = 0.5f;
    [SerializeField] private float onFlightEndChannel = 0.25f;
    private bool heldDown = false;
    protected override void OnKeyDown(Vector2 position)
    {
        if (owner.Gun.ChannelingManager.Channeling) return;

        owner.Gun.ChannelingManager.Channel(startChannel, StartFlight);
    }
    private void StartFlight()
    {
        heldDown = true;
        owner.Gun.ChannelingManager.Channel(duration, () => OnKeyUp(Vector2.zero));
        owner.MovementController.MovementEnabled = false;
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
        owner.MovementController.MovementEnabled = true;
        OnCast();
        owner.Gun.ChannelingManager.Channel(onFlightEndChannel, null);
    }

    protected override void OnReset()
    {
        base.OnReset();
        heldDown = false;
    }
}
