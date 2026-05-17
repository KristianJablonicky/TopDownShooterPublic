using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextDumpPopUpManager : SingletonMonoBehaviour<TextDumpPopUpManager>
{
    [SerializeField] private TMP_Text header, textDump;
    [SerializeField] private RectTransform headerBackgroundTransform, textDumpBackgroundTransform;
    [SerializeField] private PopUpWindowBase popUpWindowBase;
    [SerializeField] private Image image;
    [SerializeField] private GameObject imageContainer;
    public void ShowText(string headerText, string text, bool centered, Sprite imageGraphic = null)
    {
        popUpWindowBase.GetActivated();

        header.text = headerText;
        textDump.text = text;
        
        if (centered)
        {
            textDump.alignment = TextAlignmentOptions.Center;
        }
        else
        {
            textDump.alignment = TextAlignmentOptions.TopLeft;
        }

        if (imageGraphic != null)
        {
            image.sprite = imageGraphic;
            imageContainer.SetActive(true);
        }
        else
        {
            imageContainer.SetActive(false);
        }

        StartCoroutine(ResizeAfterOneFrame());
    }

    private IEnumerator ResizeAfterOneFrame()
    {
        yield return null;
        ResizeBackground(header, headerBackgroundTransform, RectTransform.Axis.Horizontal);
        ResizeBackground(textDump, textDumpBackgroundTransform, RectTransform.Axis.Vertical);
    }

    private void ResizeBackground(TMP_Text text, RectTransform rectTransform, RectTransform.Axis targetAxis)
    {
        text.ForceMeshUpdate();
        var size = text.GetRenderedValues();

        var targetSize = targetAxis == RectTransform.Axis.Horizontal ? size.x : size.y;
        rectTransform.SetSizeWithCurrentAnchors(targetAxis, targetSize);
    }
}
