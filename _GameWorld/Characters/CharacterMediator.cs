using System;
using UnityEngine;

public class CharacterMediator : MonoBehaviour, IResettable
{
    [field: SerializeField] public Gun Gun { get; private set; }
    [field: SerializeField] public MovementController MovementController { get; private set; }
    [field: SerializeField] public RotationController RotationController { get; private set; }
    [field: SerializeField] public AbilityManager AbilityManager { get; private set; }
    [field: SerializeField] public HealthComponent HealthComponent { get; private set; }
    [field: SerializeField] public SpriteRenderer SpriteRenderer { get; private set; }
    [field: SerializeField] public Hitbox Head { get; private set; }
    [field: SerializeField] public PlayerInputHandler InputHandler { get; private set; }
    [field: SerializeField] public PlayerNetworkInput NetworkInput { get; private set; }
    [field: SerializeField] public AbilityRPCs AbilityRPCs { get; private set; }
    [field: SerializeField] public PlayerVision PlayerVision { get; private set; }
    [field: SerializeField] public AnimationController AnimationController { get; private set; }
    [field: SerializeField] public BloodManager BloodManager { get; private set; }

    public Ascendance Ascendance { get; private set; }
    public ModifiersList Modifiers { get; private set; } = new ModifiersList();

    [SerializeField] private GameObject characterPhysics;

    [field: SerializeField] public bool IsNPC { get; private set; } = false;

    public Vector2 GetPosition() => MovementController.transform.position;
    public Transform GetTransform() => MovementController.transform;

    private ulong playerId;
    public void SetID(ulong newId) => playerId = newId;
    public ulong PlayerId => playerId;

    public string PlayerName => NetworkInput.PlayerName.Value.ToString();
    public bool IsLocalPlayer => NetworkInput.IsOwner;

    public Floor CurrentFloor => FloorUtilities.GetCurrentFloor(GetPosition());
    
    public PlayerData playerData;

    private Corpse corpse;
    public bool IsAlive => HealthComponent.CurrentHealth > 0;

    public event Action<Role> NewRoleAssigned;
    public Role? Role { get; private set; } = null;
    public void SetRole(Role newRole)
    {
        Role = newRole;
        NewRoleAssigned?.Invoke(newRole);
    }

    /// <summary>
    /// The first passed object is the mediator of the killed character, the second is the killer
    /// </summary>
    public event Action<CharacterMediator, CharacterMediator> Killed, ScoredAKill;
    public event Action<CharacterMediator> Died, Respawned;
    public event Action Disconnected;

    private void Awake()
    {
        Ascendance = new(this);
        BloodManager?.Init(this);
    }

    public void Die(CharacterMediator killer)
    {
        Killed?.Invoke(this, killer);
        Died?.Invoke(this);
        killer.RequestGotAKillEventInvoke(this);

        if (!IsNPC)
        {
            // respawn after a delay if the match has not started yet
            RespawnManager.Instance.RequestRespawn(this);
        }
        SetActivity(false);
    }

    public void SetActivity(bool active)
    {
        if (!active)
        {
            HealthComponent.CurrentHealth.Set(0);
        }


        characterPhysics.SetActive(active);

        PlayerVision?.ChangeActivityIfPlayerOrAlly(active);

        if (active)
        {
            if(corpse != null)
            {
                Destroy(corpse.gameObject);
            }
        }
        else
        {
            corpse = AnimationController?.DeathAnimation(SpriteRenderer.color);
        }
    }

    public void Reset()
    {
        SetActivity(true);
        Gun?.Reset();
        HealthComponent.Reset();
        AbilityManager?.Reset();
        MovementController?.Reset();
        AnimationController?.Reset();
        //PlayerVision?.Reset();

        Ascendance?.Reset();
        BloodManager?.Reset();

        if (SpriteRenderer != null)
        {
            SpriteRenderer.color = Color.white;
        }

        Respawned?.Invoke(this);
    }

    private void OnDestroy()
    {
        Disconnected?.Invoke();
    }

    public void RequestGotAKillEventInvoke(CharacterMediator victim)
    {
        ScoredAKill?.Invoke(victim, this);
    }

    #region Utilities
    public bool InRange(CharacterMediator otherMediator, float maxRange, bool ignoreFloors)
    {
        return InRange(otherMediator.GetPosition(), maxRange, ignoreFloors);
    }

    public bool InRange(Vector2 position, float maxRange, bool ignoreFloors)
    {

        if (!ignoreFloors)
        {
            return InRange(position, maxRange);
        }

        return InRange(position, maxRange)
            || InRange(FloorUtilities.GetPositionOnTheOtherFloor(position), maxRange);
    }

    private bool InRange(Vector2 position, float maxRange) => Vector2.Distance(GetPosition(), position) <= maxRange;

    public bool LookingAt(CharacterMediator otherMediator, float angle)
    {
        var diff = otherMediator.GetPosition() - GetPosition();

        var targetAngle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;

        var angleDiff = Mathf.DeltaAngle(RotationController.GetRotationAngle, targetAngle);

        return Mathf.Abs(angleDiff) <= angle * 0.5f;
    }

    #endregion
}
