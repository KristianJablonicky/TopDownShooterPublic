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

    [Header("References")]
    [SerializeField] private RectTransform cursorRoot;
    [SerializeField] private Image aimDirectionArrow;
    [SerializeField] private Sprite AimDirectionUp, AimDirectionDown;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject ActiveGO, ChannelingGO;
    [SerializeField] private float minLineOffset = 3f;

    private Gun playerGun;
    private void Awake()
    {
        Cursor.visible = false;
        PlayerNetworkInput.OwnerSpawned += OnOwnerSpawn;
        enabled = false;
    }

    private void OnDestroy()
    {
        Cursor.visible = true;
    }

    private void OnOwnerSpawn(CharacterMediator mediator)
    {
        playerGun = mediator.Gun;
        enabled = true;
        mediator.InputHandler.AimDirectionChanged += OnAimDirectionChange;
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
        
        
        if (playerGun.ChannelingManager.Channeling)
        {
            var progress = playerGun.ChannelingManager.Progress;
            ChannelingGO.SetActive(true);
            ActiveGO.SetActive(false);
            channelingImage.fillAmount = 1f - progress;
            return;
        }
        else if (ChannelingGO.activeSelf)
        {
            ChannelingGO.SetActive(false);
            ActiveGO.SetActive(true);
        }

        var worldCenter = playerCamera.ScreenToWorldPoint(
            new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f)
        );

        var worldOffset = worldCenter + playerCamera.transform.up * (playerGun.GetDiameter() / 2f);

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


        topLine.anchoredPosition = new Vector2(0, offset);
        bottomLine.anchoredPosition = new Vector2(0, -offset);
        leftLine.anchoredPosition = new Vector2(-offset, 0);
        rightLine.anchoredPosition = new Vector2(offset, 0);

        var headShotValue = playerGun.CanHeadShot();
        headshotDot.alpha = headShotValue > 0f ? Mathf.Max(playerGun.CanHeadShot(), 0.5f) : 0f;
    }

}