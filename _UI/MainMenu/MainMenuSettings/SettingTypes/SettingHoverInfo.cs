using UnityEngine;
using UnityEngine.EventSystems;

public class SettingHoverInfo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField, TextArea] private string settingInfo;

    public void OnPointerEnter(PointerEventData eventData)
    {
        HoverInfoPopUp.ShowInfo(settingInfo);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HoverInfoPopUp.Hide();
    }

    private void Awake()
    {
        if (settingInfo == string.Empty) Destroy(gameObject);
    }
}
