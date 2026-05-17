using UnityEngine;

public class DisplaySettingsHandler : MonoBehaviour
{
    [SerializeField] private bool lockFpsAt60 = true;
    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        var ds = DataStorage.Instance;
        OnFullScreenChange(
            ds.SubscribeAndGetCurrentValue(
                SettingsKeys.FullScreen, OnFullScreenChange, 1)
        );
        if (lockFpsAt60)
        {
            SetFrameRate(60);
        }
        else
        {
            SetFrameRate(
                ds.SubscribeAndGetCurrentValue(
                    SettingsKeys.FrameRate, SetFrameRate, Constants.Defaults.frameRate)
            );
        }
    }
    private void OnDestroy()
    {
        var storage = DataStorage.Instance;
        storage.Unsubscribe(SettingsKeys.FullScreen, OnFullScreenChange);
        storage.Unsubscribe(SettingsKeys.FrameRate, SetFrameRate);
    }
    private void OnFullScreenChange(int fullScreen)
    {
        if (fullScreen == 1)
        {
            Screen.SetResolution(
                Screen.currentResolution.width,
                Screen.currentResolution.height,
                FullScreenMode.FullScreenWindow // or ExclusiveFullScreen
            );
        }
        else
        {
            Screen.fullScreen = false;
        }
    }

    private void SetFrameRate(int newFrameRate)
    {
        Application.targetFrameRate = newFrameRate;
    }
}
