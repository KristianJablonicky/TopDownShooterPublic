using System;
public class ObservableValue<T> where T : struct, IComparable, IConvertible
{
    private T _value;

    /// <summary>
    /// First value is the new value, second value is the adjustment.
    /// </summary>
    public event Action<T, T> OnAdjusted;

    public event Action<T> OnValueSet;

    public ObservableValue(T initialValue)
    {
        _value = initialValue;
    }

    public T Get() => _value;

    public void Set(T newValue)
    {
        _value = newValue;
        OnValueSet?.Invoke(newValue);
    }

    public void Adjust(T adjustAmount)
    {
        var type = typeof(T);
        if (type == typeof(int))
            _value = (T)(object)((int)(object)_value + (int)(object)adjustAmount);
        else if (type == typeof(float))
            _value = (T)(object)((float)(object)_value + (float)(object)adjustAmount);
        else if (type == typeof(double))
            _value = (T)(object)((double)(object)_value + (double)(object)adjustAmount);
        else
            throw new NotSupportedException($"Type {typeof(T)} not supported");

        OnAdjusted?.Invoke(_value, adjustAmount);
        OnValueSet?.Invoke(_value);
    }

    public void Adjust(float adjustAmount, float? floor = null, float? ceiling = null)
    {
        if (_value is float val)
        {
            var newValue = val + adjustAmount;
            newValue = floor.HasValue ? Math.Max(newValue, floor.Value) : newValue;
            newValue = ceiling.HasValue ? Math.Min(newValue, ceiling.Value) : newValue;
            Set((T)(object)newValue);
        }
        else
        {
            UnityEngine.Debug.LogError("Adjust with float only supported for ObservableValue<float>");
        }
    }
    public void Adjust(int adjustAmount, int? floor = null, int? ceiling = null)
    {
        if (_value is int val)
        {
            var newValue = val + adjustAmount;
            newValue = floor.HasValue ? Math.Max(newValue, floor.Value) : newValue;
            newValue = ceiling.HasValue ? Math.Min(newValue, ceiling.Value) : newValue;
            Set((T)(object)newValue);
        }
        else
        {
            UnityEngine.Debug.LogError("Adjust with int only supported for ObservableValue<int>");
        }
    }

    public static implicit operator T(ObservableValue<T> observable)
    {
        return observable.Get();
    }

    private void Increment(double delta)
    {
        double v = Convert.ToDouble(_value) + delta;
        Set((T)Convert.ChangeType(v, typeof(T)));
    }

    public static ObservableValue<T> operator ++(ObservableValue<T> observable)
    {
        observable.Increment(+1);
        return observable;
    }

    public static ObservableValue<T> operator --(ObservableValue<T> observable)
    {
        observable.Increment(-1);
        return observable;
    }

    public override string ToString() => _value.ToString();
}
