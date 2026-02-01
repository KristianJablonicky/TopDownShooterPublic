using UnityEngine;

public class SmokeGameObject : DestroyOnRoundEnd
{
    [Header("Smoke Ability Settings")]
    [field: SerializeField] public float Duration { get; private set; } = 4f;
    [field: SerializeField] public float Radius { get; private set; } = 2.5f;
    [field: SerializeField] public float Range { get; private set; } = 8f;

    [Header("References")]
    [SerializeField] private GameObject guaranteedVision;
    [SerializeField] private ThrowSmoke abilitySO;
    [SerializeField] private FadeOutThenGetDestroyed fadeOutAnimation;
    [SerializeField] private SoundPlayer soundPlayer;
    [SerializeField] private AudioClip soundClip;

    private void Awake()
    {
        transform.localScale = Vector3.one * Radius;
        soundPlayer.RequestPlaySound(transform, soundClip, false);
        fadeOutAnimation.PlayAnimation(Duration);
        guaranteedVision.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent(out HealthComponent healthComponent))
        {
            if (!healthComponent.Mediator.IsLocalPlayer) return;
            guaranteedVision.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent(out HealthComponent healthComponent))
        {
            if (!guaranteedVision.activeSelf
                && !healthComponent.Mediator.IsLocalPlayer) return;
            guaranteedVision.SetActive(false);
        }
    }
}
