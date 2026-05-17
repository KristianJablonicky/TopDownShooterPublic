using System;
using Unity.Netcode;
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
    [field: SerializeField] public NetworkObject NetworkObject { get; private set; }
    [field: SerializeField] public AbilityRPCs AbilityRPCs { get; private set; }
    [field: SerializeField] public PlayerVision PlayerVision { get; private set; }
    [field: SerializeField] public VisionRange VisionRange { get; private set; }
    [field: SerializeField] public AnimationController AnimationController { get; private set; }
    [field: SerializeField] public DeathDissolveMediator DeathDissolveMediator { get; private set; }
    [field: SerializeField] public BloodManager BloodManager { get; private set; }
    [field: SerializeField] public CharacterParticlesMediator ParticlesMediator { get; private set; }
    [field: SerializeField] public Ascendance Ascendance { get; private set; }
    [field: SerializeField] public AiDecisions AiDecisions { get; private set; }
    [field: SerializeField] public SoundPlayer SoundPlayer { get; private set; }
    public ModifiersList Modifiers { get; private set; } = new ModifiersList();

    [SerializeField] private GameObject characterPhysics;
    [field: SerializeField] public bool IsTrainingDummy { get; private set; } = false;
    private OwnerType? ownerType;
    public OwnerType GetOwnerType()
    {
        if (!ownerType.HasValue)
        {
            if (IsTrainingDummy) ownerType = OwnerType.TrainingDummy;
            else if (AiDecisions != null) ownerType = OwnerType.Bot;
            else ownerType = OwnerType.Player;
        }
        return ownerType.Value;
    }
    public bool IsNpc => GetOwnerType() != OwnerType.Player;
    private bool _isBot = false;
    public bool IsBot => _isBot;
    private bool _isPlayer = false;
    public bool IsPlayer => _isPlayer;
    public enum OwnerType
    {
        Player,
        Bot,
        TrainingDummy
    }

    public Vector2 GetPosition() => MovementController.transform.position;
    public Transform GetTransform() => MovementController.transform;

    private ulong playerId;
    public void SetID(ulong newId) => playerId = newId;
    public ulong PlayerId => playerId;

    public string PlayerName => NetworkInput.PlayerName.Value.ToString();
    public bool IsOwner => NetworkInput.IsOwner;
    public bool IsLocalPlayer => IsOwner
        && GetOwnerType() == OwnerType.Player;

    public bool IsOwnerOrNPC => IsOwner
        || GetOwnerType() != OwnerType.Player;

    public Floor CurrentFloor => FloorUtilities.GetCurrentFloor(GetPosition());

    public PlayerData playerData;

    public CharacterToolkit Toolkit => AbilityManager.Toolkit;
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
    public event Action<CharacterMediator> Died, Respawned, RespawnedAfterDying;
    public event Action Disconnected;


    private CharacterMediator teamMate;
    public CharacterMediator GetTeamMate()
    {
        if (teamMate != null) return teamMate;

        if (GameStateManager.Instance.GameInProgress)
        {
            teamMate = playerData.GetTeamMate().Mediator;
            return teamMate;
        }
        return null;
    }

    private void Awake()
    {
        if (GetOwnerType() == OwnerType.TrainingDummy) return;
        BloodManager?.Init(this, ParticlesMediator.bloodParticles);
        _isBot = AiDecisions != null;
        _isPlayer = !_isBot;
    }

    public void Die(CharacterMediator killer)
    {
        Killed?.Invoke(this, killer);
        Died?.Invoke(this);
        killer.RequestGotAKillEventInvoke(this);

        if ((DataStorage.IsSinglePlayer && GetOwnerType() == OwnerType.Player)
            || (!DataStorage.IsSinglePlayer))
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

        PlayerVision?.ChangeActivityIfPlayerOrAlly(active);

        if (active)
        {
            characterPhysics.SetActive(active);

            Tweener.Tween(0f, 0f, 1f, 0.25f, TweenStyle.quadratic,
                value => SpriteRenderer.SetAlpha(value));
        }
        else
        {
            DeathDissolveMediator?.Dissolve(this);
            characterPhysics.SetActive(active);
        }
    }

    public void Reset()
    {
        bool wasAlive = IsAlive;

        SetActivity(true);
        Gun?.Reset();
        HealthComponent.Reset();
        AbilityManager?.Reset();
        MovementController?.Reset();
        AnimationController?.Reset();
        //PlayerVision?.Reset();

        Ascendance?.Reset();
        BloodManager?.Reset();

        Respawned?.Invoke(this);
        if (!wasAlive)
        {
            RespawnedAfterDying?.Invoke(this);
        }
    }

    private void OnDestroy()
    {
        Disconnected?.Invoke();
    }

    public void RequestGotAKillEventInvoke(CharacterMediator victim)
    {
        if (victim != this) ScoredAKill?.Invoke(victim, this);
    }

    public override string ToString() => $"{Toolkit.DatabaseEntry} {playerId} ({PlayerName})";

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

    private bool InRange(Vector2 position, float maxRange) => GetDistance(position) <= maxRange;
    public float GetDistance(CharacterMediator otherMediator) => GetDistance(otherMediator.GetPosition());
    public float GetDistance(Vector2 position) => Vector2.Distance(GetPosition(), position);
    public bool LookingAt(CharacterMediator otherMediator, float angle, bool ignoreFloors)
    {
        Vector2 otherPosition;
        if (ignoreFloors
        &&  FloorUtilities.GetCurrentFloor(this) != FloorUtilities.GetCurrentFloor(otherMediator))
        {
            otherPosition = FloorUtilities.GetPositionOnTheOtherFloor(otherMediator);
        }
        else
        {
            otherPosition = otherMediator.GetPosition();
        }
        var diff = otherPosition - GetPosition();

        var targetAngle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;

        var angleDiff = Mathf.DeltaAngle(RotationController.GetRotationAngle, targetAngle);

        return Mathf.Abs(angleDiff) <= angle * 0.5f;
    }

    private bool? _cachedTeamMateCheck = null;

    public bool? IsTeamMate()
    {
        if (_cachedTeamMateCheck.HasValue)
        {
            return _cachedTeamMateCheck.Value;
        }

        if (!GameStateManager.Instance.GameInProgress) return null;
        
        _cachedTeamMateCheck = IsLocalPlayer ||
            playerData.GetTeamMate().Mediator.IsLocalPlayer;

        return _cachedTeamMateCheck.Value;
    }

    #endregion
}
