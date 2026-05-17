using System;
using UnityEngine;

public abstract class MenuSettingInputType : MonoBehaviour
{
    public event Action<int> ValueChanged;
    protected void InvokeValueChanged(int newValue)
    {
        ValueChanged?.Invoke(newValue);
    }
    public abstract void SetInitialValue(int value);
}
