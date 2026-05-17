using TMPro;
using UnityEngine;

public class CrosshairColorDropDown : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropDown;
    [SerializeField] private CrosshairColor crosshairColor;

    private void Start()
    {
        var lastOption = (int)CrosshairColorOptions.LAST_OPTION;
        for (int i = 0; i <= lastOption; i++)
        {
            var option = (CrosshairColorOptions)i;
            var color = CrosshairColor.GetColor(option);
            dropDown.options.Add(new(
                $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{option}</color>"
                )
            );
        }

        dropDown.onValueChanged.AddListener((value) =>
        {
            crosshairColor.SetColor(value);
        });
    }

    public TMP_Dropdown GetDropdown() => dropDown;

    public void SetInitialValue(int value)
    {
        dropDown.value = value;
        dropDown.RefreshShownValue();
    }
}
