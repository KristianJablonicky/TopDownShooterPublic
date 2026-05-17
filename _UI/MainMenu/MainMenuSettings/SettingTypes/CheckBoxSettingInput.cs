using UnityEngine;
using UnityEngine.UI;

public class CheckBoxSettingInput : MenuSettingInputType
{
    [SerializeField] private Toggle toggle;
    public override void SetInitialValue(int value)
    {
        toggle.isOn = value == 1;
        toggle.onValueChanged.AddListener(newCheck => {
            InvokeValueChanged(newCheck ? 1 : 0);
        });
    }

}
