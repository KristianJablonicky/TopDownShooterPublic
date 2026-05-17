using UnityEngine;
using static AimDirection;

public class InputHandlerBase : MonoBehaviour
{
    [SerializeField] protected CharacterMediator mediator;
    protected PlayerNetworkInput networkInput;
    protected RotationController rotationController;
    protected Gun gun;

    protected virtual void Awake()
    {
        networkInput = mediator.NetworkInput;
        rotationController = mediator.RotationController;
        gun = mediator.Gun;
    }

    public AimDirection AimDirection { get; protected set; } = Straight;

    protected void Shoot(Vector2 cursorPos, bool firstPress)
    {
        if (WindowStackManager.Instance.WindowsOpen) return;
        if (!gun.CanShoot(firstPress)) return;

        var shotCount = gun.ShotCount;
        for (int i = 0; i < shotCount; i++)
        {
            var actualShootDirection = gun.GetShootDirection(cursorPos);
            var canHeadshot = gun.CanHeadShot(firstPress, AimDirection);
            gun.ApplyRecoil();

            networkInput.RequestShootRpc(cursorPos, actualShootDirection, canHeadshot, AimDirection);
        }
        networkInput.ShowGunshotVisualsRPC(mediator.PlayerId);
    }
}
