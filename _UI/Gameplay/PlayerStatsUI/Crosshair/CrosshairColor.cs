using UnityEngine;
using UnityEngine.UI;

public class CrosshairColor : MonoBehaviour
{
    [SerializeField] private Image[] crosshairParts;

    private void Start()
    {
        var colorOption = DataStorage.Instance.GetInt(DataKeyInt.SettingsCrosshairColor);
        SetColor(colorOption);
    }

    public void SetColor(int colorIndex)
    {
        var targetColor = GetColor((CrosshairColorOptions)colorIndex);
        foreach (var part in crosshairParts)
        {
            part.color = targetColor;
        }
    }

    public static Color GetColor(CrosshairColorOptions color)
    {
        return color switch
        {
            CrosshairColorOptions.White => Color.white,
            CrosshairColorOptions.Cyan => Color.cyan,
            CrosshairColorOptions.Green => Color.green,
            CrosshairColorOptions.Orange => CommonColors.Instance.Orange,
            CrosshairColorOptions.Red => Color.softRed,
            CrosshairColorOptions.Yellow => Color.yellow,
            CrosshairColorOptions.Pink => Color.hotPink,
            CrosshairColorOptions.Purple => CommonColors.Instance.Cyan,
            _ => Color.white,
        };
    }

}

public enum CrosshairColorOptions
{
    White,
    Cyan,
    Green,
    Orange,
    Red,
    Yellow,
    Pink,
    Purple,

    LAST_OPTION = Purple
}
