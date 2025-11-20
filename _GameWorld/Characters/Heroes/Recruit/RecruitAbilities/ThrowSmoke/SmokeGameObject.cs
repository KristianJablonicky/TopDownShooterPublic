using UnityEngine;

public class SmokeGameObject : DestroyOnRoundEnd
{
    [SerializeField] private GameObject guaranteedVision;
    [SerializeField] private ThrowSmoke abilitySO;
    [SerializeField] private FadeOutThenGetDestroyed fadeOutAnimation;
    [SerializeField] private SoundPlayer soundPlayer;
    [SerializeField] private AudioClip soundClip;

    private void Awake()
    {
        transform.localScale = Vector3.one * abilitySO.Radius;
        soundPlayer.RequestPlaySound(transform, soundClip, false);
        fadeOutAnimation.PlayAnimation(abilitySO.Duration);
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
