using System;
using System.Collections;
using System.Net.NetworkInformation;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using static AimDirection;

public class PlayerInputHandler : MonoBehaviour
{
    private RawImage rawImage;
    private Camera uiCamera;
    private Camera playerCamera;

    [SerializeField] private PlayerNetworkInput networkInput;
    [SerializeField] private RenderTexture renderTexture;
    [SerializeField] private CharacterMediator mediator;
    [SerializeField] private MovementController movementController;
    [SerializeField] private RotationController rotationController;
    [SerializeField] private Gun gun;

    [SerializeField] private float cursorAccuracyMultiplier = 0.1f;

    private AbilityManager abilityManager;

    private AimDirection aimDirection = Straight;

    public event Action<bool> AimedDownSights, ScrolledWheelUp;
    public event Action<AimDirection> AimDirectionChanged;
    private bool aimingDownSights = false;
    private bool ADSSetToToggle;

    public Vector2 CursorPosition { get; private set; }
    private Action updateAction;

    private void Awake()
    {
        enabled = false;
        updateAction = UpdateInputAlive;
        // TODO: remove SetActive(true); once a smarter approach towards death system is chosen
        mediator.Died += (_) => { gameObject.SetActive(true); updateAction = UpdateInputPostMortem; };
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

    private void Update()
    {
        updateAction();
    }
    public void UpdateInputAlive()
    {
        UpdateMovement();
        UpdateMouse();
        UpdateUniqueActions();
        UpdateAbilities();

        UpdateDebug();

        UpdateAlwaysAvailable();
    }

    public void UpdateInputPostMortem()
    {
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
        
        ChangeOnHoldState(KeyCode.LeftShift, movementController.Walk);

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        movementController.WalkInDirection(moveX, moveY);
    }

    private void UpdateMouse()
    {
        var newPosition = GetCursorPosition();
        gun.ApplyRecoilMouseMovement((newPosition - CursorPosition).magnitude * cursorAccuracyMultiplier);
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

    private void HandleScrolling(bool heldControl, bool scrolledUp)
    {
        if (heldControl)
        {
            ScrolledWheelUp?.Invoke(scrolledUp);
        }
        else
        {
            int change = scrolledUp ? 1 : -1;
            var newAimDirection = Mathf.Clamp((int)aimDirection + change, 0, 2);
            SetAimDirection((AimDirection)newAimDirection);
        }
    }

    private void SetAimDirection(AimDirection newAimDirection)
    {
        if (aimDirection == newAimDirection) return;

        aimDirection = newAimDirection;
        AimDirectionChanged?.Invoke(newAimDirection);
    }
    public Vector2 GetCursorPositionNormalized()
    {
        return new Vector2(
            (Input.mousePosition.x / Screen.width - 0.5f) * 2f,
            (Input.mousePosition.y / Screen.height - 0.5f) * 2f
        ).normalized;
    }
    private Vector2 GetCursorPosition()
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rawImage.rectTransform,
                Input.mousePosition,
                uiCamera,
                out Vector2 localPoint))
        {
            return Vector2.zero; // mouse not over RawImage
        }
        // Clamp local point to RawImage rect
        var rect = rawImage.rectTransform.rect;
        localPoint.x = Mathf.Clamp(localPoint.x, rect.xMin, rect.xMax);
        localPoint.y = Mathf.Clamp(localPoint.y, rect.yMin, rect.yMax);

        // Normalize local point to [0,1] UV
        var u = (localPoint.x - rect.x) / rect.width;
        var v = (localPoint.y - rect.y) / rect.height;

        // Convert to RenderTexture pixel coords
        var px = u * renderTexture.width;
        var py = v * renderTexture.height;

        // Now map into player camera space
        return playerCamera.ScreenToWorldPoint(new Vector3(px, py, playerCamera.nearClipPlane));
    }
    private void Shoot(Vector2 cursorPos, bool firstPress)
    {
        if (!gun.CanShoot(firstPress)) return;

        var shotCount = gun.ShotCount;
        for (int i = 0; i < shotCount; i++)
        {
            var actualShootDirection = gun.GetShootDirection(cursorPos);
            gun.ApplyRecoil();

            var canHeadshot = gun.CanHeadShot(firstPress, aimDirection);

            networkInput.RequestShootRpc(cursorPos, actualShootDirection, canHeadshot, aimDirection);
        }
        networkInput.ShowGunshotVisualsRPC(mediator.PlayerId);
    }

    private void UpdateUniqueActions()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            gun.Reload();
        }

        if (pingReady && Input.GetKeyDown(KeyCode.V))
        {
            networkInput.RequestPingRpc(CursorPosition, mediator.PlayerId);
            StartCoroutine(PingCoolDown());
        }
    }

    private bool pingReady = true;
    private IEnumerator PingCoolDown()
    {
        pingReady = false;
        yield return new WaitForSeconds(3f);
        pingReady = true;
    }
    private void UpdateAbilities()
    {
        UpdateAbility(abilityManager.MovementAbility, false);
        UpdateAbility(abilityManager.UtilityAbility, false);
    }
    private void UpdateAbility(ActiveAbility ability, bool postMortem)
    {
        if (!ability.ReadyToCast()) return;
        if (postMortem) CursorPosition = GetCursorPositionNormalized();
        ChangeOnHoldState((KeyCode)ability.KeyCode, (pressed) => ability.OnKeyInteraction(pressed, CursorPosition));
    }


    private void UpdateAlwaysAvailable()
    {
        ChangeOnHoldState(KeyCode.Tab, ScoreBoard.Instance.ChangeState);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            NetworkManager.Singleton.Shutdown();
        }
    }

    private void UpdateDebug()
    {
        if (Input.GetKeyDown(KeyCode.Q)) mediator.NetworkInput.DealDamage(10, mediator);
        if (Input.GetKeyDown(KeyCode.F)) mediator.PlayerVision.SetVisionRangeProportional(0.5f);
    }
}

public enum AimDirection
{
    Down,
    Straight,
    Up
}
