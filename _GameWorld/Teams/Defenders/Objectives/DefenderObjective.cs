using System;
using Unity.Netcode;
using UnityEngine;

public class DefenderObjective : SingletonMonoBehaviour<DefenderObjective>, IResettable
{
    [Header("Objective Settings")]
    [SerializeField] private float channelDuration = 2f;
    [SerializeField] private float speedSlowWhileChanneling = 0.5f;
    [field: SerializeField] public int SacrificeCost { get; private set; } = 50;
    [field: SerializeField] public int SacrificeCostAttackerBlood { get; private set; } = 15;
    [field: SerializeField] public int SacrificesRequired { get; private set; } = 3;

    [SerializeField] private AudioClip channelStart, channelEnd;
    
    [Header("References")]
    [SerializeField] private SoundPlayer soundPlayer;
    [SerializeField] private Collider2D sacrificeStartArea;
    [SerializeField] private ChaliceManager chaliceManager;
    [SerializeField] private GameObject particleSpawner;

    public ObservableValue<int> SacrificesRemaining { get; private set; }
    public event Action ObjectiveChanneled;

    private GameStateManager stateManager;

    protected override void OverriddenAwake()
    {
        base.OverriddenAwake();
        SacrificesRemaining = new(SacrificesRequired);
    }

    private void Start()
    {
        stateManager = GameStateManager.Instance;
        stateManager.NewRoundStarted += Reset;
    }

    private bool gameStarted = false;
    public bool CanSacrifice(CharacterMediator mediator)
    {
        if (!gameStarted)
        {
            gameStarted = stateManager != null
            && stateManager.GameInProgress
            || mediator.AiDecisions != null; // altar defense
            if (!gameStarted) return false;
        }
        return gameStarted
            && (!stateManager.RoundDecided || !stateManager.GameInProgress) // round not decided or altar defense
            && sacrificeStartArea.OverlapPoint(mediator.GetPosition());
    }

    //&& mediator.InRange(transform.position, sacrificeRange, false);
    public void StartSacrifice(CharacterMediator sacrificedMediator)
    {
        var channeling = sacrificedMediator.Gun.ChannelingManager;
        if (channeling.Channeling) return;

        ObjectiveChanneled?.Invoke();
        
        soundPlayer.RequestPlaySound(transform, channelStart, true);
        channeling.StartChannelingSlowedDown
        (
            channelDuration,
            () => RequestCompleteSacrifice(sacrificedMediator),
            sacrificedMediator,
            speedSlowWhileChanneling,
            false
        );

        chaliceManager.StartChannelingAnimation(sacrificedMediator, channelDuration);
    }

    private void RequestCompleteSacrifice(CharacterMediator sacrificedMediator)
    {
        if (!NetworkManager.Singleton.IsHost) return;
        
        if (!sacrificedMediator.IsAlive
            || stateManager.RoundDecided) return;

        // guaranteed to run on host - synch up all other players
        sacrificedMediator.NetworkInput.ClientObjectiveChannelCompletedRpc(sacrificedMediator.PlayerId);
    }

    public void CompleteSacrifice(CharacterMediator sacrificedMediator)
    {
        soundPlayer.RequestPlaySound(transform, channelEnd, true);

        SortOutHealthCost(sacrificedMediator);
        SacrificesRemaining--;
        if (SacrificesRemaining == 0)
        {
            stateManager.ObjectiveCaptured(sacrificedMediator.playerData.Team);
            particleSpawner.SetActive(true);
        }
    }

    private void SortOutHealthCost(CharacterMediator sacrificedMediator)
    {
        if (!sacrificedMediator.IsOwner) return;

        sacrificedMediator.NetworkInput.TakeDamage(GetCurrentCost(sacrificedMediator), DamageTag.Neutral);
    }

    public int GetCurrentCost(CharacterMediator sacrificedMediator)
    {
        if (sacrificedMediator.Role == Role.Attacker
            || sacrificedMediator.BloodManager.BloodPickedUp)
        {
            var damage = Mathf.Min(
                   sacrificedMediator.HealthComponent.CurrentHealth - 1,
                   SacrificeCostAttackerBlood
               );
            return damage;
        }
        else
        {
            return SacrificeCost;
        }
    }

    public bool SafeToSacrifice(CharacterMediator mediator)
    {
        return mediator.HealthComponent.CurrentHealth > GetCurrentCost(mediator)
            || SacrificesRemaining == 1;
    }

    public void Reset()
    {
        SacrificesRemaining.Set(SacrificesRequired);
        particleSpawner.SetActive(false);
    }
}
