using UnityEngine;

public class HeroDatabaseSingleton : SingletonMonoBehaviour<HeroDatabaseSingleton>
{
    [SerializeField] private CharacterToolkit[] heroToolkits;
    public static CharacterToolkit GetHeroToolkit(HeroDatabase hero)
        => Instance.heroToolkits[(int)hero];
}
public enum HeroDatabase
{
    Recruit = 0,
    BabaYaga = 1,
    Dracula = 2,
    Djinn = 3
}