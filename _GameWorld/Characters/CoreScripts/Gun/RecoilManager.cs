using UnityEngine;

public class RecoilManager : IUpdatable
{
    public float CurrentRecoil { get; private set; } = 0f;
    private float movementRecoil = 0f,
        gunRecoil;
    private readonly GunConfig config;
    private readonly MovementController mc;
    public RecoilManager(GunConfig config, MovementController mc)
    {
        this.config = config;
        this.mc = mc;
    }

    public float GetRecoilAngle() => CurrentRecoil * config.maxRecoilAngle;
    public bool CanHeadshot() => CurrentRecoil <= config.headshotAccuracyRequirement;
    public void IUpdate(float dt)
    {
        var decay = dt * config.recoilRecoveryPerSecond;

        movementRecoil += dt * mc.GetCurrentSpeed() * config.movementInaccuracyFactor;
        movementRecoil -= decay;
        movementRecoil = Mathf.Min(movementRecoil, config.movementInaccuracyFactor);
        movementRecoil = Mathf.Clamp01(movementRecoil);

        gunRecoil -= decay;
        gunRecoil = Mathf.Clamp01(gunRecoil);

        UpdateCurrentRecoil();
    }

    public void ApplyRecoil(float shotCount)
    {
        gunRecoil += shotCount * config.baseRecoilPerShot;
        gunRecoil *= 1f + config.recoilPerShotMultiplier;
        UpdateCurrentRecoil();
    }

    public void ApplyRecoilMouseMovement(float recoil)
    {
        gunRecoil += recoil;
        UpdateCurrentRecoil();
    }

    private void UpdateCurrentRecoil()
    {
        CurrentRecoil = movementRecoil + gunRecoil;
        CurrentRecoil = Mathf.Clamp01(CurrentRecoil);
    }
}
