using UnityEngine;

public class DropDownSettingInput : MenuSettingInputType
{
    [SerializeField] private CrosshairColorDropDown crosshairColorDropDown;

    private void Start()
    {
        crosshairColorDropDown.GetDropdown().onValueChanged.AddListener(InvokeValueChanged);
    }

    public override void SetInitialValue(int value)
    {
        crosshairColorDropDown.SetInitialValue(value);
    }
}
