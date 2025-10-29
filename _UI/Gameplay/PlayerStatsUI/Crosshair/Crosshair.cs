using UnityEngine;
using UnityEngine.UI;

public class Crosshair : MonoBehaviour
{
    [Header("Accuracy")]
    [SerializeField] private RectTransform topLine;
    [SerializeField] private RectTransform bottomLine, leftLine, rightLine;
    [Header("Headshot")]
    [SerializeField] private CanvasGroup headshotDot;
    [Header("Channeling")]
    [SerializeField] private Image channelingImage;
    [SerializeField] private CanvasGroup[] crosshairCanvasGroups;
    [SerializeField] private float crosshairAlphaWhileChanneling = 0.25f;

    [Header("References")]
    [SerializeField] private RectTransform cursorRoot;
    [SerializeField] private Image aimDirectionArrow;
    [SerializeField] private Sprite AimDirectionUp, AimDirectionDown;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject ActiveGO, ChannelingGO;
    [SerializeField] private float minLineOffset = 3f;


    private Gun playerGun;
    private CharacterMediator mediator;
    private void Awake()
    {
        Cursor.visible = false;
        PlayerNetworkInput.OwnerSpawned += OnOwnerSpawn;
        enabled = false;
    }

    private void Start()
    {
        EscapeMenuManager.Instance.Toggled += OnEscapeMenuToggled;
    }

    private void OnDestroy()
    {
        Cursor.visible = true;
    }

    private void OnOwnerSpawn(CharacterMediator mediator)
    {
        this.mediator = mediator;
        playerGun = mediator.Gun;
        enabled = true;
        mediator.InputHandler.AimDirectionChanged += OnAimDirectionChange;

        mediator.Died += (_) => UpdateOnDeath();
    }

    private void OnAimDirectionChange(AimDirection aimDirection)
    {
        if (aimDirection == AimDirection.Straight)
        {
            aimDirectionArrow.gameObject.SetActive(false);
        }
        else
        {
            aimDirectionArrow.gameObject.SetActive(true);
            aimDirectionArrow.sprite = aimDirection == AimDirection.Up ? AimDirectionUp : AimDirectionDown;
        }
    }

    void Update()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            cursorRoot.parent as RectTransform,
            Input.mousePosition,
            playerCamera,
            out Vector2 mousePos
        );
        cursorRoot.localPosition = mousePos;
        if (mediator.IsAlive) UpdateAlive();
    }

    private void UpdateAlive()
    {
        if (playerGun.ChannelingManager.Channeling)
        {
            var progress = playerGun.ChannelingManager.Progress;
            ChannelingGO.SetActive(true);

            //ActiveGO.SetActive(false);
            SetActiveCrosshairAlpha(crosshairAlphaWhileChanneling);


            channelingImage.fillAmount = 1f - progress;
        }
        else if (ChannelingGO.activeSelf)
        {
            ChannelingGO.SetActive(false);
            //ActiveGO.SetActive(true);
            SetActiveCrosshairAlpha(1f);
        }

        var worldCenter = playerCamera.ScreenToWorldPoint(
            new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f)
        );

        var angleRad = playerGun.GetAngle() * Mathf.Deg2Rad;


        var distance = Vector2.Distance(mediator.GetPosition(), mediator.InputHandler.CursorPosition);

        var spreadRadius = Mathf.Tan(angleRad / 2f) * distance;

        var worldOffset = worldCenter + playerCamera.transform.up * spreadRadius;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            cursorRoot.parent as RectTransform,
            playerCamera.WorldToScreenPoint(worldCenter),
            playerCamera,
            out Vector2 uiCenter
        );
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            cursorRoot.parent as RectTransform,
            playerCamera.WorldToScreenPoint(worldOffset),
            playerCamera,
            out Vector2 uiOffset
        );

        var offset = (uiOffset - uiCenter).magnitude;
        offset = Mathf.Max(minLineOffset, offset);

        topLine.anchoredPosition = new(0, offset);
        bottomLine.anchoredPosition = new(0, -offset);
        leftLine.anchoredPosition = new(-offset, 0);
        rightLine.anchoredPosition = new(offset, 0);


        var headShotValue = playerGun.CanHeadShot();
        headshotDot.alpha = headShotValue > 0f ? Mathf.Max(playerGun.CanHeadShot(), 0.5f) : 0f;
    }

    private void SetActiveCrosshairAlpha(float targetAlpha)
    {
        foreach (var line in crosshairCanvasGroups)
        {
            line.alpha = targetAlpha;
        }
    }

    private void UpdateOnDeath()
    {
        ChannelingGO.SetActive(false);
        ActiveGO.SetActive(true);

        topLine.anchoredPosition = new(0, minLineOffset);
        bottomLine.anchoredPosition = new(0, -minLineOffset);
        leftLine.anchoredPosition = new(-minLineOffset, 0);
        rightLine.anchoredPosition = new(minLineOffset, 0);
        headshotDot.alpha = 1f;
    }

    private void OnEscapeMenuToggled(bool visible)
    {
        gameObject.SetActive(!visible);
        Cursor.visible = visible;
    }
}