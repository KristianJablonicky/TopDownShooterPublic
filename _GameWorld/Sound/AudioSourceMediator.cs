using UnityEngine;

public class AudioSourceMediator : MonoBehaviour, IResettable
{
    [SerializeField, Range(0f, 1f)] private float differentFloorVolumeRatio = 0.5f;
    [SerializeField] private AudioSource audioSource, audioSourceMuffled;
    [SerializeField, Tooltip("Don't mind this!")] private string clipName;
    public float DefaultVolume { get; private set; }
    public void SetVolume(float baseVolume, bool isDefault)
    {
        audioSource.volume = baseVolume;
        audioSourceMuffled.volume = baseVolume * differentFloorVolumeRatio;
        if (isDefault) DefaultVolume = baseVolume;
    }

    public void PlaySound(AudioClip clip, PitchAdjustment pitchAdjustment)
    {
        PlaySound(clip, audioSource, pitchAdjustment); 
    }

    public void PlaySound(AudioClip clip, bool soundOnThisFloor, Vector2 destinationPosition, PitchAdjustment pitchAdjustment)
    {
        transform.position = destinationPosition;

        var source = soundOnThisFloor ? audioSource : audioSourceMuffled;
        PlaySound(clip, source, pitchAdjustment);
    }

    private void PlaySound(AudioClip clip, AudioSource source, PitchAdjustment pitchAdjustment)
    {
        clipName = clip.name;
        source.pitch = pitchAdjustment.GetPitch();
        source.PlayOneShot(clip);
    }

    public void Stop()
    {
        audioSource.Stop();
        audioSourceMuffled.Stop();
    }
    public void Reset() => SetVolume(DefaultVolume, false);
}
