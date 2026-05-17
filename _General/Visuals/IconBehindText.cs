using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IconBehindText : MonoBehaviour
{
    [SerializeField] private Graphic graphicBehindText;
    [SerializeField] private TMP_Text textOnTopOfIcon;

    public void SetText(string text)
    {
        graphicBehindText.gameObject.SetActive(true);
        if (text == null)
        {
            Hide();
            return;
        }
        textOnTopOfIcon.text = text;
    }

    public void Hide()
    {
        graphicBehindText.gameObject.SetActive(false);
    }
}
