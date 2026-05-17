using UnityEngine;

[CreateAssetMenu(fileName = "FlyBroom", menuName = "Abilities/Movement/FlyBroom")]
public class FlyBroom : MovementAbility
{
    [SerializeField] private float duration = 2f;
    [SerializeField] private float movementForcePerSecond = 85f;
    [SerializeField] private float startChannel = 0.5f;
    [SerializeField] private float onFlightEndChannel = 0.25f;
    [Header("Audio")]
    [SerializeField] private AudioClip flyingClip;
    [SerializeField] private float fadeDuration = 0.4f;
    private bool heldDown = false;
    protected override void OnKeyDown(Vector2 position)
    {
        if (channelingManager.Channeling) return;

        channelingManager.StartChannelingStandingStill(startChannel, StartFlight, owner, false);
        AlsoPlayAnimation();
    }
    private void StartFlight()
    {
        heldDown = true;
        channelingManager.StartChannelingStandingStill(duration, () => OnKeyUp(Vector2.zero), owner, false);
        AlsoPlayAnimation(specialIndex: 0);
        owner.AnimationController.SetLetVisibility(false);

        owner.SoundPlayer.RequestPlaySound(owner.GetTransform(), flyingClip, false);
        owner.SoundPlayer.FadeVolume(0f, 1f, fadeDuration);
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
        AlsoPlayAnimation(specialIndex: 1);
        owner.AnimationController.SetLetVisibility(true);
        owner.SoundPlayer.FadeVolume(1f, 0f, onFlightEndChannel);
    }

    protected override void OnReset()
    {
        heldDown = false;
    }

    public override string _GetSpecificAttributes()
    {
        return $"Duration: {duration}\nSpeed: {movementForcePerSecond}\nTotal channel time: {startChannel + onFlightEndChannel}";
    }
}
