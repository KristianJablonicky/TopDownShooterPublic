using UnityEngine;

public class PlayerCameraSoundPlayer : SingletonMonoBehaviour<PlayerCameraSoundPlayer>
{
    [SerializeField] private SoundPlayer soundPlayer;
    public void PlaySound(AudioClip clip, bool randomizePitch)
    {
        soundPlayer.RequestPlaySound(transform, clip, randomizePitch);
    }
}
