using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static AimDirection;

public class PlayerInputHandler : InputHandlerBase
{
    private RawImage rawImage;
    private Camera uiCamera;
    private Camera playerCamera, teamMateCamera;

    [SerializeField] private RenderTexture renderTexture;
    [SerializeField] private MovementController movementController;

    [SerializeField] private float cursorAccuracyMultiplier = 0.1f;
    [SerializeField] private float pingCoolDown = 2.5f, emoteCoolDown = 3f;

    private AbilityManager abilityManager;

    public event Action<bool> AimedDownSights, ScrolledWheelUp;
    public event Action<AimDirection> AimDirectionChanged;
    public event Action EscapePressed;
    private bool aimingDownSights = false;
    private bool ADSSetToToggle;

    public static Vector2 CursorPosition { get; private set; }
    private Action updateAction;
    private Action conditionalAction;

    private CommunicationCoolDown pingCoolDownHandler, emoteCoolDownHandler;

    protected override void Awake()
    {
        base.Awake();
        enabled = false;
        updateAction = UpdateInputAlive;
        mediator.Died += (_) => OnDeath();
        mediator.Ascendance.SpiritLeft += (_) => OnSpiritLeft();
        mediator.Respawned += (_) => OnRespawned();

        pingCoolDownHandler = new(pingCoolDown);
        emoteCoolDownHandler = new(emoteCoolDown);
    }

    public void Init()
    {
        enabled = true;
        SetPlayerPreferences();
        abilityManager = mediator.AbilityManager;
        GameStateManager.Instance.GameStarted += OnGameStart;
    }

    private void SetPlayerPreferences()
    {
        ADSSetToToggle = Settings.AdsSetToToggle;
    }
    public void SetCamera(Camera camera, Camera ui, RawImage rawImage)
    {
        playerCamera = camera;
        uiCamera = ui;
        this.rawImage = rawImage;
    }
    public void SetTeamMateCamera(Camera teamMateCamera) => this.teamMateCamera = teamMateCamera;

    private void OnDeath()
    {
        gameObject.SetActive(true);
        updateAction = UpdateAlwaysAvailable;
        SetAimDirection(Straight);
    }
    private bool postMortem = false;
    private CharacterMediator teamMate;
    private void OnSpiritLeft()
    {
        updateAction = UpdateInputPostMortem;
        if (teamMate == null
        &&  GameStateManager.Instance.GameInProgress)
        {
            teamMate = mediator.GetTeamMate();
        }
        postMortem = true;
    }
    private void OnRespawned()
    {
        updateAction = UpdateInputAlive;
        postMortem = false;
        SetAimDirection(Straight);
    }

    private void Update()
    {
        updateAction?.Invoke();
    }
    public void UpdateInputAlive()
    {
        UpdateMovement();
        UpdateMouse();
        UpdateUniqueActions();
        UpdateAbilities();

#if UNITY_EDITOR
        UpdateDebug();
#endif
        UpdateObjectiveInput();

        UpdateAlwaysAvailable();
    }

    public void UpdateInputPostMortem()
    {
        UpdateMousePostMortem();
        UpdateAbility(abilityManager.AbilityPostMortem, true);
        UpdateAlwaysAvailable();
    }

    private void ChangeOnHoldState(KeyCode keyCode, Action<bool> action)
    {
        var holdState = Held(keyCode);
        if (holdState.HasValue)
        {
            action(holdState.Value);
        }
    }

    private bool? Held(KeyCode keyCode)
    {
        if (Input.GetKeyDown(keyCode))
        {
            return true;
        }
        else if (Input.GetKeyUp(keyCode))
        {
            return false;
        }
        return null;
    }

    private void UpdateMovement()
    {
        var moveX = Input.GetAxisRaw("Horizontal");
        var moveY = Input.GetAxisRaw("Vertical");
        movementController.WalkInDirection(moveX, moveY);
    }

