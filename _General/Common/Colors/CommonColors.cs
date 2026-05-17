using UnityEngine;

public class CommonColors : SingletonMonoBehaviour<CommonColors>
{
    [field: SerializeField] public Color White {  get; private set; }
    [field: SerializeField] public Color Black { get; private set; }
    [field: SerializeField] public Color Orange { get; private set; }
    [field: SerializeField] public Color Cyan { get; private set; }
    [field: SerializeField] public Color LightOrange { get; private set; }
    [field: SerializeField] public Color LightCyan { get; private set; }
    [field: SerializeField] public Color BotNameColor { get; private set; } = Color.gray;

    public Color GetColor(Colors color)
    {
        return color switch
        {
            Colors.White => White,
            Colors.Black => Black,
            Colors.Orange => Orange,
            Colors.Cyan => Cyan,
            _ => Color.magenta,
        };
    }
    public static Color GetTeamColor(int team)
        => team == 0 ? Instance.Orange : Instance.Cyan;

    public static Color GetTeamColor(Team team) => GetTeamColor((int)team);

    public static Color GetTeamColorLight(int team)
        => team == 0 ? Instance.LightOrange : Instance.LightCyan;

    public static Color GetTeamColorLight(Team team) => GetTeamColorLight((int)team);
}

public enum Colors
{
    White,
    Black,
    Orange,
    Cyan
}