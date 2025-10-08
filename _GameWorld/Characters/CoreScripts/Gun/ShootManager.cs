using UnityEngine;

public class ShootManager : IUpdatable, IResettable
{
    private readonly GunConfig config;
    private float currentCoolDown = 0f;
    public ObservableValue<int> CurrentAmmo { get; private set; }

    private readonly float coolDown;
    private readonly ChannelingManager channel;

    public enum ShootResult
    {
        Shot,
        DidNotShoot,
        Reload
    }
    public ShootManager(GunConfig config, ChannelingManager channelingManager)
    {
        CurrentAmmo = new(config.capacity);
        this.config = config;
        coolDown = 1f / config.RPM * 60f;

        channel = channelingManager;
    }

    public int GetCapacity() => config.capacity;

    public ShootResult CanShoot()
    {
        if (CurrentAmmo > 0
            && currentCoolDown <= 0f
            && !channel.Channeling)
        {
            CurrentAmmo.Adjust(-1);
            currentCoolDown = coolDown;
            if (CurrentAmmo == 0)
            {
                Reload();
                return ShootResult.Reload;
            }
            
            return ShootResult.Shot;
        }
        return ShootResult.DidNotShoot;
    }

    public void Reload()
    {
        if (CurrentAmmo == config.capacity
            || channel.Channeling) return;

        channel.Channel(config.reloadDuration, () => CurrentAmmo.Set(config.capacity));
    }

    public void IUpdate(float dt)
    {
        if (currentCoolDown > 0f)
        {
            currentCoolDown -= dt;
            currentCoolDown = Mathf.Max(0f, currentCoolDown);
        }
    }

    public void Reset()
    {
        currentCoolDown = 0f;
        CurrentAmmo.Set(config.capacity);
    }

    public void AdjustAmmo(int adjustment) => CurrentAmmo.Adjust(adjustment);

    public void SetAmmo(int newAmmo) => CurrentAmmo.Set(newAmmo);
}
