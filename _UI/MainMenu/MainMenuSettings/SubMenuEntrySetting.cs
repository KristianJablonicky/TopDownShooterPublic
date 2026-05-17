using UnityEngine;

public class SubMenuEntrySetting : MonoBehaviour
{
    [SerializeField] private SettingsKeys settingsKey;
    [SerializeField] private int defaultValue;

    [Header("References")]
    [SerializeField] private GameObject inputTypeContainer;
    //[SerializeField] private MenuSettingInputType settingInput;

    private void Start()
    {
        var storage = DataStorage.Instance;
        if (inputTypeContainer.transform.GetChild(0).TryGetComponent<MenuSettingInputType>(out var settingInput))
        {
            settingInput.SetInitialValue(
                storage.GetInt(settingsKey, defaultValue)
            );

            settingInput.ValueChanged += newValue =>
            {
                storage.SetSettingAndNotify(settingsKey, newValue);
            };
            return;
        }
        Debug.LogWarning($"setting input for {settingsKey} type entry is null!");
    }
}
