using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class SliderSettingType : MenuSettingInputType
{
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text currentValue;
    [SerializeField] private float maxValueTranslatesTo = 100f;

    public override void SetInitialValue(int value)
    {
        SetValue(value);
        slider.value = (int)(value / maxValueTranslatesTo * slider.maxValue);
        slider.onValueChanged.AddListener(value =>
        {
            SetValue((int)(value * maxValueTranslatesTo / slider.maxValue));
        });
    }
    private void SetValue(int value)
    {
        currentValue.text = value.ToString();
        InvokeValueChanged(value);
    }
}
