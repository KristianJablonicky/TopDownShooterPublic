using UnityEngine;

public class VolumeAdjust : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    private void Awake()
    {
        audioSource.volume = DataStorage.GetVolume() * Constants.nonSpatialVolumeMultiplier;
    }
}
