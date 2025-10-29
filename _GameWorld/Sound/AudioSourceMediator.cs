using UnityEngine;

public class AudioSourceMediator : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)] private float differentFloorVolumeRatio = 0.5f;
    [SerializeField] private AudioSource audioSource, audioSourceMuffled;

    public void Init(float baseVolume)
    {
        audioSource.volume = baseVolume;
        audioSourceMuffled.volume = baseVolume * differentFloorVolumeRatio;
    }


    public void PlaySound(AudioClip clip, bool randomizePitch)
    {
        PlaySound(clip, audioSource, randomizePitch); 
    }

    public void PlaySound(AudioClip clip, bool soundOnThisFloor, Vector2 destinationPosition, bool randomizePitch)
    {
        transform.position = destinationPosition;

        var source = soundOnThisFloor ? audioSource : audioSourceMuffled;
        PlaySound(clip, source, randomizePitch);
    }

    private void PlaySound(AudioClip clip, AudioSource source, bool randomizePitch)
    {
        source.pitch = GetPitch(randomizePitch);
        source.PlayOneShot(clip);
    }

    private float GetPitch(bool randomizePitch)
    {
        if (randomizePitch)
        {
            return Random.Range(0.95f, 1.05f);
        }
        return 1f;
    }
}
