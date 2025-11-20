using UnityEngine;

public class RecoilManager : IUpdatable
{
    public float CurrentRecoil { get; private set; } = 0f;
    private float cursorRecoil, //movementRecoil = 0f,
        gunRecoil,
        movementPenaltyRecoil = 0f;

    private float lastSpeed;

    private const float speedMultiplierThreshold = 1.15f;

    private readonly GunConfig config;
    private readonly MovementController mc;
    public RecoilManager(GunConfig config, MovementController mc)
    {
        this.config = config;
        this.mc = mc;
    }

    public float GetRecoilAngle() => CurrentRecoil * config.maxRecoilAngle;
    public bool CanHeadshot() => CurrentRecoil <= config.headshotAccuracyRequirement;

    private bool decaying = true;
    public void IUpdate(float dt)
    {
        var decay = dt * config.recoilRecoveryPerSecond;

        var currentSpeed = mc.GetCurrentSpeed();
        if (currentSpeed > lastSpeed || currentSpeed > 1f)
        {
            ApplyMovementRecoil();
            decaying = false;
        }
        else if (currentSpeed * speedMultiplierThreshold < lastSpeed)
        {
            ApplyMovementDecay(decay);
            decaying = true;

        }
        else
        {
            if (decaying) ApplyMovementDecay(decay);
            else ApplyMovementRecoil();
        }

        lastSpeed = currentSpeed;


        gunRecoil -= decay;
        gunRecoil = Mathf.Clamp01(gunRecoil);

        cursorRecoil -= decay * config.cursorRecoilRecoveryMultiplier;
        cursorRecoil = Mathf.Clamp01(cursorRecoil);


        UpdateCurrentRecoil();
    }

    private void ApplyMovementRecoil()
    {
        movementPenaltyRecoil = config.movementInaccuracyFactor;
    }
    private void ApplyMovementDecay(float decay)
    {
        movementPenaltyRecoil -= decay;
        movementPenaltyRecoil = Mathf.Clamp01(movementPenaltyRecoil);
    }

    public void ApplyRecoil(float shotCount)
    {
        gunRecoil += shotCount * config.baseRecoilPerShot;
        gunRecoil *= 1f + config.recoilPerShotMultiplier;
        UpdateCurrentRecoil();
    }

    public void ApplyRecoilMouseMovement(float recoil)
    {
        cursorRecoil += recoil;
        UpdateCurrentRecoil();
    }

    private void UpdateCurrentRecoil()
    {
        CurrentRecoil = gunRecoil + movementPenaltyRecoil + cursorRecoil;
        CurrentRecoil = Mathf.Clamp01(CurrentRecoil);
    }
}
