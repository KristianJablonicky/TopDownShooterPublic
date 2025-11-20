using TMPro;
using UnityEngine;

public class ObservableVariableBinder : MonoBehaviour
{
    [SerializeField] private TMP_Text textField;
    [SerializeField] private DisplayType displayType = DisplayType.Default;
    [SerializeField] private string prefix;
    [SerializeField, Tooltip("(s) to optionally remove the s based on the new value")]
        private string suffix;
    [SerializeField, TextArea] private string defaultValue;

    private ObservableValue<int> observedVariable;

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

    public void Bind(ObservableValue<int> variable, bool updateValueImmediately)
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

    private void UpdateText(int newValue)
    {
        if (suffix.Length > 0)
        {
            var editedSuffix = suffix.Replace("(s)", newValue == 1 ? "" : "s");
            textField.text = $"{prefix}{strategy.FormatValue(newValue)} {editedSuffix}";
        }
        else
        {
            textField.text = $"{prefix}{strategy.FormatValue(newValue)}";

        }
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
        public string FormatValue(int value);
    }

    private class DefaultDisplayStrategy : IDisplayStrategy
    {
        public string FormatValue(int value) => value.ToString();
    }

    private class TimeDisplayStrategy : IDisplayStrategy
    {
        public string FormatValue(int value)
        {
            var minutes = value / 60;
            var seconds = value % 60;
            return $"{minutes:0}:{seconds:00}";
        }
    }
    #endregion
}
