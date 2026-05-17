using UnityEngine;

[CreateAssetMenu(fileName = "NpcSpawnConfig", menuName = "Scriptable Objects/NpcSpawnConfig")]
public class NpcSpawnConfig : ScriptableObject
{
    [field: SerializeField] public CharacterMediator[] NpcPrefabs { get; private set; }
    [field: SerializeField] public Vector2[] SpawnPoints { get; private set; }

    public CharacterMediator RandomNpc => NpcPrefabs[Random.Range(0, NpcPrefabs.Length)];
    public Vector2 RandomPosition => SpawnPoints[Random.Range(0, SpawnPoints.Length)];
}
