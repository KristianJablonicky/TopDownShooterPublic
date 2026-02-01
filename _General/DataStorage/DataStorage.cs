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
        string sKey = key.ToString();
        if (stringData.ContainsKey(sKey))
        {
            return stringData[sKey];
        }

        stringData.Add(sKey, PlayerPrefs.GetString(sKey, string.Empty));
        return stringData[sKey];
    }
    public void SetInt(DataKeyInt key, int value)
    {
        string sKey = key.ToString();
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
    public void SetIntHeroSpecific(DataKeyInt key, HeroDatabase? hero, int value)
    {
        if (!hero.HasValue)
        {
            hero = (HeroDatabase)GetInt(DataKeyInt.PickedHero);
        }
        SetInt(GetHeroSpecificKey(key, hero.Value), value);
    }

    public int GetInt(DataKeyInt key)
    {
        var sKey = key.ToString();
        return GetInt(sKey);
    }
    private int GetInt(string key)
    {
        if (intData.ContainsKey(key))
        {
            return intData[key];
        }

        intData.Add(key, PlayerPrefs.GetInt(key, 0));
        return intData[key];
    }

    /// <summary>
    /// Get an int value specific to a hero. If hero is null, use the currently picked hero.
    /// </summary>
    public int GetIntHeroSpecific(DataKeyInt key, HeroDatabase? hero)
    {
        if (!hero.HasValue)
        {
            hero = (HeroDatabase)GetInt(DataKeyInt.PickedHero);
        }
        return GetInt(GetHeroSpecificKey(key, hero.Value));
    }

    public void Increment(DataKeyInt key, int increaseAmount)
    {
        SetInt(key, GetInt(key) + increaseAmount);
    }

    public GameMode GetGameMode() => (GameMode)GetInt(DataKeyInt.GameMode);

    private string GetHeroSpecificKey(DataKeyInt key, HeroDatabase hero)
    {
        return $"{key}_{(int)hero}";
    }

    public static bool IsSinglePlayer => Instance.GetGameMode() == GameMode.SinglePlayer;

    public static float GetVolume()
    {
        return Instance.GetInt(DataKeyInt.SettingsVolume) / 100f * Constants.maxVolume;
    }
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
    SettingsCrosshairColor
}

public enum GameMode
{
    MultiPlayer,
    SinglePlayer
}