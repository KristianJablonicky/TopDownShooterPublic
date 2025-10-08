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
        PlayerPrefs.SetInt(sKey, value);
        if (!intData.ContainsKey(sKey))
        {
            intData.Add(sKey, value);
        }
        else
        {
            intData[sKey] = value;
        }
    }
    public int GetInt(DataKeyInt key)
    {
        string sKey = key.ToString();
        if (intData.ContainsKey(sKey))
        {
            return intData[sKey];
        }

        intData.Add(sKey, PlayerPrefs.GetInt(sKey, 0));
        return intData[sKey];
    }

    public void Increment(DataKeyInt key, int increaseAmount)
    {
        SetInt(key, GetInt(key) + increaseAmount);
    }
}

public enum DataKeyString
{
    Name
}

public enum DataKeyInt
{
    PickedHero,
    Wins,
    Losses,
    Kills,
    Deaths
}