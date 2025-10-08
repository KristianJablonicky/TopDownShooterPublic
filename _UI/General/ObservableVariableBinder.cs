using TMPro;
using UnityEngine;

public class ObservableVariableBinder : MonoBehaviour
{
    [SerializeField] private TMP_Text textField;
    [SerializeField] private DisplayType displayType = DisplayType.Default;
    [SerializeField, TextArea] private string defaultValue;

    private IDisplayStrategy strategy;
    private enum DisplayType
    {
        Default,
        Time,
    }

    private void Awake()
    {
        strategy = DisplayStrategyFactory();
        textField.text = defaultValue;
    }

    public void Bind(ObservableValue<float> variable)
    {
        variable.OnValueSet += UpdateText;
    }

    private void UpdateText(float newValue)
    {
        textField.text = strategy.FormatValue(newValue);
    }

    #region Display Strategies
    private IDisplayStrategy DisplayStrategyFactory()
    {
        return displayType switch
        {
            DisplayType.Default => new DefaultDisplayStrategy(),
            DisplayType.Time => new TimeDisplayStrategy(),
            _ => new DefaultDisplayStrategy(),
        };
    }

    private interface IDisplayStrategy
    {
        public string FormatValue(float value);
    }

    private class DefaultDisplayStrategy : IDisplayStrategy
    {
        public string FormatValue(float value) => value.ToString();
    }

    private class TimeDisplayStrategy : IDisplayStrategy
    {
        public string FormatValue(float value)
        {
            int minutes = Mathf.FloorToInt(value / 60);
            int seconds = Mathf.FloorToInt(value % 60);
            return $"{minutes:00}:{seconds:00}";
        }
    }
    #endregion
}
