using System.Collections;
using UnityEngine;

public class RespawnManager : SingletonMonoBehaviour<RespawnManager>
{
    private bool gameInProgress = false;
    private bool singlePlayer;
    [SerializeField] private Transform[] spawnPointsUpper, spawnPointsLower;
    private void Start()
    {
        GameStateManager.Instance.GameStarted += () => gameInProgress = true;
        singlePlayer = DataStorage.IsSinglePlayer;
    }
    public void RequestRespawn(CharacterMediator mediator)
    {
        StartCoroutine(WarmUpRespawnCoroutine(mediator));
    }

    private IEnumerator WarmUpRespawnCoroutine(CharacterMediator mediator)
    {
        if (gameInProgress) yield break;
        yield return new WaitForSeconds(3f);
        if (gameInProgress) yield break;

        if (mediator == null) yield break;

        mediator.Reset();


        if (!mediator.IsLocalPlayer) yield break;

        Vector2 respawnPos;
        if (singlePlayer)
        {
            var radius = Random.Range(2f, 5f);
            respawnPos = mediator.GetPosition() +
                Random.insideUnitCircle *
                radius;
        }
        else
        {
            respawnPos = GetSpawnPoint(mediator.PlayerId);
        }

        mediator.MovementController.SetPosition(respawnPos);
    }

    public Vector2 GetSpawnPoint(ulong id)
    {
        var spawnPoints = id % 2 == 0 ? spawnPointsUpper : spawnPointsLower;
        return spawnPoints[Random.Range(0, spawnPoints.Length)].transform.position;
    }
}
