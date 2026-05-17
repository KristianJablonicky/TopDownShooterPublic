using TMPro;
using UnityEngine;

public class HoverInfoPopUp : CanvasFaderBase
{
    [SerializeField] private TMP_Text info;
    [SerializeField] private RectTransform myTransform, backgroundTransform;
    [SerializeField] private Camera uiCamera;

    [SerializeField] private Vector2 offsetMultiplier;
    private static HoverInfoPopUp instance;
    private Vector2 offset;
    private RectTransform parentTransform;

    private float minX, minY, maxX, maxY;

    private void Awake()
    {
        instance = this;
        
        parentTransform = transform.parent.GetComponent<RectTransform>();

        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }

    public static void ShowInfo(string text, string header = null)
    {
        instance.ShowInfoInternal(text, header);
    }
    private void ShowInfoInternal(string text, string header = null)
    {
        gameObject.SetActive(true);
        if (header != null)
        {
            info.text = $"<b>{header}</b>\n{text}";
        }
        else
        {
            info.text = text;
        }
        TweenState(true);


        info.ForceMeshUpdate();
        var size = info.GetRenderedValues();

        backgroundTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
        backgroundTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);

        var parentSize = parentTransform.rect.size;
        var rectSize = size + new Vector2(12f, 12f);
        var pivot = myTransform.pivot;

        minX = -parentSize.x * parentTransform.pivot.x + rectSize.x * pivot.x;
        maxX = parentSize.x * (1f - parentTransform.pivot.x) - rectSize.x * (1f - pivot.x);

        minY = -parentSize.y * parentTransform.pivot.y + rectSize.y * pivot.y;
        maxY = parentSize.y * (1f - parentTransform.pivot.y) - rectSize.y * (1f - pivot.y);

        offset = offsetMultiplier * size;
    }
    public static void Hide()
    {
        instance.TweenState(false, () => instance.gameObject.SetActive(false));
    }

    private void Update()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentTransform,
            Input.mousePosition,
            uiCamera,
            out var localPos
        );

        localPos += offset;

        localPos.x = Mathf.Clamp(localPos.x, minX, maxX);
        localPos.y = Mathf.Clamp(localPos.y, minY, maxY);
        myTransform.anchoredPosition = localPos;
    }
}
