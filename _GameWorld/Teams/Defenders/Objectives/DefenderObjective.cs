using UnityEngine;

public class DefenderObjective : SingletonMonoBehaviour<DefenderObjective>, IResettable
{
    [Header("Objective Settings")]
    [SerializeField] private float channelDuration = 2f;
    [SerializeField] private float speedSlowWhileChanneling = 0.5f;
    [field: SerializeField] public int SacrificeCost { get; private set; } = 50;
    [field: SerializeField] public int SacrificeCostAttackerBlood { get; private set; } = 15;
    [field: SerializeField] public int SacrificesRequired { get; private set; } = 3;

    [SerializeField] private AudioClip beep;
    
    [Header("References")]
    [SerializeField] private SoundPlayer soundPlayer;
    [SerializeField] private Collider2D sacrificeStartArea;
    [SerializeField] private ChaliceManager chaliceManager;
    [SerializeField] private GameObject particleSpawner;

    public ObservableValue<int> SacrificesRemaining { get; private set; }

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

        //manager.NewRoundStarted += GetActivated;
        //manager.RoundEnded += GetDeactivated;
    }


    public bool CanSacrifice(CharacterMediator mediator)
        => stateManager != null
        && stateManager.GameInProgress
        && !stateManager.RoundDecided
        && sacrificeStartArea.OverlapPoint(mediator.GetPosition());
    //&& mediator.InRange(transform.position, sacrificeRange, false);
    public void StartSacrifice(CharacterMediator sacrificedMediator)
    {
        var channeling = sacrificedMediator.Gun.ChannelingManager;
        if (channeling.Channeling) return;

        soundPlayer.RequestPlaySound(transform, beep, false);
        channeling.StartChannelingSlowedDown
        (
            channelDuration,
            () => CompleteSacrifice(sacrificedMediator),
            sacrificedMediator,
            speedSlowWhileChanneling,
            false
        );

        chaliceManager.StartChannelingAnimation(sacrificedMediator, channelDuration);
    }

    private void CompleteSacrifice(CharacterMediator sacrificedMediator)
    {
        if (!sacrificedMediator.IsAlive
            || stateManager.RoundDecided) return;
        SortOutHealthCost(sacrificedMediator);
        soundPlayer.RequestPlaySound(transform, beep, true);

        SacrificesRemaining--;
        if (SacrificesRemaining == 0)
        {
            stateManager.ObjectiveCaptured(sacrificedMediator.playerData.Team);
            particleSpawner.SetActive(true);
        }
    }

    private void SortOutHealthCost(CharacterMediator sacrificedMediator)
    {
        if (!sacrificedMediator.IsLocalPlayer) return;

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

    /*
    private IEnumerator CheckArea()
    {
        contestedTime = 0f;
        beepGoal = 1f;
        var wait = new WaitForSeconds(checkAreaInterval);

        var manager = GameStateManager.Instance;
        var attackers = manager.GetTeamDataByRole(Role.Attacker);
        var defenders = manager.GetTeamDataByRole(Role.Defender);
       

        while (true)
        {
            if (IsPlayerOfATeamInArea(attackers, mediator => mediator.Ascendance.HasAscended))
            {
                if (!IsPlayerOfATeamInArea(defenders, null))
                {
                    ProgressContest(Role.Attacker);
                }
            }
            else if (IsPlayerOfATeamInArea(defenders, mediator => mediator.BloodManager.BloodPickedUp))
            {
                if (!IsPlayerOfATeamInArea(attackers, null))
                {
                    ProgressContest(Role.Defender);
                }
            }
            else if (contestedTime > 0f)
            {
                objectiveArea.GetDeactivated();
                contestedTime = 0f;
                beepGoal = beepInterval;
            }

            yield return wait;
        }
    }

    private void ProgressContest(Role teamRole)
    {
        if (contestedTime == 0f)
        {
            objectiveArea.GetActivated();
            soundPlayer.RequestPlaySound(transform, beep, false);
        }
        contestedTime += checkAreaInterval;
        Debug.Log($"Area contested for {contestedTime:0.0} seconds.");

        if (contestedTime >= beepGoal)
        {
            soundPlayer.RequestPlaySound(transform, beep, false);
            beepGoal += beepInterval;
        }

        if (contestedTime >= Constants.objectiveCaptureTime)
        {
            Debug.Log($"Objective captured by {teamRole}s!");
            GameStateManager.Instance.ObjectiveCaptured(teamRole);
        }
    }

    private bool IsPlayerOfATeamInArea(TeamData team, Predicate<CharacterMediator> additionalCheck)
    {
        return team.Players.Any(p => IsPlayerInArea(p.Mediator, additionalCheck));
    }
    private bool IsPlayerInArea(CharacterMediator mediator, Predicate<CharacterMediator> additionalCheck)
    {
        bool passesExtraCheck = additionalCheck?.Invoke(mediator) ?? true;
        return mediator.IsAlive
            && passesExtraCheck
            && mediator.InRange(transform.position, objectiveArea.transform.localScale.x);
    }
    */
    public void Reset()
    {
        SacrificesRemaining.Set(SacrificesRequired);
        particleSpawner.SetActive(false);
    }
}
