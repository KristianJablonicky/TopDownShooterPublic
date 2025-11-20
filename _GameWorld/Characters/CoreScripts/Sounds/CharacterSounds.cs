using UnityEngine;

public class CharacterSounds : MonoBehaviour
{
    [SerializeField] private CharacterMediator mediator;
    [SerializeField] private SoundPlayer soundPlayer;

    [SerializeField] private AudioClip[] onHitSounds, onHeadShotSounds;


    private void Start()
    {
        mediator.HealthComponent.DamageTakenWithTags += OnDamageTaken;
    }

    private void OnDamageTaken(DamageTag tag)
    {
        if (tag == DamageTag.Shot)
        {
            PlayHitSound();
        }
        else if (tag == DamageTag.HeadShot)
        {
            PlayHeadShotSound();
        }
    }

    public void PlayHitSound()
    {
        soundPlayer.RequestPlaySound(transform, onHitSounds, true);
    }

    public void PlayHeadShotSound()
    {
        soundPlayer.RequestPlaySound(transform, onHeadShotSounds, true);
    }
}
