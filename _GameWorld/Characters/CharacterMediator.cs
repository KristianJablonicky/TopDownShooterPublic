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

    [SerializeField] private GameObject[] deactivateOnDeath;
    private (GameObject go, bool wasActive)[] deactivateInternal; 

    public Vector2 GetPosition() => MovementController.transform.position;
    public ulong PlayerId => NetworkInput.OwnerClientId;
    public string PlayerName => NetworkInput.PlayerName.Value.ToString();
    public bool IsLocalPlayer => NetworkInput.IsOwner;

    const float yThreshold = Constants.floorYOffset * 0.5f;
    public Floor CurrentFloor => GetPosition().y < yThreshold ? Floor.First : Floor.Second;
    
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
        SetActivity(false);
        Killed?.Invoke(this, killer);
        Died?.Invoke(this);

        // respawn after a delay if the match has not started yet
        StartCoroutine(GameStateManager.Instance.WarmUpRespawn(this));
    }

    private void SetActivity(bool active)
    {
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
            corpse = AnimationController.DeathAnimation(SpriteRenderer.color);
        }
    }

    public void Reset()
    {
        SetActivity(true);
        Gun.Reset();
        HealthComponent.Reset();
        AbilityManager.Reset();
        MovementController.Reset();
        PlayerVision.Reset();
        Respawned?.Invoke(this);
    }

    private void OnDestroy()
    {
        Disconnected?.Invoke();
    }
}
