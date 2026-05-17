using UnityEngine;

[CreateAssetMenu(fileName = "BotNames", menuName = "Scriptable Objects/Bot Names")]
public class BotName : ScriptableObject
{
    [field: SerializeField] public HeroDatabase Hero { get; private set; }
    [field: SerializeField] public string[] BotNames { get; private set; }
}