using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    [SerializeField] private AudioSourceMediator audioSourcePrefab;
    [SerializeField, Range(0f, 1f)] private float baseVolume = 1f;
    [SerializeField] private CharacterMediator followLocalPlayer;
    [SerializeField] private bool setAsParent = false;
    private bool following = false;
    private AudioSourceMediator audioSourceInstance;
    private CharacterMediator localPlayer;

    private float volumeMultiplier;

    private bool setUp = false;
    private bool waitingForLocalPlayer = false;
    private AudioClip toBePlayedWhenPlayerSpawns;
    private void Start()
    {
        if (setUp) return;
        setUp = true;

        OnVolumeChanged(
            DataStorage.Instance.SubscribeAndGetCurrentValue(
                SettingsKeys.MasterVolume, OnVolumeChanged, Constants.Defaults.volume)
        );

        var manager = CharacterManager.Instance;
        if (manager == null)
        {
            StartMainMenu();
            return;
        }

        var localPlayerInstance = manager.LocalPlayerMediator;
        if (localPlayerInstance == null) // Local player not yet spawned
        {
            PlayerNetworkInput.PlayerSpawned += PlayerInstanceObtained;
            waitingForLocalPlayer = true;
        }
        else // Local player already spawned - multiplayer setting
        {
            PlayerInstanceObtained(localPlayerInstance);
        }

    }

    private void PlayerInstanceObtained(CharacterMediator instance)
    {
        localPlayer = instance;
        waitingForLocalPlayer = false;

        audioSourceInstance = Instantiate(audioSourcePrefab);
        audioSourceInstance.SetVolume(baseVolume * volumeMultiplier, true);

        if (followLocalPlayer != null &&
            followLocalPlayer == instance)
        {
            following = true;
            audioSourceInstance.transform.SetParent(followLocalPlayer.GetTransform(), false);
            return;
        }
        if (setAsParent)
        {
            audioSourceInstance.transform.SetParent(transform, false);
        }


        localPlayer.MovementController.FloorChanged += OnFloorChanged;
        if (toBePlayedWhenPlayerSpawns != null)
        {
            RequestPlaySound(transform, toBePlayedWhenPlayerSpawns, false);
        }
    }

    private void OnVolumeChanged(int newVolume)
    {
        volumeMultiplier = DataStorage.VolumeToFloat(newVolume);
        if (audioSourceInstance != null)
        {
            audioSourceInstance.SetVolume(baseVolume * volumeMultiplier, true);
        }
    }

    private void OnFloorChanged(Floor newFloor)
    {
        FloorUtilities.ApplyYOffset(audioSourceInstance.transform, newFloor);
    }

    public void RequestPlaySound(Transform transform, AudioClip[] clips, bool randomizePitch)
    {
        PlaySound(transform, clips[Random.Range(0, clips.Length)], new(randomizePitch));
    }
    public void RequestPlaySound(Transform transform, AudioClip clip, bool randomizePitch)
    {
        PlaySound(transform, clip, new(randomizePitch));
    }

    public void RequestPlaySound(Transform transform, AudioClip clip, float pitchAdjustment)
    {
        PlaySound(transform, clip, new(pitchAdjustment));
    }

    private void PlaySound(Transform transform, AudioClip clip, PitchAdjustment adjustment)
    {
        if (following)
        {
            audioSourceInstance.PlaySound(
                clip,
                adjustment
            );
            return;
        }

        if (localPlayer == null)
        {
            Start();
            if (localPlayer == null)
            {
                // main menu
                if (!waitingForLocalPlayer)
                {
                    // Main menu - no player instance
                    audioSourceInstance.PlaySound(
                        clip,
                        true,
                        Vector2.zero,
                        adjustment
                    );
                }
                else
                {
                    // cache the clip for later
                    toBePlayedWhenPlayerSpawns = clip;
                }
                return;
            }
        }

        var localPlayerFloor = localPlayer.CurrentFloor;

        // use teammate's floor when dead (since the spectate camera follows them)
        if (localPlayer.Ascendance.HasSpiritLeftAlready)
        {
            var teamMate = localPlayer.GetTeamMate();
            if (teamMate != null)
            {
                localPlayerFloor = teamMate.CurrentFloor;
            }
        }
        
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
            adjustment
        );
    }

    private async void OnDestroy()
    {
        if (localPlayer != null)
        {
            localPlayer.MovementController.FloorChanged -= OnFloorChanged;
        }
        if (audioSourceInstance != null)
        {
            await TaskExtensions.Delay(3f);
            if (audioSourceInstance == null) return;
            Destroy(audioSourceInstance.gameObject);
        }

        PlayerNetworkInput.PlayerSpawned -= PlayerInstanceObtained;
        DataStorage.Instance.Unsubscribe(SettingsKeys.MasterVolume, OnVolumeChanged);
    }


    private Coroutine fadeCoroutine;
    public void FadeVolume(float start, float end, float duration)
    {
        start *= audioSourceInstance.DefaultVolume;
        end *= audioSourceInstance.DefaultVolume;
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            audioSourceInstance.Reset();
        }

        fadeCoroutine = StartCoroutine(
            Tweener.TweenCoroutine(this, start, end, duration,
            TweenStyle.quadratic,
            value => audioSourceInstance.SetVolume(value * baseVolume, false),
            onExit: () => {
                if (end == 0f)
                {
                    audioSourceInstance.Stop();
                }
                else
                {
                    audioSourceInstance.Reset();
                }
            })
        );
    }

    private void StartMainMenu()
    {
        audioSourceInstance = Instantiate(audioSourcePrefab);
        audioSourceInstance.SetVolume(baseVolume * volumeMultiplier, true);
    }
}

public class PitchAdjustment
{
    private float adjustment;
    private bool? randomize;
    public PitchAdjustment(float adjustment)
    {
        this.adjustment = adjustment;
    }
    public PitchAdjustment(bool randomize)
    {
        this.randomize = randomize;
    }

    public float GetPitch()
    {
        if (randomize.HasValue)
        {
            return Random.Range(0.95f, 1.05f);
        }
        return 1f + adjustment;
    }
}