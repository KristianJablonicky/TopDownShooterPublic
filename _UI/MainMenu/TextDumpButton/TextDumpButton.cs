using UnityEngine;

public class TextDumpButton : MonoBehaviour
{
    [field: SerializeField, TextArea] public string Header { get; private set; }
    [field: SerializeField] public TextAsset TextFile { get; private set; }
    [field: SerializeField, TextArea] public string Text { get; private set; }
    [field: SerializeField] public Sprite DumpImage { get; private set; }
    [SerializeField] private bool centerText = true;

    public void ShowText()
    {
        if (TextFile == null)
        {
            TextDumpPopUpManager.Instance.ShowText(Header, Text, centerText, DumpImage);
        }
        else
        {
            TextDumpPopUpManager.Instance.ShowText(Header, TextFile.text, centerText, DumpImage);
        }
    }
}
