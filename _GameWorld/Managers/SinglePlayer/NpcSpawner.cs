using UnityEngine;

public abstract class NpcSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] protected NpcSpawnConfig spawnConfig;
    [SerializeField] protected NpcDifficultyConfig difficultyConfig;

    public static float CurrentDifficultyValue { get; protected set; } = 0f;

    public void SpawnNpc(Vector2? spawnPoint, CharacterMediator prefabBase)
    {
        if (!spawnPoint.HasValue)
        {
            spawnPoint = spawnConfig.RandomPosition;
        }

        //var npcInstance = Instantiate(prefab, spawnPoint.Value, Quaternion.identity);
        var npcInstance = Instantiate(prefabBase);
        npcInstance.NetworkObject.Spawn();
        npcInstance.MovementController.SetPosition(spawnPoint.Value);

        npcInstance.AiDecisions.SetDifficulty(difficultyConfig);

        OnSpawn(npcInstance);
    }

    protected virtual void OnSpawn(CharacterMediator mediator) { }
}
