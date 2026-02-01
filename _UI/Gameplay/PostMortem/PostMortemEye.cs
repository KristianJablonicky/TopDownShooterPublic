using UnityEngine;

public class PostMortemEye : MonoBehaviour
{
    [SerializeField] private GameObject eyeVisuals;
    [SerializeField] private OneTimeAnimation eyeAnimations;

    private void Awake()
    {
        SetVisibility(false);
        PlayerNetworkInput.PlayerSpawned += OnPlayerSpawn;
    }

    private void OnPlayerSpawn(CharacterMediator mediator)
    {
        mediator.Died += OnDeath;
        mediator.Respawned += OnRespawn;

        var ability = mediator.AbilityManager.AbilityPostMortem;
        ability.AbilityBecameReady += () => PlayEyeAnimation(true);
        ability.AbilityCast += () => PlayEyeAnimation(false);

    }

    private void OnDeath(CharacterMediator mediator)
    {
        SetVisibility(true);
        PlayEyeAnimation(false);
    }

    private void OnRespawn(CharacterMediator mediator)
    {
        SetVisibility(false);
    }

    private void SetVisibility(bool visibility)
    {
        eyeVisuals.SetActive(visibility);
    }

    private void PlayEyeAnimation(bool open)
    {
        eyeAnimations.PlayAnimation(reverseOrder: open);
    }
}
