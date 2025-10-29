using UnityEngine;
using UnityEngine.UI;

public class MainMenuSettings : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Toggle relativeSounds;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] sampleClips;

    [SerializeField] private float intervalBetweenSounds = 1f;
    private float currentTimeWindow = 0;

    private void Awake()
    {
        var storage = DataStorage.Instance;
        volumeSlider.value = storage.GetInt(DataKeyInt.SettingsVolume);
        relativeSounds.isOn = storage.GetInt(DataKeyInt.SettingsRelativeSounds) == 1;

        volumeSlider.onValueChanged.AddListener(OnSliderDragged);
        currentTimeWindow = intervalBetweenSounds * 0.5f;
    }

    private void OnSliderDragged(float volume)
    {
        audioSource.volume = volume / 100f * Constants.maxVolume * Constants.nonSpatialVolumeMultiplier;
        currentTimeWindow += Time.deltaTime;
        if (currentTimeWindow >= intervalBetweenSounds)
        {
            currentTimeWindow -= intervalBetweenSounds;
            audioSource.PlayOneShot(sampleClips[Random.Range(0, sampleClips.Length)]);
        }

    }

    private void OnDestroy()
    {
        var storage = DataStorage.Instance;
        storage.SetInt(DataKeyInt.SettingsVolume, (int)volumeSlider.value);
        storage.SetInt(DataKeyInt.SettingsRelativeSounds, relativeSounds.isOn ? 1 : 0);
    }
}
