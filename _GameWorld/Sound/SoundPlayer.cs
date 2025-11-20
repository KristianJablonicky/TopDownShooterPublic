using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    [SerializeField] private AudioSourceMediator audioSourcePrefab;
    [SerializeField, Range(0f, 1f)] private float baseVolume = 1f;
    [SerializeField] private CharacterMediator followLocalPlayer;
    private bool following = false;
    private AudioSourceMediator audioSourceInstance;
    private CharacterMediator localPlayer;


    private bool setUp = false;
    private void Start()
    {
        if (setUp) return;
        baseVolume *= DataStorage.GetVolume();

        var localPlayerInstance = CharacterManager.Instance.LocalPlayerMediator;
        if (localPlayerInstance == null) // Local player not yet spawned
        {
            PlayerNetworkInput.PlayerSpawned += PlayerInstanceObtained;
        }
        else // Local player already spawned - multiplayer setting
        {
            PlayerInstanceObtained(localPlayerInstance);
        }

        setUp = true;
    }

    private void PlayerInstanceObtained(CharacterMediator instance)
    {
        localPlayer = instance;

        audioSourceInstance = Instantiate(audioSourcePrefab);
        audioSourceInstance.Init(baseVolume);

        if (followLocalPlayer != null &&
            followLocalPlayer == instance)
        {
            following = true;
            audioSourceInstance.transform.SetParent(transform, false);
            return;
        }


        localPlayer.MovementController.FloorChanged += OnFloorChanged;

    }

    private void OnFloorChanged(Floor newFloor)
    {
        FloorUtilities.ApplyYOffset(audioSourceInstance.transform, newFloor);
    }

    public void RequestPlaySound(Transform transform, AudioClip[] clips, bool randomizePitch)
    {
        PlaySound(transform, clips[Random.Range(0, clips.Length)], randomizePitch);
    }
    public void RequestPlaySound(Transform transform, AudioClip clip, bool randomizePitch)
    {
        PlaySound(transform, clip, randomizePitch);
    }

    private void PlaySound(Transform transform, AudioClip clip, bool randomizePitch)
    {
        if (following)
        {
            audioSourceInstance.PlaySound(
                clip,
                randomizePitch
            );
            return;
        }

        if (localPlayer == null) Start();

        var localPlayerFloor = localPlayer.CurrentFloor;
        var soundOnThisFloor = localPlayerFloor == FloorUtilities.GetCurrentFloor(transform);
        Vector2 targetPosition = transform.position;

        if (!soundOnThisFloor)
        {
            targetPosition = FloorUtilities.GetPositionY(
                targetPosition,
                localPlayerFloor
            );
        }

        audioSourceInstance.PlaySound(
            clip,
            soundOnThisFloor,
            targetPosition,
            randomizePitch
        );
    }

    private async void OnDestroy()
    {
        if (audioSourceInstance != null)
        {
            await TaskExtensions.Delay(3f);
            if (audioSourceInstance == null) return;
            Destroy(audioSourceInstance.gameObject);
        }
        if (localPlayer != null)
        {
            localPlayer.MovementController.FloorChanged -= OnFloorChanged;
        }

        PlayerNetworkInput.PlayerSpawned -= PlayerInstanceObtained;
    }
}
