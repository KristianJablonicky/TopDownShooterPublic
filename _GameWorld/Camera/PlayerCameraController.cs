using UnityEngine;
using UnityEngine.UI;

public sealed class PlayerCameraController : CameraController
{
    [SerializeField] private Camera cameraComponent, teamMateCamera, uiCamera;
    [SerializeField] private RawImage rawImage;

    [Header("ADS settings")]
    [SerializeField] private float minOffset = 2f;
    [SerializeField] private float maxOffset = 5f, scrollStep = 1f;

    [Header("outside ADS settings")]
    [SerializeField] private float minZoom = 1f;
    [SerializeField] private float maxZoom = 1.5f, scrollStepZoom = 0.1f;

    [Header("Post Mortem References")]
    [SerializeField] private RenderTexture playerRenderTexture;
    [SerializeField] private RenderTexture teamMateRenderTexture;

    private float currentOffset, currentZoom = 1f;

    private bool zoomed = false;
    private PlayerInputHandler input;
    protected override void Awake()
    {
        base.Awake();
        PlayerNetworkInput.OwnerSpawned += OnOwnerSpawned;
    }

    private void OnOwnerSpawned(CharacterMediator mediator)
    {
        mediator.InputHandler.SetCamera(cameraComponent, uiCamera, rawImage);
        input = mediator.InputHandler;

        mediator.InputHandler.AimedDownSights += OnAimChange;
        mediator.InputHandler.ScrolledWheelUp += OnWheelScroll;
        currentOffset = minOffset;

        mediator.Died += OnDeath;
        mediator.Respawned += OnRespawn;
    }
    private void OnDeath(CharacterMediator mediator)
    {
        if (!GameStateManager.Instance.GameInProgress) return;
        var teamMate = mediator.playerData.Team.GetTeamMate(mediator.playerData).Mediator;
        if (teamMate.IsAlive)
        {
            teamMateCamera.targetTexture = playerRenderTexture;
        }
        teamMate.PlayerVision.SwitchLights(true);
    }

    private void OnRespawn(CharacterMediator mediator)
    {
        if (!GameStateManager.Instance.GameInProgress) return;
        teamMateCamera.targetTexture = teamMateRenderTexture;
        var teamMate = mediator.playerData.Team.GetTeamMate(mediator.playerData).Mediator;
        teamMate.PlayerVision.SwitchLights(false);
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
            currentZoom = newZoom;

            renderUIImage.transform.localScale = new Vector2(newZoom, newZoom);
        }
    }

    private (float, float) UpdatePositionZoomed()
    {
        castPosition = (Vector2)followedGO.transform.position;
        playerCursorDelta = (input.CursorPosition - castPosition);
        direction = playerCursorDelta.normalized;
        magnitude = playerCursorDelta.magnitude > currentOffset ? currentOffset : magnitude;

        var t = transform.position.WithXY(
            Vector2.MoveTowards(transform.position,
                castPosition + (direction * magnitude),
                maxDistanceDelta * Time.deltaTime
            )
        );
        return (t.x, t.y);
    }
}
