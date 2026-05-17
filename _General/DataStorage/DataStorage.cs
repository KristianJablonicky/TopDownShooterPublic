using System;
using System.Collections.Generic;
using UnityEngine;

public class DataStorage
{
    public static DataStorage Instance = new();
    private readonly Dictionary<string, string> stringData = new();
    private readonly Dictionary<string, int> intData = new();

    private DataStorage()
    {
        stringData = new();
        intData = new();
    }

    // Temporary strings to reason abrupt disconnects
    // TODO: ideally remove eventually
    public string lastScoreBoardState, disconnectReason;

    public void SetString(DataKeyString key, string value)
    {
        string sKey = key.ToString();
        PlayerPrefs.SetString(sKey, value);
        if (!stringData.ContainsKey(sKey))
        {
            stringData.Add(sKey, value);
        }
        else
        {
            stringData[sKey] = value;
        }
    }
    public string GetString(DataKeyString key)
    {
        var sKey = key.ToString();
        if (stringData.ContainsKey(sKey))
        {
            return stringData[sKey];
        }

        stringData.Add(sKey, PlayerPrefs.GetString(sKey, string.Empty));
        return stringData[sKey];
    }

    public void SetSettingAndNotify(SettingsKeys key, int value)
    {
        SetInt((DataKeyInt)key, value);
        GetDictionary()[key]?.Invoke(value);
    }
    public void SetInt(DataKeyInt key, int value)
    {
        var sKey = key.ToString();
        SetInt(sKey, value);
    }
    private void SetInt(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
        if (!intData.ContainsKey(key))
        {
            intData.Add(key, value);
        }
        else
        {
            intData[key] = value;
        }
    }

    /// <summary>
    /// Set an int value specific to a hero. If hero is null, use the currently picked hero.
    /// </summary>
    public void SetIntHeroSpecific(DataKeyInt key, HeroDatabase? hero, GameMode? gameMode, int value)
    {
        if (!hero.HasValue)
        {
            hero = (HeroDatabase)GetInt(DataKeyInt.PickedHero);
        }
        if (!gameMode.HasValue)
        {
            gameMode = GetGameMode();
        }
        SetInt(GetHeroSpecificKey(key, hero.Value, gameMode.Value), value);
    }

    public int GetInt(DataKeyInt key, int defaultValue = 0)
    {
        var sKey = key.ToString();
        return GetInt(sKey, defaultValue);
    }
    public int GetInt(SettingsKeys key, int defaultValue = 0)
    {
        return GetInt((DataKeyInt)key, defaultValue);
    }
    private int GetInt(string key, int defaultValue = 0)
    {
        if (intData.ContainsKey(key))
        {
            return intData[key];
        }

        intData.Add(key, PlayerPrefs.GetInt(key, defaultValue));
        return intData[key];
    }

    /// <summary>
    /// Get an int value specific to a hero. If hero is null, use the currently picked hero.
    /// </summary>
    public int GetIntHeroSpecific(DataKeyInt key, HeroDatabase? hero, GameMode? gameMode)
    {
        if (!hero.HasValue)
        {
            hero = (HeroDatabase)GetInt(DataKeyInt.PickedHero);
        }
        return GetInt(GetHeroSpecificKey(key, hero.Value, gameMode));
    }

    public void Increment(DataKeyInt key, int increaseAmount)
    {
        SetInt(key, GetInt(key) + increaseAmount);
    }

    public GameMode GetGameMode() => (GameMode)GetInt(DataKeyInt.GameMode);

    private string GetHeroSpecificKey(DataKeyInt key, HeroDatabase hero, GameMode? gameMode)
    {
        if (gameMode.HasValue) return $"{key}_{(int)hero}_{(int)gameMode.Value}";
        return $"{key}_{(int)hero}_{(int)Instance.GetGameMode()}";
    }

    public static bool IsSinglePlayer => Instance.GetGameMode() > GameMode.MultiPlayer;

    // TODO: see where this is used, and move it to one VolumeHandler superclass that also has a
    // [SerializeField] specificAudioType for in-game sounds, voice lines and announcer...
    public static float VolumeToFloat(int volume) => volume / 100f * Constants.maxVolume;

    #region event bus
    private Action<int>[] valueChangedEvents;
    private Dictionary<SettingsKeys, Action<int>> eventDictionary;
    public int SubscribeAndGetCurrentValue(SettingsKeys setting, Action<int> eventHandler, int defaultSetting)
    {
        GetDictionary()[setting] += eventHandler;
        return GetInt(setting, defaultSetting);
    }

    public void Unsubscribe(SettingsKeys setting, Action<int> eventHandler)
    {
        if (eventDictionary is null) return;
        GetDictionary()[setting] -= eventHandler;
    }

    private Dictionary<SettingsKeys, Action<int>> GetDictionary()
    {
        if (eventDictionary is null)
        {
            var settingsCount = KeyToIndexMapping.Length;
            eventDictionary = new(settingsCount);
            valueChangedEvents = new Action<int>[settingsCount];
            for (int i = 0; i < settingsCount; i++)
            {
                eventDictionary.Add(KeyToIndexMapping[i], valueChangedEvents[i]);
            }
        }
        return eventDictionary;
    }
    
    static readonly SettingsKeys[] KeyToIndexMapping =
    {
        SettingsKeys.MasterVolume,
        SettingsKeys.RelativeSounds,

        SettingsKeys.FullScreen,
        SettingsKeys.FrameRate,

        SettingsKeys.ClassicCrosshair,
        SettingsKeys.CrosshairColor
    };
    #endregion

}
public enum SettingsKeys
{
    // Audio
    MasterVolume = DataKeyInt.SettingsVolume,
    RelativeSounds = DataKeyInt.SettingsRelativeSounds,
    
    // Video
    FullScreen = DataKeyInt.SettingsFullScreen,
    FrameRate = DataKeyInt.SettingsFrameRate,
    
    // Gameplay
    ClassicCrosshair = DataKeyInt.SettingsRelativeCrosshair,
    CrosshairColor = DataKeyInt.SettingsCrosshairColor,
}

public enum DataKeyString
{
    Name
}

public enum DataKeyInt
{
    PickedHero,
    GameMode,
    Wins,
    Losses,
    Kills,
    Deaths,
    HighScore,
    SettingsVolume,
    SettingsRelativeSounds,
    SettingsRelativeCrosshair,
    SettingsCrosshairColor,

    SettingsFullScreen,
    SettingsFrameRate
}


public enum GameMode
{
    MultiPlayer,
    Training,
    AltarDefense
}