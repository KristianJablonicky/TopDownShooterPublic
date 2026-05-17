using System.Collections.Generic;

public class ModifiableFloat : IResettable
{
    public ObservableValue<float> Multiplier { get; private set; }
    public ObservableValue<float> BaseValue { get; private set; }
    public ObservableValue<float> CurrentValue { get; private set; }


    private readonly Dictionary<object, float> multipliers;

    public static implicit operator float(ModifiableFloat value)
    {
        if (value.CurrentValue != null)
        {
            return value.CurrentValue;
        }
        return value.Multiplier;
    }

    /// <summary>
    /// Set up observable values and default multiplier.
    /// </summary>
    /// <param name="baseValue">Leave null to avoid creating and invoking base and current value changes.</param>
    public ModifiableFloat(float? baseValue = null)
    {
        if (baseValue.HasValue)
        {
            BaseValue = new(baseValue.Value);
            CurrentValue = new(baseValue.Value);
        }
        Multiplier = new(1f);

        multipliers = new();
    }

    /// <summary>
    /// Add or change multiplier from a specific source.
    /// </summary>
    /// <param name="source">the external caller</param>
    /// <param name="multiplier">movement multiplier in %. Negative numbers mean X% reduction</param>
    public void AddOrChangeMultiplier(object source, int multiplier) => AddOrChangeMultiplier(source, multiplier / 100f);

    /// <summary>
    /// Add or change a multiplier from a specific source.
    /// </summary>
    /// <param name="source">the external caller</param>
    /// <param name="multiplier">multiplier as a float. Negative numbers mean reduction</param>
    public void AddOrChangeMultiplier(object source, float multiplier)
    {
        if (!multipliers.ContainsKey(source))
        {
            multipliers.Add(source, multiplier);
        }
        multipliers[source] = multiplier;

        SetMultiplier();
    }

    public void RemoveMultiplier(object source)
    {
        if (!multipliers.ContainsKey(source)) return;
        multipliers.Remove(source);
        SetMultiplier();
    }

    private void SetMultiplier()
    {
        var totalMultiplier = 1f;
        foreach (var multiplier in multipliers.Values)
        {
            totalMultiplier *= 1f + multiplier;
        }

        Multiplier.Set(totalMultiplier);
        
        CurrentValue?.Set(BaseValue * totalMultiplier);

        /*
        moveVelocity *= (totalMultiplier / movementMultiplier);
        movementMultiplier = totalMultiplier;
        */
    }

    public void Reset()
    {
        multipliers.Clear();
        SetMultiplier();
    }
}