    private void UpdateMouse()
    {
        var newPosition = GetCursorPosition();
        var magnitude = (newPosition - CursorPosition).magnitude;
        
        // only apply cursor movement inaccuracy if the player hasn't moved across floors.
        if (magnitude < Constants.floorYOffset * 0.9f)
        {
            gun.ApplyRecoilMouseMovement(magnitude * cursorAccuracyMultiplier);
        }
        CursorPosition = newPosition;

        rotationController.SetCursorPosition(CursorPosition);
        
        if (Input.GetMouseButtonDown(0))
        {
            Shoot(CursorPosition, true);
        }
        else if (Input.GetMouseButton(0))
        {
            Shoot(CursorPosition, false);
        }

        if (ADSSetToToggle)
        {
            if (Input.GetMouseButtonDown(1))
            {
                aimingDownSights = !aimingDownSights;
                AimedDownSights?.Invoke(aimingDownSights);
            }
        }
        else
        {
            ChangeOnHoldState(KeyCode.Mouse1, AimedDownSights.Invoke);
        }

        HandleZoom();
    }

    private void UpdateMousePostMortem()
    {
        HandleZoom();
        ChangeOnHoldState(KeyCode.LeftControl, OnDirectionChange);
    }

    private void HandleZoom()
    {
        var scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            ChangeZoom(scroll > 0f);
        }
    }

    private void ChangeZoom(bool scrolledUp)
    {
        ScrolledWheelUp?.Invoke(scrolledUp);
    }

    private void OnDirectionChange(bool pressedDown)
    {
        CharacterMediator handlingMediator = !postMortem ? mediator : teamMate;
        if (!pressedDown)
        {
            SetAimDirection(Straight);
            // unsubscribe twice just in case you die subscribed
            handlingMediator.MovementController.FloorChanged -= OnFloorChange;
            handlingMediator.MovementController.FloorChanged -= OnFloorChange;
            return;
        }
        SetAimDirection(FloorUtilities.GetOtherFloorDirection(handlingMediator));

        handlingMediator.MovementController.FloorChanged += OnFloorChange;
    }

    private void OnFloorChange(Floor newFloor)
        => SetAimDirection(FloorUtilities.GetOtherFloorDirection(newFloor));

    private void SetAimDirection(AimDirection newAimDirection)
    {
        if (AimDirection == newAimDirection) return;

        AimDirection = newAimDirection;
        AimDirectionChanged?.Invoke(newAimDirection);
    }
    public Vector2 GetCursorPositionNormalized()
    {
        return new Vector2(
            (Input.mousePosition.x / Screen.width - 0.5f) * 2f,
            (Input.mousePosition.y / Screen.height - 0.5f) * 2f
        ).normalized;
    }
    private Vector2 GetCursorPosition(Camera camera = null)
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rawImage.rectTransform,
                Input.mousePosition,
                uiCamera,
                out Vector2 localPoint))
        {
            return Vector2.zero;
        }
        
        var rect = rawImage.rectTransform.rect;
        localPoint.x = Mathf.Clamp(localPoint.x, rect.xMin, rect.xMax);
        localPoint.y = Mathf.Clamp(localPoint.y, rect.yMin, rect.yMax);

        var u = (localPoint.x - rect.x) / rect.width;
        var v = (localPoint.y - rect.y) / rect.height;

        var px = u * renderTexture.width;
        var py = v * renderTexture.height;

        if (camera == null)
        {
            camera = playerCamera;
        }

        return camera.ScreenToWorldPoint(new(px, py, playerCamera.nearClipPlane));
    }

    private void UpdateUniqueActions()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (!gun.CanReload()) return;
            networkInput.RequestReloadRpc();
        }

        ChangeOnHoldState(KeyCode.LeftControl, OnDirectionChange);
    }

    private void UpdateAbilities()
    {
        UpdateAbility(abilityManager.MovementAbility, false);
        UpdateAbility(abilityManager.UtilityAbility, false);
    }
    private void UpdateAbility(ActiveAbility ability, bool postMortem)
    {
        if (postMortem) CursorPosition = GetCursorPosition(teamMateCamera);
        if (!ability.ReadyToCast) return;

        //if (postMortem) CursorPosition = GetCursorPositionNormalized();


        ChangeOnHoldState((KeyCode)ability.KeyCode, (pressed) => ability.OnKeyInteraction(pressed, CursorPosition));
    }


    private void UpdateObjectiveInput()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (DefenderObjective.Instance.CanSacrifice(mediator))
            {
                networkInput.RequestObjectiveSacrifice();
            }
        }
    }

    private void UpdateAlwaysAvailable()
    {
        ChangeOnHoldState(KeyCode.Tab, ScoreBoard.Instance.ChangeState);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            EscapePressed?.Invoke();
        }

        if (pingCoolDownHandler.Ready && Input.GetKeyDown(KeyCode.Mouse2))
        {
            networkInput.RequestPingRpc(CursorPosition, AimDirection, mediator.PlayerId);
            pingCoolDownHandler.Use();
        }

        if (emoteCoolDownHandler.Ready)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) Emote(EmoteType.positive);
            else if (Input.GetKeyDown(KeyCode.Alpha2)) Emote(EmoteType.negative);
            else if (Input.GetKeyDown(KeyCode.Alpha3)) EmotePrivately(EmoteType.positive);
            else if (Input.GetKeyDown(KeyCode.Alpha4)) EmotePrivately(EmoteType.negative);
        }

        conditionalAction?.Invoke();
    }

    private void Emote(EmoteType emoteType)
    {
        networkInput.EmoteRpc(mediator.PlayerId, emoteType);
        emoteCoolDownHandler.Use();
    }

    private void EmotePrivately(EmoteType emoteType)
    {
        var teamMate = mediator.GetTeamMate();
        if (teamMate == null) return;
        networkInput.EmoteTeam(mediator.PlayerId, teamMate.PlayerId, emoteType);
        emoteCoolDownHandler.Use();
    }

    private const KeyCode movementHotKey = KeyCode.LeftShift, utilityHotKey = KeyCode.F;
    private void OnGameStart()
    {
        var teamMate = mediator.GetTeamMate();
        if (!teamMate.IsBot) return;
        ChangeCommandBotSubscription(true);
        teamMate.Died += (_) => ChangeCommandBotSubscription(false);
        teamMate.RespawnedAfterDying += (_) => ChangeCommandBotSubscription(true);
    }

    private void ChangeCommandBotSubscription(bool subscribe)
    {
        if (subscribe)
        {
            conditionalAction += CommandBot;
        }
        else
        {
            conditionalAction -= CommandBot;
        }
    }

    private void CommandBot()
    {
        CheckAbility(AbilityType.Movement, movementHotKey);
        CheckAbility(AbilityType.Utility, utilityHotKey);
    }

    private void CheckAbility(AbilityType type, KeyCode abilityHotKey)
    {
        if (Input.GetKeyDown(abilityHotKey))
        {
            networkInput.RequestAbilityCastRpc(type, true, CursorPosition);
        }
        else if (Input.GetKeyUp(abilityHotKey))
        {
            networkInput.RequestAbilityCastRpc(type, false, CursorPosition);
        }
    }

    private class CommunicationCoolDown
    {
        public bool Ready { get; private set; } = true;
        private float coolDown;
        public CommunicationCoolDown(float coolDown)
        {
            this.coolDown = coolDown;
        }

        public void Use()
        {
            if (!Ready) return;
            Ready = false;
            Invoker.Instance.ExecuteAfterDelay(coolDown, () => Ready = true);
        }
    }

    private void UpdateDebug()
    {
        if (Input.GetKeyDown(KeyCode.L)) mediator.NetworkInput.DealDamage(100, DamageTag.Neutral, mediator, mediator, false);
        if (Input.GetKeyDown(KeyCode.H))
        {
            mediator.NetworkInput.DealDamage(30, DamageTag.Neutral, mediator, mediator, false);
        }
        /*
        if (Input.GetKeyDown(KeyCode.G))
        {
        }
        */
    }
}

public enum AimDirection
{
    Down,
    Straight,
    Up
}
