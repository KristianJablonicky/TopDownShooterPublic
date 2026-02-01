using UnityEngine;

public class DamageFlickerUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField, Range(0f, 1f)] private float
        redBorderThreshold = 0.25f,
        minAlphaWhenBelowThreshold = 0.25f,
        maxIntensityOnHit = 0.75f;
    [SerializeField] float duration = 0.25f;
    private int baseMaxHealth;
    private CharacterMediator owner;

    private void Awake()
    {
        PlayerNetworkInput.PlayerSpawned += OnPlayerSpawn;
    }

    private void OnPlayerSpawn(CharacterMediator mediator)
    {
        owner = mediator;
        baseMaxHealth = mediator.HealthComponent.MaxHealth;
        lastValue = baseMaxHealth;
        mediator.HealthComponent.CurrentHealth.OnValueSet += OnHealthChange;
    }
    private int lastValue;
    private void OnHealthChange(int currentHealth)
    {
        var healthThreshold = baseMaxHealth * redBorderThreshold;
        var minIntensity = 0f;
        if (currentHealth != 0 &&
            currentHealth < healthThreshold)
        {
            minIntensity = minAlphaWhenBelowThreshold * (1f - currentHealth / healthThreshold);
        }
        
        if (lastValue > currentHealth          // took damage
            && currentHealth != baseMaxHealth) // and it wasn't a heal on round start
        {
            FlashDamage(minIntensity, Mathf.Abs(currentHealth - lastValue) / (float)baseMaxHealth);
        }
        else if (lastValue != currentHealth)
        {
            StopAllCoroutines();
            canvasGroup.alpha = minIntensity;
        }
        lastValue = currentHealth;
    }

    private void FlashDamage(float minIntensity, float damageIntensity)
    {
        var maxIntensity = minIntensity + damageIntensity * maxIntensityOnHit;
        StopAllCoroutines();
        StartCoroutine
        (
            Tweener.TweenCoroutine
            (
                this, minIntensity, maxIntensity, duration,
                TweenStyle.sinusPingPong,
                value => canvasGroup.alpha = value
            )
        );
    }
}
