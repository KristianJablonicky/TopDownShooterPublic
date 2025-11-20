using UnityEngine;

public abstract class AbilityPostMortem : ActiveAbility
{
    protected CharacterMediator teamMate;
    [SerializeField] private float onDeathCoolDownRatio = 0.5f;
    public void Init(CharacterMediator teamMate)
    {
        this.teamMate = teamMate;
        owner.Ascendance.SpiritLeft += OnOwnerDeath;
        owner.Respawned += OnOwnerRespawn;
    }

    private bool openedThirdEye = false;
    private void OnOwnerDeath(CharacterMediator owner)
    {
        if (teamMate.IsAlive)
        {
            var coolDown = onDeathCoolDownRatio * CoolDown;
            if (coolDown != 0f)
            {
                CurrentCoolDown.Set(coolDown);
            }

            ThirdEyeOpen();
            openedThirdEye = true;
        }
    }

    private void OnOwnerRespawn(CharacterMediator owner)
    {
        if (openedThirdEye)
        {
            ThirdEyeClosed();
        }
        openedThirdEye = false;
    }

    protected virtual void ThirdEyeOpen() { }
    protected virtual void ThirdEyeClosed() { }
    protected override void OnKeyDown(Vector2 position)
    {
        if (openedThirdEye)
        {
            OnKeyDownSecure(position);
        }
    }

    protected override void OnKeyUp(Vector2 position)
    {
        if (openedThirdEye)
        {
            OnKeyUpSecure(position);
        }
    }

    protected abstract void OnKeyDownSecure(Vector2 position);
    protected abstract void OnKeyUpSecure(Vector2 position);

    public override AbilityType GetAbilityType() => AbilityType.PostMortem;
}
