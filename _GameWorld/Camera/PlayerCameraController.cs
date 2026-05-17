using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public sealed class PlayerCameraController : CameraController
{
    [SerializeField] private Camera cameraComponent, teamMateCamera, uiCamera;
    [SerializeField] private RawImage rawImage;
    [SerializeField] private CopyRotation audioListener;
    [SerializeField] private GameObject teamMateView;

    [Header("ADS settings")]
    [SerializeField] private float minOffset = 2f;
    [SerializeField] private float maxOffset = 5f, scrollStep = 1f;

    [Header("outside ADS settings")]
    [SerializeField] private float minZoom = 1f;
    [SerializeField] private float maxZoom = 1.5f, scrollStepZoom = 0.1f;

    [Header("Post Mortem References")]
    [SerializeField] private RenderTexture playerRenderTexture;
    [SerializeField] private RenderTexture teamMateRenderTexture;
    [SerializeField, Range(1f, 2f)] private float teammateViewZoom = 1.5f;
    private int startPPU, zoomedPPU;
    [SerializeField] private PixelPerfectCamera teammatePixelPerfectCamera;

    private float currentOffset, currentZoom = 1f;

    private bool zoomed = false;
    private PlayerInputHandler input;
    private RotationController rotationController;

    protected override void Awake()
    {
        base.Awake();
        PlayerNetworkInput.PlayerSpawned += OnOwnerSpawned;
        UpdateZoom((minZoom + maxZoom) / 2f);
    }

    private void OnOwnerSpawned(CharacterMediator mediator)
    {
        rotationController = mediator.RotationController;
        input = mediator.InputHandler;
        input.SetCamera(cameraComponent, uiCamera, rawImage);

        mediator.InputHandler.AimedDownSights += OnAimChange;
        mediator.InputHandler.ScrolledWheelUp += OnWheelScroll;
        currentOffset = minOffset;

        HandleRelativeSounds();

        mediator.Ascendance.SpiritLeft += OnDeath;
        mediator.Respawned += OnRespawn;

        GameStateManager.Instance.GameStarted += OnGameStart;
    }

    private void HandleRelativeSounds()
    {
        OnRelativeSoundsChanged(
            DataStorage.Instance.SubscribeAndGetCurrentValue(
                SettingsKeys.RelativeSounds, OnRelativeSoundsChanged, Constants.Defaults.relativeAudio
        ));
        var relative = DataStorage.Instance.GetInt(DataKeyInt.SettingsRelativeSounds);
    }

    private void OnDestroy()
    {
        DataStorage.Instance.Unsubscribe(SettingsKeys.RelativeSounds, OnRelativeSoundsChanged);
    }

    private void OnRelativeSoundsChanged(int newState)
    {
        if (newState == 1)
        {
            audioListener.enabled = true;
            audioListener.Copy(rotationController.gameObject);
        }
        else
        {
            audioListener.gameObject.transform.rotation = Quaternion.identity;
            audioListener.enabled = false;
        }
    }


    private bool gameInProgress = false;
    private CharacterMediator teamMate;
    private void OnGameStart()
    {
        gameInProgress = true;
        teamMate = CharacterManager.Instance.LocalPlayer.GetTeamMate().Mediator;
        teamMate.Ascendance.SpiritLeft += OnTeamMateDeath;

        startPPU = teammatePixelPerfectCamera.assetsPPU;
        zoomedPPU = Mathf.RoundToInt(startPPU * teammateViewZoom);

        HandleAllyPPU(true);

    }
    private void OnDeath(CharacterMediator mediator)
    {
        if (!gameInProgress) return;
        if (teamMate.IsAlive)
        {
            HandleAllyPPU(false);

            teamMateCamera.targetTexture = playerRenderTexture;
            teamMateView.SetActive(false);

            // AudioListener related changes
            audioListener.gameObject.transform.rotation = Quaternion.identity;
            followedGO = teamMate.MovementController.gameObject;

        }
        teamMate.PlayerVision.SwitchLights(true);
    }
    private void HandleAllyPPU(bool zoom)
    {
        if (zoom)
        {
            teammatePixelPerfectCamera.assetsPPU = zoomedPPU;
        }
        else
        {
            teammatePixelPerfectCamera.assetsPPU = startPPU;
        }
    }
    private void OnRespawn(CharacterMediator mediator)
    {
        if (!gameInProgress) return;
        HandleAllyPPU(true);
        teamMateCamera.targetTexture = teamMateRenderTexture;
        teamMate.PlayerVision.SwitchLights(false);
        teamMateView.SetActive(true);

        followedGO = mediator.MovementController.gameObject;
    }

    private void OnTeamMateDeath(CharacterMediator mediator)
    {
        teamMateView.SetActive(false);
    }


    private Vector2 playerCursorDelta, direction, castPosition;
    private float magnitude;

    public void OnAimChange(bool zoom)
    {
        zoomed = zoom;
        if (zoom)
        {
            updateAction = UpdatePositionZoomed;
        }
        else
        {
            updateAction = UpdatePosition;
        }
    }
    private void OnWheelScroll(bool up)
    {
        var direction = up ? 1f : -1f;
        if (zoomed)
        {
            var newZoom = currentOffset + direction * scrollStep;
            currentOffset = Mathf.Clamp(newZoom, minOffset, maxOffset);
        }
        else
        {
            var newZoom = currentZoom + direction * scrollStepZoom;
            newZoom = Mathf.Clamp(newZoom, minZoom, maxZoom);
            UpdateZoom(newZoom);
        }
    }

    private void UpdateZoom(float newZoom)
    {
        currentZoom = newZoom;
        renderUIImage.transform.localScale = new Vector2(newZoom, newZoom);
    }
    [SerializeField] private float magnitudeMultiplier = 0.25f;
    private (float, float) UpdatePositionZoomed()
    {
        castPosition = (Vector2)followedGO.transform.position;
        playerCursorDelta = (PlayerInputHandler.CursorPosition - castPosition);
        direction = playerCursorDelta.normalized;

        magnitude = playerCursorDelta.magnitude * magnitudeMultiplier;
        magnitude = magnitude > currentOffset
            ? currentOffset : magnitude;

        var t = transform.position.WithXY(
            Vector2.MoveTowards(transform.position,
                castPosition + (direction * magnitude),
                0.5f * maxDistanceDelta * Time.deltaTime
            )
        );
        return (t.x, t.y);
    }

    /*
    private float rotation;
    private const float rotationSpeed = 90f;
    void Update()
    {
        if (Input.GetKey(KeyCode.Q)) ChangeRotation(rotationSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.E)) ChangeRotation(-rotationSpeed * Time.deltaTime);
        if (Input.GetKeyDown(KeyCode.X)) ChangeRotation(0, true);
    }
    private void ChangeRotation(float newRotation, bool set = false)
    {
        if (set)
        {
            rotation = newRotation;
        }
        else
        {
            rotation += newRotation;
        }
        transform.rotation = Quaternion.Euler(0f, 0f, rotation);

        player.MovementController.SetEulerAngleZ(rotation);
    }
    */
}
