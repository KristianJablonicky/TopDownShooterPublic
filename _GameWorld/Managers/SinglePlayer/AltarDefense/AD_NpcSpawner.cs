using System.Collections;
using UnityEngine;

public class AD_NpcSpawner : NpcSpawner
{
    [Header("Settings")]
    [SerializeField] private float timeBetweenSpawns = 5f;
    protected override void OnSpawn(CharacterMediator mediator)
    {
        CurrentDifficultyValue += difficultyConfig.DifficultyStep;
    }
    public void StartSpawning()
    {
        StartCoroutine(SpawnCoroutine());
    }

    private IEnumerator SpawnCoroutine()
    {
        while (true)
        {
            SpawnNpc(null, spawnConfig.RandomNpc);
            yield return new WaitForSeconds(timeBetweenSpawns);
        }
    }
}
