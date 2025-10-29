using TMPro;
using UnityEngine;

public class ObservableVariableBinder : MonoBehaviour
{
    [SerializeField] private TMP_Text textField;
    [SerializeField] private DisplayType displayType = DisplayType.Default;
    [SerializeField] private string prefix;
    [SerializeField, TextArea] private string defaultValue;

    private ObservableValue<float> observedVariable;

    private IDisplayStrategy strategy;
    private enum DisplayType
    {
        Default,
        Time,
    }

    private bool isInitialized = false;

    private void Awake()
    {
        if (!isInitialized) Init();
    }

    private void Init()
    {
        strategy = DisplayStrategyFactory();
        textField.text = defaultValue;
        gameObject.SetActive(false);
        isInitialized = true;
    }

    public void Bind(ObservableValue<float> variable, bool updateValueImmediately)
    {
        if (!isInitialized) Init();
        if (observedVariable is null)
        {
            gameObject.SetActive(true);
            observedVariable = variable;

        }
        else if (observedVariable != variable)
        {
            observedVariable.OnValueSet -= UpdateText;
            observedVariable = variable;
        }
        else
        {
            // Already bound to this variable
            return;
        }
        observedVariable.OnValueSet += UpdateText;
        if (updateValueImmediately) UpdateText(observedVariable);
    }

    private void UpdateText(float newValue)
    {
        textField.text = $"{prefix}{strategy.FormatValue(newValue)}";
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
