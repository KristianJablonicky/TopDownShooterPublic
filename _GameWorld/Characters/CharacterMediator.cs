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

    public ModifiersList Modifiers { get; private set; } = new ModifiersList();

    [SerializeField] private GameObject[] deactivateOnDeath;

    [field: SerializeField] public bool IsNPC { get; private set; } = false;
    
    private (GameObject go, bool wasActive)[] deactivateInternal; 

    public Vector2 GetPosition() => MovementController.transform.position;
    public Transform GetTransform() => MovementController.transform;

    private ulong playerId;
    public void SetID(ulong newId) => playerId = newId;
    public ulong PlayerId => playerId;

    public string PlayerName => NetworkInput.PlayerName.Value.ToString();
    public bool IsLocalPlayer => NetworkInput.IsOwner;

    public Floor CurrentFloor => FloorUtilities.GetCurrentFloor(GetPosition());
    
    public PlayerData playerData;
    public Role role;

    private Corpse corpse;
    public bool IsAlive => HealthComponent.CurrentHealth > 0;

    /// <summary>
    /// The first passed object is the mediator of the killed character, the second is the killer
    /// </summary>
    public event Action<CharacterMediator, CharacterMediator> Killed;
    public event Action<CharacterMediator> Died, Respawned;
    public event Action Disconnected;

    private void Awake()
    {
        deactivateInternal = new(GameObject go, bool wasActive)[deactivateOnDeath.Length];
        for (int i = 0; i < deactivateOnDeath.Length; i++)
        {
            deactivateInternal[i] = (deactivateOnDeath[i], deactivateOnDeath[i].activeSelf);
        }
    }

    public void Die(CharacterMediator killer)
    {
        Killed?.Invoke(this, killer);
        Died?.Invoke(this);

        if (!IsNPC)
        {
            // respawn after a delay if the match has not started yet
            StartCoroutine(GameStateManager.Instance.WarmUpRespawn(this));
        }
        SetActivity(false);
    }

    public void SetActivity(bool active)
    {
        if (!active)
        {
            HealthComponent.CurrentHealth.Set(0);
        }
        for (int i = 0; i < deactivateInternal.Length; i++)
        {
            // double check if the gameObject was active before
            // enemy vision gets deactivated, and should not go back to being active
            if (!active)
            {
                deactivateInternal[i].wasActive = deactivateInternal[i].go.activeSelf;
            }

            if (deactivateInternal[i].wasActive)
            {
                deactivateInternal[i].go.SetActive(active);
            }

        }


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
        PlayerVision?.Reset();
        Respawned?.Invoke(this);
    }

    private void OnDestroy()
    {
        Disconnected?.Invoke();
    }

    #region Utilities
    public bool InRange(CharacterMediator otherMediator, float maxRange)
    {
        return InRange(otherMediator.GetPosition(), maxRange);
    }

    public bool InRange(Vector2 position, float maxRange)
    {
        return Vector2.Distance(GetPosition(), position) <= maxRange;
    }

    public bool LookingAt(CharacterMediator otherMediator, float angle)
    {
        var diff = otherMediator.GetPosition() - GetPosition();

        var targetAngle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;

        var angleDiff = Mathf.DeltaAngle(RotationController.GetRotationAngle, targetAngle);

        return Mathf.Abs(angleDiff) <= angle * 0.5f;
    }

    #endregion
}
