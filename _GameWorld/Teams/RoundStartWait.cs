using System;
using UnityEditor;
using UnityEngine;

public class RoundStartWait : SingletonMonoBehaviour<RoundStartWait>
{
    [SerializeField] private float delayAttackers = 8f, delayDefenders = 3f;
    public event Action<float> OnRoundStartWait;

    private GameStateManager gameStateManager;

    private void Start()
    {
        gameStateManager = GameStateManager.Instance;
        gameStateManager.NewRoundStarted += OnNewRound;
    }

    private void OnNewRound()
    {
        foreach (var mediator in CharacterManager.Instance.Mediators.Values)
        {
            if (!mediator.IsOwner) continue;
            
            var delay = mediator.Role == Role.Attacker ? delayAttackers : delayDefenders;
            DisableActions(delay, mediator);
            if (mediator.IsLocalPlayer)
            {
                OnRoundStartWait?.Invoke(delay);
            }
        }
    }

    private void DisableActions(float delay, CharacterMediator mediator)
    {
        mediator.Gun.ChannelingManager.Reset();
        // To avoid lovely stuff like Dracula spawn killing
        mediator.AbilityManager.DisableAbilities(delay, delayAttackers);
        mediator.Gun.ChannelingManager.StartChannelingStandingStill(delay, null, mediator, false);
    }
}
