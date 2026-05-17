using System.Collections;
using UnityEngine;

public class RiseOnHover : Hoverable
{
    [SerializeField] private float travelTime = 0.25f;
    [SerializeField] private float targetYHover, targetXHover;
    [SerializeField] private RectTransform movingTransform;
    [SerializeField] private float targetRotationZ = 0f;
    [SerializeField, Range(1f, 2f)] private float targetScaleMultiplier = 1f;

    [SerializeField] private CardSetUp card;

    private const float timeTillPopUp = 0.5f;
    private const KeyCode infoHotkey = KeyCode.Q;
    private Coroutine coroutine;

    private bool moving = false;
    private float startY, startX;
    private float t, direction;
    private float baseRotationZ;
    private Vector3 baseScale, targetScale;

    private void Awake()
    {
        startY = movingTransform.anchoredPosition.y;
        startX = movingTransform.anchoredPosition.x;

        baseRotationZ = movingTransform.localEulerAngles.z;

        baseScale = movingTransform.localScale;
        targetScale = baseScale * targetScaleMultiplier;

        t = 0f;
        enabled = false;
    }
    public override void HoverStateChanged(bool hoveredOn)
    {
        direction = hoveredOn ? 1f : -1f;
        enabled = true;
        moving = true;

        if (hoveredOn)
        {
            coroutine = StartCoroutine(WaitBeforePopUp());
        }
        else
        {
            if (coroutine != null) StopCoroutine(coroutine);
            HoverInfoPopUp.Hide();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(infoHotkey))
        {
            TextDumpPopUpManager.Instance.ShowText(card.Name, card.LongDescription, true,
                card.ScriptableObjectBase.Icon);
        }
    }
    private void FixedUpdate()
    {
        if (!moving) return;
        t = Mathf.Clamp01(t + direction * Time.deltaTime / travelTime);

        var pos = movingTransform.anchoredPosition;
        pos.y = Mathf.Lerp(startY, targetYHover, t);
        pos.x = Mathf.Lerp(startX, targetXHover, t);
        movingTransform.anchoredPosition = pos;

        movingTransform.localScale = Vector3.Lerp(baseScale, targetScale, t);

        var rot = movingTransform.localEulerAngles;
        rot.z = Mathf.LerpAngle(baseRotationZ, targetRotationZ, t);
        movingTransform.localEulerAngles = rot;

        if (t == 0f || t == 1f)
        {
            if (t == 0f) enabled = false;
            moving = false;
        }

    }

    private IEnumerator WaitBeforePopUp()
    {
        yield return new WaitForSeconds(timeTillPopUp);
        HoverInfoPopUp.ShowInfo($"Press {infoHotkey} to view more info.");
    }
}
