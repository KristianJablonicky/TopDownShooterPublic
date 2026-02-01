using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using static AimDirection;

public class PlayerInputHandler : MonoBehaviour
{
    private RawImage rawImage;
    private Camera uiCamera;
    private Camera playerCamera, teamMateCamera;

    [SerializeField] private PlayerNetworkInput networkInput;
    [SerializeField] private RenderTexture renderTexture;
    [SerializeField] private CharacterMediator mediator;
    [SerializeField] private MovementController movementController;
    [SerializeField] private RotationController rotationController;
    [SerializeField] private Gun gun;

    [SerializeField] private float cursorAccuracyMultiplier = 0.1f;
    [SerializeField] private float pingCoolDown = 2.5f;

    private AbilityManager abilityManager;

    public AimDirection AimDirection { get; private set; } = Straight;

    public event Action<bool> AimedDownSights, ScrolledWheelUp;
    public event Action<AimDirection> AimDirectionChanged;
    public event Action EscapePressed;
    private bool aimingDownSights = false;
    private bool ADSSetToToggle;

    public Vector2 CursorPosition { get; private set; }
    private Action updateAction;

    private void Awake()
    {
        enabled = false;
        updateAction = UpdateInputAlive;
        mediator.Died += (_) => { gameObject.SetActive(true); updateAction = null; };
        mediator.Ascendance.SpiritLeft += (_) => { updateAction = UpdateInputPostMortem; };
        mediator.Respawned += (_) => updateAction = UpdateInputAlive;
    }

    public void Init()
    {
        enabled = true;
        SetPlayerPreferences();
        abilityManager = mediator.AbilityManager;
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
        // TODO: reconsider adding in walk
        //ChangeOnHoldState(KeyCode.LeftShift, movementController.Walk);

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
        
        if (!EscapeMenuManager.MenuOpen)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Shoot(CursorPosition, true);
            }
            else if (Input.GetMouseButton(0))
            {
                Shoot(CursorPosition, false);
            }
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

        bool heldControl = Input.GetKey(KeyCode.LeftControl);

        var scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
        {
            HandleScrolling(heldControl, true);
        }
        else if (scroll < 0f)
        {
            HandleScrolling(heldControl, false);
        }

        if (Input.GetKeyDown(KeyCode.Mouse2))
        {
            SetAimDirection(Straight);
        }
    }

    private void UpdateMousePostMortem()
    {
        var scroll = Input.GetAxis("Mouse ScrollWheel");
        var heldControl = Input.GetKey(KeyCode.LeftControl);
        if (scroll > 0f)
        {
            HandleScrolling(heldControl, true);
        }
        else if (scroll < 0f)
        {
            HandleScrolling(heldControl, false);
        }

        if (Input.GetKeyDown(KeyCode.Mouse2))
        {
            SetAimDirection(Straight);
        }
    }


    private void HandleScrolling(bool heldControl, bool scrolledUp)
    {
        if (heldControl)
        {
            ScrolledWheelUp?.Invoke(scrolledUp);
        }
        else
        {
            int change = scrolledUp ? 1 : -1;
            var newAimDirection = Mathf.Clamp((int)AimDirection + change, 0, 2);
            SetAimDirection((AimDirection)newAimDirection);
        }
    }

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
    private void Shoot(Vector2 cursorPos, bool firstPress)
    {
        if (!gun.CanShoot(firstPress)) return;

        var shotCount = gun.ShotCount;
        for (int i = 0; i < shotCount; i++)
        {
            var actualShootDirection = gun.GetShootDirection(cursorPos);
            var canHeadshot = gun.CanHeadShot(firstPress, AimDirection);
            gun.ApplyRecoil();

            networkInput.RequestShootRpc(cursorPos, actualShootDirection, canHeadshot, AimDirection);
        }
        networkInput.ShowGunshotVisualsRPC(mediator.PlayerId);
    }

    private void UpdateUniqueActions()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (!gun.CanReload()) return;
            networkInput.RequestReloadRpc();
        }
    }

    private void UpdateAbilities()
    {
        UpdateAbility(abilityManager.MovementAbility, false);
        UpdateAbility(abilityManager.UtilityAbility, false);
    }
    private void UpdateAbility(ActiveAbility ability, bool postMortem)
    {
        if (postMortem) CursorPosition = GetCursorPosition(teamMateCamera);
        if (!ability.ReadyToCast()) return;

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

        if (pingReady && Input.GetKeyDown(KeyCode.V))
        {
            networkInput.RequestPingRpc(CursorPosition, AimDirection, mediator.PlayerId);
            StartCoroutine(PingCoolDown());
        }
    }

    private bool pingReady = true;
    private IEnumerator PingCoolDown()
    {
        pingReady = false;
        yield return new WaitForSeconds(pingCoolDown);
        pingReady = true;
    }

    private void UpdateDebug()
    {
        if (Input.GetKeyDown(KeyCode.Q)) mediator.NetworkInput.DealDamage(100, DamageTag.Neutral, mediator, mediator);
        if (Input.GetKeyDown(KeyCode.H))
        {
            mediator.NetworkInput.DealDamage(30, DamageTag.Neutral, mediator, mediator);
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
