using UnityEngine;

public class CommonColors : SingletonMonoBehaviour<CommonColors>
{
    [field: SerializeField] public Color White {  get; private set; }
    [field: SerializeField] public Color Black { get; private set; }
    [field: SerializeField] public Color Orange { get; private set; }
    [field: SerializeField] public Color Cyan { get; private set; }

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
    {
        if (team == 0)
        {
            return Instance.Orange;
        }
        return Instance.Cyan;
    }
}

public enum Colors
{
    White,
    Black,
    Orange,
    Cyan
}