using UnityEngine;

public class VolumeAdjust : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    private void Awake()
    {
        OnVolumeChanged(
            DataStorage.Instance.SubscribeAndGetCurrentValue(
                SettingsKeys.MasterVolume, OnVolumeChanged, Constants.Defaults.volume)
        );
    }

    private void OnDestroy()
    {
        DataStorage.Instance.Unsubscribe(SettingsKeys.MasterVolume, OnVolumeChanged);
    }

    private void OnVolumeChanged(int newVolume)
    {
        audioSource.volume = DataStorage.VolumeToFloat(newVolume);
    }
}
