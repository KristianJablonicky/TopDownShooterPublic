using UnityEngine;

public class CardPileSoundManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CardSpawner cardSpawner;
    [SerializeField] private SoundPlayer soundPlayer;

    [Header("Card Pile Sounds")]
    [SerializeField] private AudioClip pickedUp, putDown;
    [SerializeField] private AudioClip[] shuffled;


    private void Start()
    {
        var manager = HeroSelectionManager.Instance;
        manager.HeroCardPickedUp += () => PlaySound(pickedUp);
        manager.HeroCardBeingPutDown += () => PlaySound(putDown);
        cardSpawner.CardShuffleStarted += (_) => PlaySound(shuffled);
    }

    private void PlaySound(AudioClip clip)
    {
        soundPlayer.RequestPlaySound(transform, clip, false);
    }
    private void PlaySound(AudioClip[] clips)
    {
        soundPlayer.RequestPlaySound(transform, clips, false);
    }
}
