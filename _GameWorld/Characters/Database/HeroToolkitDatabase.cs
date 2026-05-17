using UnityEngine;

[CreateAssetMenu(fileName = "HeroToolkitDatabase", menuName = "Scriptable Objects/HeroToolkitDatabase")]
public class HeroToolkitDatabase : ScriptableObject
{
    [field: SerializeField] public CharacterToolkit[] HeroToolkits { get; private set; }
}
